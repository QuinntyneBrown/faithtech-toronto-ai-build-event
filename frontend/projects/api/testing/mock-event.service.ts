import { Injectable } from '@angular/core';
import { EventSummary, IEventService, ServiceFailure } from '@faithtech/api';

@Injectable()
export class MockEventService implements IEventService {
  private events: EventSummary[] = [];
  private receipts = new Map<string, EventSummary>();
  async list() { return [...this.events]; }
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
