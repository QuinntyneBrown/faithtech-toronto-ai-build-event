import { EventState } from '../models/event-state';
import { SCREENS } from '../models/screen';
import { EventCommand } from './event-command';
import { seed } from './seed';
import { email, participantFields, projectFields, text } from './validation';

export function publicName(p: { name: string; label: string }): string { return p.name ? `${p.name} · ${p.label}` : p.label; }
function randomIndex(length: number): number {
  const range = 0x100000000, limit = range - range % length;
  let value: number; do { value = crypto.getRandomValues(new Uint32Array(1))[0]; } while (value >= limit);
  return value % length;
}
export function applyCommand(state: EventState, command: EventCommand): EventState {
  if (command.type === 'reset') return { ...seed(command.populated), revision: state.revision + 1 };
  const next = structuredClone(state);
  const label = () => 'Participant ' + String(next.nextLabel++).padStart(3, '0');
  switch (command.type) {
    case 'enter': {
      if (next.stage !== 'countdown') throw new Error('Entry is closed. Ask an administrator for help.');
      const address = email(command.email), existing = next.participants.find(p => p.email === address);
      if (existing && existing.id !== command.id) throw new Error('This email is already entered. Ask an administrator to update your details.');
      if (!existing) next.participants.push({ id: command.id, label: label(), email: address, name: '', makes: '', heart: '', teamId: null });
      break;
    }
    case 'profile': {
      if (next.stage !== 'countdown') throw new Error('Personal entry is now closed. Ask an administrator to make changes.');
      const p = next.participants.find(p => p.id === command.id);
      if (!p) throw new Error('Your entry is no longer available.');
      Object.assign(p, { name: text(command.name, 200), makes: text(command.makes, 2000), heart: text(command.heart, 2000) }); break;
    }
    case 'participantSave': {
      const p = participantFields(command.participant), existing = next.participants.find(item => item.id === p.id);
      if (next.participants.some(item => item.email === p.email && item.id !== p.id)) throw new Error('That email is already entered.');
      if (existing) Object.assign(existing, { ...p, label: existing.label, teamId: existing.teamId });
      else next.participants.push({ ...p, label: label(), teamId: null }); break;
    }
    case 'participantDelete': {
      const p = next.participants.find(p => p.id === command.id);
      if (!p) throw new Error('This participant has already been removed.');
      next.participants = next.participants.filter(p => p.id !== command.id);
      next.draws.forEach(d => { d.candidates = d.candidates.filter(name => name !== p.label && !name.endsWith(' · ' + p.label)); if (d.winnerId === command.id) d.label = 'Removed participant'; }); break;
    }
    case 'projectSave': {
      const p = projectFields(command.project), i = next.projects.findIndex(item => item.id === p.id);
      if (i === -1) next.projects.push(p); else next.projects[i] = p; break;
    }
    case 'projectDelete':
      next.projects = next.projects.filter(p => p.id !== command.id);
      next.teams.forEach(t => { if (t.projectId === command.id) t.projectId = null; }); break;
    case 'advance': {
      const index = SCREENS.indexOf(next.stage);
      if (index === 3) throw new Error('Raffle is the final screen.');
      next.stage = SCREENS[index + 1];
      if (next.stage === 'teams') {
        const people = [...next.participants];
        for (let i = people.length - 1; i > 0; i--) { const j = randomIndex(i + 1); [people[i], people[j]] = [people[j], people[i]]; }
        for (let i = 0; i < people.length; i++) {
          if (i % 3 === 0) next.teams.push({ id: crypto.randomUUID(), name: 'Team ' + (next.teams.length + 1), projectId: null });
          people[i].teamId = next.teams[next.teams.length - 1].id;
        }
      } break;
    }
    case 'move': {
      const p = next.participants.find(p => p.id === command.participantId);
      if (!p) throw new Error('This participant is no longer on the roster.');
      if (command.teamId === 'new') { const team = { id: crypto.randomUUID(), name: 'Team ' + (next.teams.length + 1), projectId: null }; next.teams.push(team); p.teamId = team.id; }
      else { if (command.teamId && !next.teams.some(t => t.id === command.teamId)) throw new Error('That team no longer exists.'); p.teamId = command.teamId; } break;
    }
    case 'assign': {
      const t = next.teams.find(t => t.id === command.teamId);
      if (!t || (command.projectId && !next.projects.some(p => p.id === command.projectId))) throw new Error('That team or project is no longer available.');
      t.projectId = command.projectId; break;
    }
    case 'draw': {
      if (next.stage !== 'raffle') throw new Error('Open Raffle before drawing a name.');
      if (next.draws.some(d => Date.now() < d.reveal + 5000)) throw new Error('Let this celebration finish before drawing again.');
      const pool = next.participants.filter(p => !next.draws.some(d => d.winnerId === p.id));
      if (!pool.length) throw new Error('No eligible participants remain.');
      const winner = pool[randomIndex(pool.length)], start = Date.now();
      next.draws.push({ id: crypto.randomUUID(), winnerId: winner.id, label: publicName(winner), candidates: pool.map(publicName), start, reveal: start + 5000 }); break;
    }
    case 'restartCountdown': next.target = Date.now() + 20 * 60_000; break;
  }
  next.revision++;
  return next;
}
