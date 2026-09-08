import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { AdminSchedulePage } from '../page-objects/admin-schedule-page';

test('L2-006/007: given an empty schedule, an administrator saves distinct overnight stages and reopens their content', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open(); await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open(); await events.createDraft('Overnight build'); await events.openEvent('Overnight build');
  const schedule = new AdminSchedulePage(page);
  await schedule.open(); await schedule.expectEmpty();
  await schedule.setEventTimes('America/Toronto', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.addStage('Arrival', '2026-09-09T23:00', '2026-09-10T00:00', 'Welcome, builders.');
  await schedule.addStage('Build', '2026-09-10T00:00', '2026-09-10T01:00', 'Build something useful.');
  await schedule.save(); await schedule.expectSaved(); await page.reload();
  await schedule.expectStage('Arrival', 'Welcome, builders.');
  await schedule.expectStage('Build', 'Build something useful.');
});
