import { EventState } from '../models/event-state';
export function seed(populated = true): EventState {
  const names = ['Alex Chen', 'Amara Okafor', 'Ben Park', 'Chloe Martin', 'Daniel Reid', 'Elena Costa', 'Ezra Brooks', 'Grace Kim', 'Isaac Moore', 'Leah Patel', 'Micah Jones', 'Naomi Silva', 'Sam Taylor'];
  return { schema: 1, revision: 0, generation: crypto.randomUUID(), stage: 'countdown', target: Date.now() + 20 * 60_000, nextLabel: populated ? 14 : 1,
    participants: populated ? names.map((name, i) => ({ id: 'person-' + (i + 1), label: 'Participant ' + String(i + 1).padStart(3, '0'), name, email: 'attendee' + (i + 1) + '@example.com', makes: ['Web experiences', 'Thoughtful design', 'Community connections'][i % 3], heart: ['Helping people feel less alone.', 'Making room for people to belong.', 'Technology that serves our neighbours.'][i % 3], teamId: null })) : [],
    projects: [{ id: 'rtr', title: 'Reconciliation Through Relationships', description: 'Relationships make reconciliation real. Continue the July hackathon project with rightrelationship.ca: a shared learning journey, facilitator-reviewed matching with mutual consent, and ways for people to stay connected. Choose one small improvement your team can bring to life tonight.', repository: '', demo: '' }],
    teams: [], draws: [] };
}
