import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { EventSummary } from './event-summary';
import { EventDetail } from './event-detail';
import { EventInput } from './event-input';
import { EventFailure } from './event-failure';
import { IEventService } from './event-service.contract';
import { ServiceFailure } from '../access/service-failure';

@Injectable()
export class EventService implements IEventService {
  private readonly http = inject(HttpClient);
  async getLogo(id: string): Promise<Blob> {
    try { return await firstValueFrom(this.http.get(`/api/admin/events/${encodeURIComponent(id)}/logo`, { responseType: 'blob' })); }
    catch (error) { throw new ServiceFailure(error instanceof HttpErrorResponse ? error.status : 0); }
  }
  async uploadLogo(id: string, file: File, version: string, operationId: string): Promise<EventDetail> {
    const body = new FormData(); body.append('file', file);
    try {
      const token = await firstValueFrom(this.http.get<{requestToken: string}>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.post<EventDetail>(`/api/admin/events/${encodeURIComponent(id)}/logo`, body, {
        headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId, 'If-Match': `"${version}"` },
      }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new EventFailure(error.status, error.error?.code, error.error?.errors, error.error?.current);
      throw new EventFailure(0);
    }
  }
  async saveDraft(id: string, input: EventInput, version: string, operationId: string): Promise<EventDetail> {
    try {
      const token = await firstValueFrom(this.http.get<{requestToken: string}>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.put<EventDetail>(`/api/admin/events/${encodeURIComponent(id)}`, input, {
        headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId, 'If-Match': `"${version}"` },
      }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new EventFailure(error.status, error.error?.code, error.error?.errors, error.error?.current);
      throw new EventFailure(0);
    }
  }
  async get(id: string): Promise<EventDetail> {
    try { return await firstValueFrom(this.http.get<EventDetail>(`/api/admin/events/${encodeURIComponent(id)}`)); }
    catch (error) { throw new ServiceFailure(error instanceof HttpErrorResponse ? error.status : 0); }
  }
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
