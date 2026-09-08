import { expect, type Page } from '@playwright/test';
import { test } from '../fixtures/client-fixture';
import { ClientAccessPage } from '../page-objects/client-access-page';
import { ClientShellPage } from '../page-objects/client-shell-page';
import type { EventDetail } from '../../frontend/projects/api/src/public-api';
import type { EventFixture } from '../fixtures/event-fixture';
import type { EntryFixture } from '../fixtures/entry-fixture';

function published(id: string, title: string): EventDetail {
  return {
    id, title, published: true, useLiturgy: false, version: '1',
    venueName: null, address: null, latitude: null, longitude: null,
    waitingContent: null, closingContent: null, directionsUrl: null,
    timezone: null, start: null, end: null, startsAtUtc: null, endsAtUtc: null, logo: null,
  };
}

async function joinEvent(page: Page, events: EventFixture, entry: EntryFixture) {
  events.events.set('build-night', published('build-night', 'Build Night'));
  entry.registrations.set('build-night', [{ id: 'reg-1', code: 'ABC123', email: null, active: true }]);
  const access = new ClientAccessPage(page);
  await access.open('build-night');
  await access.signIn('alex@example.com', 'ABC123');
  return new ClientShellPage(page);
}

test('L2-004: a refreshed participant session restores identity without re-entry', async ({ page, events, entry }) => {
  const shell = await joinEvent(page, events, entry);
  await shell.expectCurrentEvent();
  await page.reload();
  await shell.expectCurrentEvent();
});

test('L2-046: core navigation reaches every activity without a broken link', async ({ page, events, entry }) => {
  const shell = await joinEvent(page, events, entry);
  await shell.goTo('Schedule'); await shell.expectStub('Schedule');
  await shell.goTo('Teams & projects'); await shell.expectStub('Teams and projects');
  await shell.goTo('People'); await shell.expectStub('People');
  await shell.goTo('Messages'); await shell.expectStub('Messages');
  await shell.goTo('Quiz'); await shell.expectStub('Quiz');
  await shell.goTo('Raffle'); await shell.expectStub('Raffle');
  await shell.goTo('Showcase'); await shell.expectStub('Showcase');
});

test('L2-004: sign-out revokes the current session and clears private content', async ({ page, events, entry }) => {
  const shell = await joinEvent(page, events, entry);
  await shell.expectCurrentEvent();
  await shell.signOut();
  await expect(page).toHaveURL(/\/events\/build-night\/access$/);
  await page.goBack();
  await expect(page.getByRole('heading', { name: "You're in.", exact: true })).toHaveCount(0);
});

test("L2-004: a session known to be expired requires re-authentication and shows no cached private content", async ({ page, events, entry, participantSession }) => {
  const shell = await joinEvent(page, events, entry);
  await shell.expectCurrentEvent();
  participantSession.expires = Date.now() - 1000;
  await page.reload();
  await expect(page).toHaveURL(/\/events\/build-night\/access$/);
  await expect(page.getByRole('heading', { name: "You're in.", exact: true })).toHaveCount(0);
});
