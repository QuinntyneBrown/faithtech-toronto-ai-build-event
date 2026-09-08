import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { expect } from '@playwright/test';

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

test('L2-038: deliberate navigation renews administrator activity', async ({ page, session }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  await new AdminEventsPage(page).open();
  await expect.poll(() => session.interactions).toBeGreaterThan(0);
});

test('L2-036: cancelling an unsaved draft preserves keyboard focus and changes nothing', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open();
  await events.cancelUnsavedDraft();
  await events.expectEmpty();
});
