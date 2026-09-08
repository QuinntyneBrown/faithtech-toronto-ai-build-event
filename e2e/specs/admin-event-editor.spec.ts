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
