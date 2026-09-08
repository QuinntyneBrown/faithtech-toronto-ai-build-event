import { Injectable } from '@angular/core';
import { IParticipantSessionService, ParticipantSessionState, ServiceFailure } from '@faithtech/api';

@Injectable()
export class MockParticipantSessionService implements IParticipantSessionService {
  read(eventId: string) { return this.call('read', { eventId }); }
  async signOut(eventId: string) { await this.call('signout', { eventId }); }
  private async call(operation: string, args: { eventId: string }): Promise<ParticipantSessionState | null> {
    const bridge = window as unknown as {
      __faithtechParticipantSession(operation: string, args: object): Promise<{ state?: ParticipantSessionState | null; status?: number }>;
    };
    const response = await bridge.__faithtechParticipantSession(operation, args);
    if (response.status) throw new ServiceFailure(response.status);
    return response.state ?? null;
  }
}
