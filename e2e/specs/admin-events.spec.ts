import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { AdminEventEditorPage } from '../page-objects/admin-event-editor-page';
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

test('L2-047: administrator copies an event configuration to a new draft with shifted intervals', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open();
  await events.createDraft('Copy source');
  await events.openEvent('Copy source');
  const editor = new AdminEventEditorPage(page);
  await editor.setCompanion(true);
  await editor.setTimes('America/Toronto', '2026-09-09T17:00', '2026-09-09T21:00');
  await editor.save();
  await editor.expectSaved();
  await editor.returnToEvents();
  await events.copy('Copy source', '2026-09-16T17:00');
  await editor.expectTimes('America/Toronto', '2026-09-16T17:00', '2026-09-16T21:00');
  await editor.expectCompanion(false);
});
