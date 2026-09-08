import { Injectable } from '@angular/core';
import { EventDetail, EventInput, EventFailure, EventSummary, IEventService, ServiceFailure } from '@faithtech/api';

@Injectable()
export class MockEventService implements IEventService {
  private events: EventSummary[] = [];
  private receipts = new Map<string, EventSummary>();
  async list() { return [...this.events]; }
  async saveDraft(id: string, input: EventInput, version: string, operationId: string): Promise<EventDetail> {
    const current = await this.get(id);
    if (current.version !== version) throw new EventFailure(409, 'stale-version', {}, current);
    const saved = { ...current, ...input, version: String(Number(version) + 1) };
    this.events = this.events.map(event => event.id === id ? saved : event);
    return saved;
  }
  async get(id: string): Promise<EventDetail> {
    const event = this.events.find(item => item.id === id);
    if (!event) throw new ServiceFailure(404);
    return { venueName: null, address: null, latitude: null, longitude: null, waitingContent: null, closingContent: null, directionsUrl: null, ...event };
  }
  async createDraft(title: string, operationId: string) {
    if ([...title.trim()].length > 200) throw new ServiceFailure(422);
    const previous = this.receipts.get(operationId);
    if (previous) return previous;
    const event = { id: crypto.randomUUID(), title: title.trim() || null, published: false, useLiturgy: false, version: '1' };
    this.events.push(event);
    this.receipts.set(operationId, event);
    return event;
  }
}
