import { EventFixture } from './event-fixture';
import { ParticipantSessionFixture } from './participant-session-fixture';

export interface RegistrationFixtureEntry {
  id: string;
  code: string;
  email: string | null;
  active: boolean;
}

const KNOWN_SUFFIXES = ['', 'schedule', 'teams', 'people', 'messages', 'quiz', 'raffle', 'showcase'];

export class EntryFixture {
  unavailable = false;
  readonly registrations = new Map<string, RegistrationFixtureEntry[]>();
  constructor(private readonly events: EventFixture, private readonly sessions: ParticipantSessionFixture) {}
  handle(operation: string, args: { eventId: string; email?: string; entryCode?: string; returnTo?: string }) {
    if (this.unavailable) return { status: 503 };
    const event = this.events.events.get(args.eventId);
    if (operation === 'header') {
      if (!event || !event.published) return { status: 404 };
      return { result: { eventId: event.id, title: event.title } };
    }
    if (operation === 'authenticate') {
      if (!event || !event.published) return { status: 401 };
      const email = (args.email ?? '').trim();
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return { status: 422, errors: { email: ['Enter a valid email address.'] } };
      const normalized = email.toUpperCase();
      const entries = this.registrations.get(args.eventId) ?? [];
      const entry = entries.find(x => x.code === args.entryCode && x.active);
      if (!entry) return { status: 401 };
      if (entry.email && entry.email !== normalized) return { status: 401 };
      if (!entry.email && entries.some(x => x.id !== entry.id && x.active && x.email === normalized)) return { status: 401 };
      entry.email = normalized;
      this.sessions.signIn(entry.id, args.eventId);
      return {
        result: {
          participantId: entry.id, eventId: args.eventId,
          absoluteExpiresAtUtc: new Date(Date.now() + 24 * 60 * 60000).toISOString(),
          authorizedInitialRoute: this.authorizedInitialRoute(args.eventId, event, args.returnTo),
        },
      };
    }
    return { status: 404 };
  }
  private authorizedInitialRoute(eventId: string, event: NonNullable<ReturnType<EventFixture['events']['get']>>, returnTo?: string) {
    const basePath = `/events/${eventId}`;
    const authorizedDefault = event.endsAtUtc && Date.now() >= Date.parse(event.endsAtUtc) ? `${basePath}/showcase` : basePath;
    if (!returnTo || returnTo[0] !== '/' || returnTo.startsWith('//') || !returnTo.startsWith(basePath)) return authorizedDefault;
    const remainder = returnTo.slice(basePath.length).replace(/^\/+|\/+$/g, '');
    return KNOWN_SUFFIXES.includes(remainder) ? returnTo : authorizedDefault;
  }
}
