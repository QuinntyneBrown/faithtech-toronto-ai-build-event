import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { AdminEventEditorPage } from '../page-objects/admin-event-editor-page';
import { expect, type Page } from '@playwright/test';

async function openEditor(page: Page) {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open();
  await events.createDraft('Toronto build night');
  await events.openEvent('Toronto build night');
  return new AdminEventEditorPage(page);
}

const logoBytes = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScLbtAAAAABJRU5ErkJggg==', 'base64');

test('L2-026: given a new draft, the administrator can save the optional companion setting from its off default', async ({ page }) => {
  const editor = await openEditor(page);
  await editor.expectCompanion(false);
  await editor.setCompanion(true);
  await editor.save();
  await editor.expectSaved();
  await page.reload();
  await editor.expectCompanion(true);
  await editor.setCompanion(false);
  await editor.save();
  await editor.expectSaved();
  await page.reload();
  await editor.expectCompanion(false);
});

test('L2-036/044: given an unavailable saved logo, details remain usable and display can be retried', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.chooseLogo('venue.png', 'image/png', logoBytes);
  await editor.uploadLogo();
  await editor.expectLogoSaved();
  events.logoUnavailable = true;
  await page.reload();
  await editor.expectLogoFallback();
  await editor.expectDraft('Toronto build night');
  events.logoUnavailable = false;
  await editor.retryLogoPreview();
  await editor.expectLogoVisible();
  expect(events.logoUploads).toBe(1);
});

test('L2-044: given a lost logo response, retry confirms one saved image', async ({ page, events }) => {
  const editor = await openEditor(page);
  events.loseNextLogoResponse = true;
  await editor.chooseLogo('venue.png', 'image/png', logoBytes);
  await editor.uploadLogo();
  await editor.expectUnconfirmed();
  await editor.retryLogo();
  await editor.expectLogoSaved();
  expect(events.logoUploads).toBe(1);
});

test('L2-044: given a stale logo upload, explicit recovery retains the selected file', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.expectDraft('Toronto build night');
  const current = [...events.events.values()][0];
  events.events.set(current.id, { ...current, title: 'Saved by another host', version: '2' });
  await editor.chooseLogo('venue.png', 'image/png', logoBytes);
  await editor.uploadLogo();
  await editor.expectLogoError('Another administrator changed');
  expect(events.logoUploads).toBe(0);
  await editor.loadLatestLogoVersion();
  await editor.expectDraft('Saved by another host');
  await editor.uploadLogo();
  await editor.expectLogoSaved();
  expect(events.logoUploads).toBe(1);
});

test('L2-001/036: given a selected logo and text edits, saving text allows upload without losing either', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.chooseLogo('venue.png', 'image/png', logoBytes);
  await editor.editContent('New title', 'New venue', 'Welcome');
  await editor.expectLogoUploadBlocked();
  await editor.save();
  await editor.expectSaved();
  await editor.uploadLogo();
  await editor.expectLogoSaved();
  await page.reload();
  await editor.expectDraft('New title');
  await editor.expectContent('New venue', 'Welcome');
  await editor.expectLogoVisible();
  expect(events.logoUploads).toBe(1);
});

test('L2-036: given an unsubmitted logo, leaving requires discard and selection can be cleared', async ({ page }) => {
  const editor = await openEditor(page);
  await editor.chooseLogo('venue.png', 'image/png', logoBytes);
  await editor.returnToEvents();
  await editor.keepEditing();
  await editor.clearLogoSelection();
  await editor.expectLogoSelectionCleared();
  await editor.returnToEvents();
  await new AdminEventsPage(page).expectDraft('Toronto build night');
});

test('L2-001/040: given a venue image, upload saves and displays the accepted logo', async ({ page }) => {
  const editor = await openEditor(page);
  const image = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScLbtAAAAABJRU5ErkJggg==', 'base64');
  await editor.chooseLogo('venue.png', 'image/png', image);
  await editor.uploadLogo();
  await editor.expectLogoSaved();
  await editor.expectLogoSelectionCleared();
});

test('L2-040: given an unsupported venue logo, upload shows a field error', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.chooseLogo('venue.svg', 'image/svg+xml', Buffer.from('<svg/>'));
  await editor.uploadLogo();
  await editor.expectLogoError('PNG, JPEG or WebP');
  expect(events.saves).toBe(0);
});

test('L2-040: given Unicode event text, client limits count normalized scalar values', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.editContent('😀'.repeat(201), 'Venue', 'Welcome');
  await editor.save();
  await editor.expectInvalidTitle();
  expect(events.saves).toBe(0);
  await editor.editContent('  ' + '😀'.repeat(200) + '  ', '  Venue  ', '  Welcome  ');
  await editor.save();
  await editor.expectSaved();
  const saved = [...events.events.values()][0];
  expect(saved.title).toBe('😀'.repeat(200));
  expect(saved.venueName).toBe('Venue');
});

for (const width of [320, 575, 576, 767, 768, 991, 992, 1199, 1200, 1440]) {
test(`L2-035/036: editor content and validation remain accessible at ${width}px`, async ({ page }) => {
  const editor = await openEditor(page);
  await editor.editContent('A'.repeat(200), 'A'.repeat(200), 'Welcome\n<script>literal text</script>');
  await editor.expectAccessible(width);
  await editor.setDirections('http://example.org');
  await editor.save();
  await editor.expectInvalidDirections();
  await editor.expectAccessible(width);
  if (width === 1440) await editor.capture('test-results/event-editor-desktop.png');
});
}

test('L2-001/047: given event dates, saving and reopening preserves timezone and overnight dates', async ({ page }) => {
  const editor = await openEditor(page);
  await editor.setTimes('America/Toronto', '2026-09-09T23:00', '2026-09-10T01:00');
  await editor.save();
  await editor.expectSaved();
  await editor.returnToEvents();
  const list = new AdminEventsPage(page);
  await list.expectTiming('Toronto build night', '2026-09-09', 'America/Toronto');
  await list.openEvent('Toronto build night');
  await editor.expectTimes('America/Toronto', '2026-09-09T23:00', '2026-09-10T01:00');
});

test('L2-036/044: given unsaved event changes, leaving requires an explicit discard', async ({ page }) => {
  const editor = await openEditor(page);
  await editor.editContent('Unsaved title', 'Unsaved venue', 'Unsaved welcome');
  await editor.returnToEvents();
  await editor.keepEditing();
  await editor.expectContent('Unsaved venue', 'Unsaved welcome');
  await editor.returnToEvents();
  await editor.discardChanges();
  const list = new AdminEventsPage(page);
  await list.expectDraft('Toronto build night');
  await list.openEvent('Toronto build night');
  await editor.expectContent('', '');
});

test('L2-044/AC4: given a stale editor, conflict recovery preserves proposed changes for explicit reapplication', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.editContent('My proposed title', 'My venue', 'My welcome');
  const original = [...events.events.values()][0];
  events.events.set(original.id, { ...original, title: 'Newer saved title', version: '2' });
  await editor.save();
  await editor.expectConflict('Newer saved title');
  await editor.expectContent('My venue', 'My welcome');
  expect(events.saves).toBe(0);
  await editor.reapply();
  await editor.save();
  await editor.expectSaved();
  expect(events.events.get(original.id)?.title).toBe('My proposed title');
});

test('L2-044/AC6: given a lost save response, retry confirms one committed operation', async ({ page, events }) => {
  const editor = await openEditor(page);
  await editor.editContent('My proposed title', 'My venue', 'My welcome');
  events.loseNextSaveResponse = true;
  await editor.save();
  await editor.expectUnconfirmed();
  await editor.retrySave();
  await editor.expectSaved();
  expect(events.saves).toBe(1);
});

test('L2-001/AC6: given invalid directions, field feedback retains the proposed draft', async ({ page }) => {
  const editor = await openEditor(page);
  await editor.editContent('My title', 'My venue', 'My welcome');
  await editor.setDirections('http://example.org');
  await editor.save();
  await editor.expectInvalidDirections();
  await editor.expectContent('My venue', 'My welcome');
  await editor.setDirections('https://example.org');
  await editor.save();
  await editor.expectSaved();
});

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

test('L2-001/AC1: given an incomplete draft, saving content retains it when reopened', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open();
  await events.createDraft('Toronto build night');
  await events.openEvent('Toronto build night');
  const editor = new AdminEventEditorPage(page);
  await editor.editContent('Updated event', 'Toronto venue', 'Welcome, builders.');
  await editor.save();
  await editor.expectSaved();
  await editor.returnToEvents();
  await events.openEvent('Updated event');
  await editor.expectDraft('Updated event');
  await editor.expectContent('Toronto venue', 'Welcome, builders.');
});
