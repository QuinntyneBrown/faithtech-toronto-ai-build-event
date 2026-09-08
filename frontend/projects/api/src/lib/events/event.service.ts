import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { EventSummary } from './event-summary';
import { IEventService } from './event-service.contract';
import { ServiceFailure } from '../access/service-failure';

@Injectable()
export class EventService implements IEventService {
  private readonly http = inject(HttpClient);
  async list(): Promise<EventSummary[]> {
    try { return await firstValueFrom(this.http.get<EventSummary[]>('/api/admin/events')); }
    catch (error) { throw new ServiceFailure(error instanceof HttpErrorResponse ? error.status : 0); }
  }
  async createDraft(title: string, operationId: string): Promise<EventSummary> {
    try {
      const token = await firstValueFrom(this.http.get<{requestToken: string}>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.post<EventSummary>('/api/admin/events', { title }, {
        headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId },
      }));
    } catch (error) { throw new ServiceFailure(error instanceof HttpErrorResponse ? error.status : 0); }
  }
}
