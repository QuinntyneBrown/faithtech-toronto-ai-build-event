import { record } from './capture.mjs';
import { participantFixture } from './api-client.mjs';
import { ClientAccessPage } from '../page-objects/client-access-page.ts';
import { ClientShellPage } from '../page-objects/client-shell-page.ts';

const lines = [
  'Participants enter their event using an email address and an individual entry code. A protected link first brings an unauthenticated visitor to this entrance.',
  'This event is a published demo fixture in disposable storage. Publication itself is not implemented yet. All sign in requests still use the real authentication service.',
  'An incorrect code is rejected without revealing other participants. The form gives feedback and allows another attempt. Entry codes are concealed in this recording.',
  'Alex is now signed in. On first use, the entry code is bound to the supplied email address. The event shell confirms that access was granted.',
  'Refreshing the page preserves the participant session. The server verifies access again before the protected page is shown.',
  'The navigation includes planned activities, but these screens are explicitly unfinished. This revision does not yet provide the live countdown, team selection, messaging, quizzes, or raffle.',
  'Signing out ends this session. Opening the protected event again returns to the entrance, ready for another authenticated visit.',
];
export async function client(directory, environment) {
  const fixture = await participantFixture(environment);
  return record('client', directory, lines, async ({ page, say, expect }) => {
    const access = new ClientAccessPage(page), shell = new ClientShellPage(page);
    await page.addInitScript(() => document.addEventListener('DOMContentLoaded', () => {
      const style = document.createElement('style'); style.textContent = '#participant-code{-webkit-text-security:disc!important}'; document.head.append(style);
    }, { once: true }));
    await page.goto(`/events/${fixture.id}`); await expect(page).toHaveURL(/\/access/);
    await access.expectTitle('Toronto AI Build Event — demo fixture'); await say(0, 'Protected event entrance'); await say(1, 'Real authentication, seeded publication');
    await access.signIn('alex@example.com', 'invalid-demo-code'); await access.expectDenied(); await say(2, 'Rejected code');
    await access.signIn('alex@example.com', fixture.code); await shell.expectCurrentEvent(); await say(3, 'Join the event');
    await page.reload(); await shell.expectCurrentEvent(); await say(4, 'Session survives refresh');
    await shell.goTo('Schedule'); await shell.expectStub('Schedule'); await say(5, 'Current implementation boundary');
    await shell.signOut(); await expect(page).toHaveURL(/\/access/);
    await page.goto(`/events/${fixture.id}`); await expect(page).toHaveURL(/\/access/); await say(6, 'Sign out');
  }, { baseURL: environment.url });
}
