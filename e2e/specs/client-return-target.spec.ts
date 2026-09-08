import { expect } from '@playwright/test';
import { test } from '../fixtures/client-fixture';
import { ClientAccessPage } from '../page-objects/client-access-page';
import { ClientShellPage } from '../page-objects/client-shell-page';
import type { EventDetail } from '../../frontend/projects/api/src/public-api';

function published(id: string, title: string): EventDetail {
  return {
    id, title, published: true, useLiturgy: false, version: '1',
    venueName: null, address: null, latitude: null, longitude: null,
    waitingContent: null, closingContent: null, directionsUrl: null,
    timezone: null, start: null, end: null, startsAtUtc: null, endsAtUtc: null, logo: null,
  };
}

test('L2-046: a protected deep link resumes after authentication when still accessible', async ({ page, events, entry }) => {
  events.events.set('build-night', published('build-night', 'Build Night'));
  entry.registrations.set('build-night', [{ id: 'reg-1', code: 'ABC123', email: null, active: true }]);

  await page.goto('/events/build-night/schedule');
  await expect(page).toHaveURL(/\/events\/build-night\/access\?returnTo=/);

  const access = new ClientAccessPage(page);
  await access.signIn('alex@example.com', 'ABC123');

  await expect(page).toHaveURL(/\/events\/build-night\/schedule$/);
  await new ClientShellPage(page).expectStub('Schedule');
});

test('L2-046: an unrecognized or another event\'s return target falls back to the authorized default', async ({ page, events, entry }) => {
  events.events.set('build-night', published('build-night', 'Build Night'));
  events.events.set('other-event', published('other-event', 'Other Event'));
  entry.registrations.set('build-night', [{ id: 'reg-1', code: 'ABC123', email: null, active: true }]);

  await page.goto('/events/build-night/access?returnTo=%2Fevents%2Fother-event%2Fschedule');
  const access = new ClientAccessPage(page);
  await access.signIn('alex@example.com', 'ABC123');

  await expect(page).toHaveURL(/\/events\/build-night$/);
  await new ClientShellPage(page).expectCurrentEvent();
});

test('L2-046/L2-004: switching to a different event never shows the prior event\'s content under the new one', async ({ page, events, entry }) => {
  events.events.set('event-a', published('event-a', 'Event A'));
  events.events.set('event-b', published('event-b', 'Event B'));
  entry.registrations.set('event-a', [{ id: 'reg-a', code: 'CODEA', email: null, active: true }]);

  const access = new ClientAccessPage(page);
  await access.open('event-a');
  await access.signIn('alex@example.com', 'CODEA');
  await new ClientShellPage(page).expectCurrentEvent();

  await page.goto('/events/event-b');
  await expect(page).toHaveURL(/\/events\/event-b\/access(\?|$)/);
  await expect(page.getByRole('heading', { name: "You're in.", exact: true })).toHaveCount(0);
});
