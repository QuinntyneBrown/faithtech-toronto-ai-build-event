import { spawn } from 'node:child_process';
import { writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';

export function run(command, args, { timeout = 120000, input, env = process.env, onOutput, cwd } = {}) {
  return new Promise((accept, reject) => {
    const child = spawn(command, args, { windowsHide: true, env, cwd, stdio: ['pipe', 'pipe', 'pipe'] });
    let output = '', expired = false;
    const timer = setTimeout(() => { expired = true; child.kill(); }, timeout);
    const collect = data => { const text = data.toString(); output += text; onOutput?.(text); };
    child.stdout.on('data', collect); child.stderr.on('data', collect);
    child.on('error', error => { clearTimeout(timer); reject(error); });
    child.on('close', code => {
      clearTimeout(timer);
      if (expired) reject(new Error(`${command}: deadline exceeded`));
      else if (code !== 0) reject(new Error(`${command}: exit ${code}\n${output}`));
      else accept(output);
    });
    child.stdin.on('error', () => {});
    child.stdin.end(input);
  });
}
export const stamp = seconds => new Date(Math.round(seconds * 1000)).toISOString().slice(11, 23);
export const captions = cues => 'WEBVTT\n\n' + cues.map(c => `${stamp(c.start)} --> ${stamp(c.end)}\n${c.text}\n`).join('\n');
export const probe = async path => JSON.parse(await run(process.env.FFPROBE || 'ffprobe',
  ['-v', 'error', '-show_format', '-show_streams', '-of', 'json', path]));

export async function speech(lines, directory) {
  const manifest = resolve(directory, 'speech.json');
  await writeFile(manifest, JSON.stringify(lines));
  await run('powershell.exe', ['-NoProfile', '-File', resolve('e2e/demo/speech.ps1'), manifest, directory]);
  return Promise.all(lines.map(async (text, i) => {
    const path = resolve(directory, `${i}.wav`);
    return { text, path, duration: Number((await probe(path)).format.duration) };
  }));
}

export async function mux(raw, cues, target, offset = 0) {
  const args = ['-y', '-ss', String(offset), '-i', raw];
  for (const cue of cues) args.push('-i', cue.path);
  const filters = cues.map((c, i) => `[${i + 1}:a]adelay=${Math.round(c.start * 1000)}:all=1[a${i}]`);
  filters.push(cues.map((_, i) => `[a${i}]`).join('') + `amix=inputs=${cues.length}:normalize=0,apad[audio]`);
  args.push('-filter_complex', filters.join(';'), '-map', '0:v', '-map', '[audio]', '-c:v', 'libvpx',
    '-vf', 'drawbox=x=0:y=0:w=16:h=16:color=0xfffdf4:t=fill',
    '-deadline', 'realtime', '-cpu-used', '6', '-crf', '12', '-b:v', '2M',
    '-c:a', 'libopus', '-b:a', '96k', '-shortest', target);
  await run(process.env.FFMPEG || 'ffmpeg', args);
  await writeFile(target.replace('.webm', '.vtt'), captions(cues));
  const metadata = await probe(target);
  if (!metadata.streams.some(s => s.codec_type === 'audio')) throw new Error('Narration missing');
  return metadata;
}
