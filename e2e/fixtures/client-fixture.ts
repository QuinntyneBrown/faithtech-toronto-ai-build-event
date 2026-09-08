import { test as base } from '@playwright/test';
import { EventFixture } from './event-fixture';
import { EntryFixture } from './entry-fixture';
import { ParticipantSessionFixture } from './participant-session-fixture';

export const test = base.extend<{ events: EventFixture; entry: EntryFixture; participantSession: ParticipantSessionFixture }>({
  events: [async ({ context }, use) => {
    const events = new EventFixture();
    await context.exposeBinding('__faithtechEvents', (_source, operation, args) => events.handle(operation, args));
    await use(events);
  }, { auto: true }],
  entry: [async ({ context, events }, use) => {
    const entry = new EntryFixture(events);
    await context.exposeBinding('__faithtechEntry', (_source, operation, args) => entry.handle(operation, args));
    await use(entry);
  }, { auto: true }],
  participantSession: [async ({ context }, use) => {
    const participantSession = new ParticipantSessionFixture();
    await context.exposeBinding('__faithtechParticipantSession', (_source, operation, args) => participantSession.handle(operation, args));
    await use(participantSession);
  }, { auto: true }],
});
