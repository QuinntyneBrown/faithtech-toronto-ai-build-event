export class ParticipantSessionFixture {
  unavailable = false;
  authenticated = false;
  participantId = '00000000-0000-0000-0000-000000000002';
  eventId = '';
  lifetime = 24 * 60 * 60000;
  expires = 0;
  handle(operation: string, args: { eventId?: string } = {}) {
    if (this.unavailable) return { status: 503 };
    if (operation === 'signout') { this.authenticated = false; return { result: null }; }
    return {
      state: this.authenticated && this.expires > Date.now() ? {
        participantId: this.participantId, eventId: this.eventId || args.eventId,
        serverNow: new Date().toISOString(), absoluteExpiresAtUtc: new Date(this.expires).toISOString(),
      } : null,
    };
  }
}
