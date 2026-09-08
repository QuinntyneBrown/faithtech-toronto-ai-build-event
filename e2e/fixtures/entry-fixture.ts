import { EventFixture } from './event-fixture';

export class EntryFixture {
  unavailable = false;
  constructor(private readonly events: EventFixture) {}
  handle(operation: string, args: { eventId: string }) {
    if (this.unavailable) return { status: 503 };
    if (operation === 'header') {
      const event = this.events.events.get(args.eventId);
      if (!event || !event.published) return { status: 404 };
      return { result: { eventId: event.id, title: event.title } };
    }
    return { status: 404 };
  }
}
