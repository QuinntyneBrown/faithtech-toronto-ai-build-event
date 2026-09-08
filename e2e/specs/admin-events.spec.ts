import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';

test('L2-001: an administrator saves an incomplete event as a draft', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open();
  await events.expectEmpty();
  await events.createDraft('Toronto build night');
  await events.expectDraft('Toronto build night');
  await events.expectDialogClosed();
});
