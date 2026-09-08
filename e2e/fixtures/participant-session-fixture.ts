export class ParticipantSessionFixture {
  unavailable = false;
  authenticated = false;
  participantId = '';
  eventId = '';
  lifetime = 24 * 60 * 60000;
  expires = 0;
  signIn(participantId: string, eventId: string) {
    this.authenticated = true; this.participantId = participantId; this.eventId = eventId;
    this.expires = Date.now() + this.lifetime;
  }
  handle(operation: string, args: { eventId?: string } = {}) {
    if (this.unavailable) return { status: 503 };
    if (operation === 'signout') { this.authenticated = false; return { result: null }; }
    if (args.eventId && this.eventId && args.eventId !== this.eventId) return { state: null };
    return {
      state: this.authenticated && this.expires > Date.now() ? {
        participantId: this.participantId, eventId: this.eventId,
        serverNow: new Date().toISOString(), absoluteExpiresAtUtc: new Date(this.expires).toISOString(),
      } : null,
    };
  }
}
