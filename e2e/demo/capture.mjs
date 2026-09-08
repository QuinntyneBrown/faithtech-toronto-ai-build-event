import { chromium, expect } from '@playwright/test';
import { mkdir, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { speech, mux } from './media.mjs';
import { timeline } from './timeline.mjs';

export async function record(slug, directory, lines, story, { baseURL } = {}) {
  const folder = resolve(directory, slug);
  await mkdir(folder, { recursive: true });
  const clips = await speech(lines, folder);
  const browser = await chromium.launch({ channel: 'chrome', headless: true });
  let context, page, timer;
  let cues = [];
  try {
    context = await browser.newContext({ baseURL, ignoreHTTPSErrors: true,
      viewport: { width: 1280, height: 720 }, recordVideo: { dir: folder, size: { width: 1280, height: 720 } } });
    context.setDefaultTimeout(15000); context.setDefaultNavigationTimeout(30000);
    page = await context.newPage();
    // Windows ARM can start the bundled video encoder after the first page frame.
    // Warm it before the story; the final export begins at the first encoded cue.
    await page.setContent('<p>Preparing the local recording.</p>');
    await page.waitForTimeout(45000);
    const start = performance.now();
    timer = setTimeout(() => browser.close(), 12 * 60 * 1000);
    const say = async (index, chapter) => {
      const clip = clips[index];
      await page.evaluate(({ text, index }) => {
        document.getElementById('demo-caption')?.remove();
        const el = document.createElement('div'); el.id = 'demo-caption'; el.textContent = text;
        el.style.cssText = 'position:fixed;bottom:18px;left:8%;width:84%;box-sizing:border-box;padding:14px 22px;background:#142b2f;color:white;font:22px/1.4 system-ui;border-radius:12px;box-shadow:0 3px 20px #0003;z-index:2147483647;pointer-events:none;text-align:center';
        // Popovers remain readable above native modal dialogs, without taking focus.
        el.setAttribute('popover', 'manual'); document.body.append(el); el.showPopover();
        el.style.margin = '0'; el.style.top = 'auto'; el.style.border = '0';
        const marker = document.createElement('span'); marker.id = 'demo-marker'; marker.setAttribute('popover', 'manual');
        marker.style.cssText = `position:fixed;left:0;top:0;right:auto;bottom:auto;width:16px;height:16px;padding:0;margin:0;border:0;pointer-events:none;background:rgb(${20 + index * 12},10,200)`;
        document.body.append(marker); marker.showPopover();
      }, { text: clip.text, index });
      const cue = { ...clip, chapter, start: (performance.now() - start) / 1000 };
      cue.end = cue.start + clip.duration; cues.push(cue);
      await page.waitForTimeout((clip.duration + 1.5) * 1000);
      await page.evaluate(() => { document.getElementById('demo-caption')?.remove(); document.getElementById('demo-marker')?.remove(); });
    };
    await story({ page, context, say, expect, folder });
    await page.waitForTimeout(1500);
    const video = page.video();
    await context.close(); context = null;
    const raw = resolve(folder, 'raw.webm'); await video.saveAs(raw);
    cues = await timeline(raw, cues);
    const offset = cues[0].start;
    cues = cues.map(cue => ({ ...cue, start: cue.start - offset, end: cue.end - offset, visibleEnd: cue.visibleEnd - offset }));
    const metadata = await mux(raw, cues, resolve(folder, `${slug}.webm`), offset);
    const result = { slug, cues: cues.map(({ path, ...cue }) => cue),
      duration: Number(metadata.format.duration), size: Number(metadata.format.size),
      streams: metadata.streams.map(({ codec_name, codec_type, width, height }) => ({ codec_name, codec_type, width, height })) };
    await writeFile(resolve(folder, `${slug}-chapters.json`), JSON.stringify(result, null, 2));
    return result;
  } catch (error) {
    if (page && !page.isClosed()) {
      await page.screenshot({ path: resolve(folder, 'failure.png'), fullPage: true }).catch(() => {});
      await writeFile(resolve(folder, 'failure.txt'), `${error.stack}\n${await page.locator('body').innerText().catch(() => '')}`);
    }
    throw error;
  } finally {
    clearTimeout(timer);
    await context?.close(); await browser.close();
  }
}
