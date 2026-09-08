import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { IEntryService } from './entry-service.contract';
import { EntryHeader } from './entry-header';
import { EntryResult } from './entry-result';
import { EntryFailure } from './entry-failure';

@Injectable()
export class EntryService implements IEntryService {
  private readonly http = inject(HttpClient);

  async header(eventId: string): Promise<EntryHeader | null> {
    try { return await firstValueFrom(this.http.get<EntryHeader>(`/api/events/${encodeURIComponent(eventId)}/entry`)); }
    catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 404) return null;
      throw this.failure(error);
    }
  }

  async authenticate(eventId: string, email: string, entryCode: string, returnTo?: string): Promise<EntryResult> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>(`/api/events/${encodeURIComponent(eventId)}/antiforgery`));
      return await firstValueFrom(this.http.post<EntryResult>(`/api/events/${encodeURIComponent(eventId)}/session`,
        { email, entryCode, returnTo }, { headers: { 'X-CSRF-TOKEN': token.requestToken } }));
    } catch (error) { throw this.failure(error); }
  }

  private failure(error: unknown): EntryFailure {
    return error instanceof HttpErrorResponse
      ? new EntryFailure(error.status, error.error?.code, error.error?.errors, Number(error.headers.get('Retry-After') ?? 0))
      : new EntryFailure(0);
  }
}
