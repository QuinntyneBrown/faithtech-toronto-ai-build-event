import { expect } from '@playwright/test';
import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { AdminRosterPage } from '../page-objects/admin-roster-page';
import { type Page } from '@playwright/test';

async function openRoster(page: Page) {
  const access = new AdminAccessPage(page); await access.open(); await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page); await events.open(); await events.createDraft('Roster acceptance'); await events.openEvent('Roster acceptance');
  const roster = new AdminRosterPage(page); await roster.open(); await roster.expectEmpty();
  return roster;
}

test('L2-036: given an unsaved participant, cancellation offers keep or discard and restores focus', async ({ page, roster: fixture }) => {
  const roster = await openRoster(page);
  await roster.beginAdd('Unsaved Alex'); await roster.cancelAdd(); await roster.keepEditing(); await roster.expectName('Unsaved Alex');
  await roster.cancelAdd(); await roster.discard(); await roster.expectEmpty(); await roster.expectAddFocused();
  expect(fixture.additions).toBe(0);
});

test('L2-002/041: given duplicate participant names, adding them reveals separate codes once and retains the roster on refresh', async ({ page }) => {
  const roster = await openRoster(page);
  await roster.add('Alex'); const first = await roster.takeCode('Alex');
  await roster.add('Alex'); const second = await roster.takeCode('Alex');
  expect(first).not.toBe(second); await roster.expectParticipants('Alex', 2);
  await page.reload(); await roster.expectParticipants('Alex', 2); await roster.expectNoCode();
});

test('L2-002/AC2: renaming a participant updates the roster without changing their identity', async ({ page }) => {
  const roster = await openRoster(page);
  await roster.add('Alex'); await roster.takeCode('Alex');
  await roster.rename('Alex', 'Alexandra');
  await roster.expectParticipants('Alexandra', 1); await roster.expectParticipants('Alex', 0);
  await page.reload(); await roster.expectParticipants('Alexandra', 1);
});
