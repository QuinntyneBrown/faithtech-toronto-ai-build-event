import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';

test('L2-038: refresh restores the server session', async ({ page }) => {
  const access = new AdminAccessPage(page);
  const session = new AdminSessionPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await session.expectSignedIn();
  await session.refresh();
  await session.expectSignedIn();
});

test('L2-038/041: known expiry clears private content while offline', async ({ page, session }) => {
  session.lifetime = 2000;
  const access = new AdminAccessPage(page);
  const screen = new AdminSessionPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await screen.expectSignedIn();
  session.unavailable = true;
  await access.expectSignIn();
  await screen.expectPrivateContentCleared();
});

test('L2-041: failed sign-out conceals private content and remains retryable', async ({ page, session }) => {
  const access = new AdminAccessPage(page);
  const screen = new AdminSessionPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await screen.expectSignedIn();
  session.signOutUnavailable = true;
  await screen.signOut();
  await screen.expectPendingSignOut();
  session.signOutUnavailable = false;
  await screen.retry();
  await access.expectSignIn();
});
