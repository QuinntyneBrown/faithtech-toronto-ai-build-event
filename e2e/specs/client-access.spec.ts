import { test } from '../fixtures/client-fixture';
import { ClientAccessPage } from '../page-objects/client-access-page';
import type { EventDetail } from '../../frontend/projects/api/src/public-api';

function published(id: string, title: string): EventDetail {
  return {
    id, title, published: true, useLiturgy: false, version: '1',
    venueName: null, address: null, latitude: null, longitude: null,
    waitingContent: null, closingContent: null, directionsUrl: null,
    timezone: null, start: null, end: null, startsAtUtc: null, endsAtUtc: null, logo: null,
  };
}

test('L2-046: an unknown event link shows a generic unavailable access screen', async ({ page }) => {
  const access = new ClientAccessPage(page);
  await access.open('00000000-0000-0000-0000-000000000099');
  await access.expectUnavailable();
});

test('L2-046: a draft event link shows the same generic unavailable access screen as unknown', async ({ page, events }) => {
  events.events.set('draft-event', { ...published('draft-event', 'Draft Night'), published: false });
  const access = new ClientAccessPage(page);
  await access.open('draft-event');
  await access.expectUnavailable();
});

test('L2-003: valid email and code bind the entry and confirm the join', async ({ page, events, entry }) => {
  events.events.set('build-night', published('build-night', 'Build Night'));
  entry.registrations.set('build-night', [{ id: 'reg-1', code: 'ABC123', email: null, active: true }]);
  const access = new ClientAccessPage(page);
  await access.open('build-night');
  await access.expectTitle('Build Night');
  await access.signIn('alex@example.com', 'ABC123');
  await access.expectJoined();
});

test('L2-003: whitespace and casing differences in a bound email still resume the same participant', async ({ page, events, entry }) => {
  events.events.set('build-night', published('build-night', 'Build Night'));
  entry.registrations.set('build-night', [{ id: 'reg-1', code: 'ABC123', email: 'ALEX@EXAMPLE.COM', active: true }]);
  const access = new ClientAccessPage(page);
  await access.open('build-night');
  await access.signIn('  alex@example.com  ', 'ABC123');
  await access.expectJoined();
});

test('L2-003: a malformed email, wrong code, or a mismatched email is denied generically', async ({ page, events, entry }) => {
  events.events.set('build-night', published('build-night', 'Build Night'));
  entry.registrations.set('build-night', [{ id: 'reg-1', code: 'ABC123', email: 'alex@example.com', active: true }]);
  const access = new ClientAccessPage(page);
  await access.open('build-night');

  await access.signIn('not-an-email', 'ABC123');
  await access.expectDenied();
  await access.expectCodeCleared();

  await access.signIn('alex@example.com', 'WRONGCODE');
  await access.expectDenied();

  await access.signIn('someone-else@example.com', 'ABC123');
  await access.expectDenied();
});
