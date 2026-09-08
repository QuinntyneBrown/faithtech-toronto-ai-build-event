import { test as base } from '@playwright/test';

export class SessionFixture {
  authenticated = false;
  unavailable = false;
  signOutUnavailable = false;
  lifetime = 30 * 60000;
  expires = 0;
  handle(operation: string, credentials: {username?: string; password?: string} = {}) {
    if (this.unavailable || (operation === 'signout' && this.signOutUnavailable)) return { status: 503 };
    if (operation === 'signin') {
      if (credentials.username !== 'host@example.com' || credentials.password !== 'host-demo') return { status: 401 };
      this.authenticated = true;
      this.expires = Date.now() + this.lifetime;
    }
    if (operation === 'signout') this.authenticated = false;
    if (operation === 'interact' && this.authenticated) this.expires = Date.now() + this.lifetime;
    return { state: this.authenticated && this.expires > Date.now() ? {
      actorId: '00000000-0000-0000-0000-000000000001', serverNow: new Date().toISOString(),
      idleExpiresAtUtc: new Date(this.expires).toISOString(), absoluteExpiresAtUtc: new Date(this.expires).toISOString(),
    } : null };
  }
}

export const test = base.extend<{session: SessionFixture}>({
  session: [async ({ context }, use) => {
    const session = new SessionFixture();
    await context.exposeBinding('__faithtechSession', (_source, operation, credentials) => session.handle(operation, credentials));
    await use(session);
  }, { auto: true }],
});
