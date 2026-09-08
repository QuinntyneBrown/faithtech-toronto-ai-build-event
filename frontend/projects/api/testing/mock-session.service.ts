import { Injectable } from '@angular/core';
import { ISessionService, SessionState, ServiceFailure } from '@faithtech/api';

@Injectable()
export class MockSessionService implements ISessionService {
  private state: SessionState | null = null;
  async read() { return this.state; }
  async signIn(username: string, password: string): Promise<SessionState> {
    if (username !== 'host@example.com' || password !== 'host-demo') throw new ServiceFailure(401);
    const now = Date.now();
    this.state = { actorId: '00000000-0000-0000-0000-000000000001', serverNow: new Date(now).toISOString(),
      absoluteExpiresAtUtc: new Date(now + 8 * 3600000).toISOString(), idleExpiresAtUtc: new Date(now + 30 * 60000).toISOString() };
    return this.state;
  }
  async signOut() { this.state = null; }
  async interact() {
    if (!this.state) throw new ServiceFailure(401);
    this.state = { ...this.state, serverNow: new Date().toISOString(), idleExpiresAtUtc: new Date(Date.now() + 30 * 60000).toISOString() };
  }
}
