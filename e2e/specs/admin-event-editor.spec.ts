import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { AdminEventEditorPage } from '../page-objects/admin-event-editor-page';

test('L2-047/AC1: given a saved draft, opening it shows its current configuration', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open();
  await events.createDraft('Toronto build night');
  await events.openEvent('Toronto build night');
  await new AdminEventEditorPage(page).expectDraft('Toronto build night');
});
