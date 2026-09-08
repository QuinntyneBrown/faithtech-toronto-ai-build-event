import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { IRosterService } from './roster-service.contract';
import { RegistrationInput } from './registration-input';
import { RosterEntry } from './roster-entry';
import { RosterIssuance } from './roster-issuance';
import { RosterFailure } from './roster-failure';

@Injectable()
export class RosterService implements IRosterService {
  private readonly http = inject(HttpClient);
  async list(eventId: string): Promise<RosterEntry[]> {
    try { return await firstValueFrom(this.http.get<RosterEntry[]>(`/api/admin/events/${encodeURIComponent(eventId)}/roster`)); }
    catch (error) { throw new RosterFailure(error instanceof HttpErrorResponse ? error.status : 0); }
  }
  async add(eventId: string, input: RegistrationInput, operationId: string): Promise<RosterIssuance> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.post<RosterIssuance>(`/api/admin/events/${encodeURIComponent(eventId)}/roster`, input,
        { headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId } }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new RosterFailure(error.status, error.error?.code, error.error?.errors);
      throw new RosterFailure(0);
    }
  }
  async rename(eventId: string, registrationId: string, displayName: string, version: string, operationId: string): Promise<RosterEntry> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.put<RosterEntry>(
        `/api/admin/events/${encodeURIComponent(eventId)}/roster/${encodeURIComponent(registrationId)}/name`, { displayName },
        { headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId, 'If-Match': `"${version}"` } }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new RosterFailure(error.status, error.error?.code, error.error?.errors);
      throw new RosterFailure(0);
    }
  }
  async deactivate(eventId: string, registrationId: string, version: string, operationId: string): Promise<RosterEntry> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.post<RosterEntry>(
        `/api/admin/events/${encodeURIComponent(eventId)}/roster/${encodeURIComponent(registrationId)}/deactivate`, null,
        { headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId, 'If-Match': `"${version}"` } }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new RosterFailure(error.status, error.error?.code, error.error?.errors);
      throw new RosterFailure(0);
    }
  }
  async replaceCode(eventId: string, registrationId: string, version: string, operationId: string): Promise<RosterIssuance> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.post<RosterIssuance>(
        `/api/admin/events/${encodeURIComponent(eventId)}/roster/${encodeURIComponent(registrationId)}/code`, { clearEmailBinding: false },
        { headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId, 'If-Match': `"${version}"` } }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new RosterFailure(error.status, error.error?.code, error.error?.errors);
      throw new RosterFailure(0);
    }
  }
}
