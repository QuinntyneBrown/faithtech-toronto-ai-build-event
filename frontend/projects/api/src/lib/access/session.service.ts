import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ISessionService } from './session-service.contract';
import { SessionState } from './session-state';
import { ServiceFailure } from './service-failure';

@Injectable()
export class SessionService implements ISessionService {
  private readonly http = inject(HttpClient);
  private readonly scope = '/api/admin';

  async read(): Promise<SessionState | null> {
    try { return await firstValueFrom(this.http.get<SessionState>(`${this.scope}/session`)); }
    catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) return null;
      throw this.failure(error);
    }
  }
  async signIn(username: string, password: string): Promise<SessionState> {
    await this.mutate('POST', '/session', { username, password });
    const state = await this.read();
    if (!state) throw new ServiceFailure(401);
    return state;
  }
  signOut(): Promise<void> { return this.mutate('DELETE', '/session'); }
  interact(): Promise<void> { return this.mutate('POST', '/session/interaction'); }

  private async mutate(method: string, path: string, body?: unknown): Promise<void> {
    try {
      const token = await firstValueFrom(this.http.get<{requestToken: string}>(`${this.scope}/antiforgery`));
      await firstValueFrom(this.http.request(method, `${this.scope}${path}`, {
        body, headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': crypto.randomUUID() },
      }));
    } catch (error) { throw this.failure(error); }
  }
  private failure(error: unknown): ServiceFailure {
    return error instanceof HttpErrorResponse
      ? new ServiceFailure(error.status, Number(error.headers.get('Retry-After') ?? 0))
      : new ServiceFailure(0);
  }
}
