import { Injectable } from '@angular/core';
import { ISessionService, SessionState, ServiceFailure } from '@faithtech/api';

@Injectable()
export class MockSessionService implements ISessionService {
  read() { return this.call('read'); }
  async signIn(username: string, password: string): Promise<SessionState> {
    const state = await this.call('signin', { username, password });
    if (!state) throw new ServiceFailure(401);
    return state;
  }
  async signOut() { await this.call('signout'); }
  async interact() { await this.call('interact'); }
  private async call(operation: string, credentials?: {username: string; password: string}): Promise<SessionState | null> {
    const bridge = window as unknown as {
      __faithtechSession(operation: string, credentials?: object): Promise<{state?: SessionState | null; status?: number}>;
    };
    const response = await bridge.__faithtechSession(operation, credentials);
    if (response.status) throw new ServiceFailure(response.status);
    return response.state ?? null;
  }
}
