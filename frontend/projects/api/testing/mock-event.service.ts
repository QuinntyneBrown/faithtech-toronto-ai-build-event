import { Injectable } from '@angular/core';
import { EventDetail, EventInput, EventFailure, EventSummary, IEventService } from '@faithtech/api';

@Injectable()
export class MockEventService implements IEventService {
  async getLogo(id: string) {
    const image = await this.call<{bytes: number[]; mediaType: string}>('getLogo', { id });
    return new Blob([new Uint8Array(image.bytes)], { type: image.mediaType });
  }
  async uploadLogo(id: string, file: File, version: string, operationId: string) {
    return this.call<EventDetail>('uploadLogo', { id, version, operationId, name: file.name, mediaType: file.type,
      bytes: Array.from(new Uint8Array(await file.arrayBuffer())) });
  }
  list() { return this.call<EventSummary[]>('list'); }
  get(id: string) { return this.call<EventDetail>('get', { id }); }
  createDraft(title: string, operationId: string) { return this.call<EventSummary>('create', { title, operationId }); }
  saveDraft(id: string, input: EventInput, version: string, operationId: string) {
    return this.call<EventDetail>('save', { id, input, version, operationId });
  }
  private async call<T>(operation: string, args: object = {}): Promise<T> {
    const bridge = window as unknown as { __faithtechEvents(operation: string, args: object): Promise<{
      result: T; status?: number; failed?: boolean; code?: string; errors?: Record<string, string[]>; current?: EventDetail;
    }> };
    const response = await bridge.__faithtechEvents(operation, args);
    if (response.failed || response.status) throw new EventFailure(response.status ?? 0, response.code, response.errors, response.current);
    return response.result;
  }
}
