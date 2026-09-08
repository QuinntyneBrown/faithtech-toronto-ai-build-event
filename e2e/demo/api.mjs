import { record } from './capture.mjs';
import { adminSession } from './api-client.mjs';
import { TerminalPage } from './terminal-page.mjs';
const lines = [
  'The dot net API is the shared service behind the administrator and participant applications. These requests run against a real local process and isolated SQL storage.',
  'An authenticated host can create an event draft. The API returns a created response and the event identifier. Authentication cookies and anti forgery tokens stay off screen.',
  'A separate read returns the same saved title and draft state. This verifies persistence rather than assuming that a successful write response is enough.',
  'A title longer than two hundred characters is rejected with a validation response. The list remains unchanged, so the rejected request did not create another event.',
  'The administrator has signed out successfully. A later request to the protected event list is denied, demonstrating that access is enforced by the API.',
  'This service also supports schedules, rosters, and participant sessions. The browser demonstrations show those workflows through their real user interfaces.',
];
export async function api(directory, environment) {
  return record('api', directory, lines, async ({ page, say, expect }) => {
    const terminal = new TerminalPage(page);
    const session = await adminSession(environment.url, environment.password);
    try {
      await terminal.open('FaithTech · API', 'Live HTTPS requests · real SQL persistence · synthetic data');
      await terminal.command('GET /api/admin/session');
      const authenticated = await session.client.get('/api/admin/session'); expect(authenticated.status()).toBe(200);
      await terminal.result('HTTP 200\nAdministrator session verified.\nCredentials and session identifiers omitted.'); await say(0, 'Shared application service');
      await terminal.command('POST /api/admin/events\n{ "title": "Toronto API demonstration" }');
      const created = await session.post('/api/admin/events', { title: 'Toronto API demonstration' }); expect(created.status()).toBe(201);
      const event = await created.json(); expect(event.published).toBe(false);
      await terminal.result(`HTTP 201 Created\n${JSON.stringify({ id: event.id, title: event.title, published: event.published }, null, 2)}`); await say(1, 'Create an event');
      await terminal.command(`GET /api/admin/events/${event.id}`);
      const read = await session.client.get(`/api/admin/events/${event.id}`); expect(read.status()).toBe(200);
      const saved = await read.json(); expect(saved.title).toBe('Toronto API demonstration'); expect(saved.published).toBe(false);
      await terminal.result(`HTTP 200\n${JSON.stringify({ id: saved.id, title: saved.title, published: saved.published }, null, 2)}`); await say(2, 'Read persisted state');
      const before = await (await session.client.get('/api/admin/events')).json();
      await terminal.command('POST /api/admin/events\nBody: title = 201 repeated letter a characters\n(The full repeated string is abbreviated for readability.)');
      const invalid = await session.post('/api/admin/events', { title: 'a'.repeat(201) }); expect(invalid.status()).toBe(422);
      const problem = await invalid.json();
      const after = await (await session.client.get('/api/admin/events')).json(); expect(after).toEqual(before);
      await terminal.result(`HTTP 422 Unprocessable Entity\ncode: ${problem.code}\ntitle: ${problem.errors.title.join(' ')}\n\nGET /api/admin/events: unchanged (${after.length} event)`); await say(3, 'Reject invalid input');
      await terminal.command('DELETE /api/admin/session\nGET /api/admin/events');
      expect((await session.signOut()).status()).toBe(204); const denied = await session.client.get('/api/admin/events'); expect(denied.status()).toBe(401);
      await terminal.result('DELETE: HTTP 204 No Content\nGET:    HTTP 401 Unauthorized'); await say(4, 'Enforce access'); await say(5, 'Related workflows');
    } finally { await session.client.dispose(); }
  });
}
