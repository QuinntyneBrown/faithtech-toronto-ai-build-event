import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { IParticipantSessionService } from './participant-session-service.contract';
import { ParticipantSessionState } from './participant-session-state';
import { ServiceFailure } from '../access/service-failure';

@Injectable()
export class ParticipantSessionService implements IParticipantSessionService {
  private readonly http = inject(HttpClient);

  async read(eventId: string): Promise<ParticipantSessionState | null> {
    try { return await firstValueFrom(this.http.get<ParticipantSessionState>(`/api/events/${encodeURIComponent(eventId)}/session`)); }
    catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) return null;
      throw this.failure(error);
    }
  }

  async signOut(eventId: string): Promise<void> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>(`/api/events/${encodeURIComponent(eventId)}/antiforgery`));
      await firstValueFrom(this.http.delete(`/api/events/${encodeURIComponent(eventId)}/session`,
        { headers: { 'X-CSRF-TOKEN': token.requestToken } }));
    } catch (error) { throw this.failure(error); }
  }

  private failure(error: unknown): ServiceFailure {
    return error instanceof HttpErrorResponse ? new ServiceFailure(error.status) : new ServiceFailure(0);
  }
}
