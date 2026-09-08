import { request, expect } from '@playwright/test';
import { randomUUID } from 'node:crypto';

export async function adminSession(baseURL, password, username = 'demo-host') {
  const client = await request.newContext({ baseURL, ignoreHTTPSErrors: true, timeout: 15000 });
  try {
    let csrf = (await (await client.get('/api/admin/antiforgery')).json()).requestToken;
    expect((await client.post('/api/admin/session', { data: { username, password }, headers: { 'X-CSRF-TOKEN': csrf } })).status()).toBe(204);
    csrf = (await (await client.get('/api/admin/antiforgery')).json()).requestToken;
    return { client, csrf,
      post: (path, data) => client.post(path, { data, headers: { 'X-CSRF-TOKEN': csrf, 'Idempotency-Key': randomUUID() } }),
      signOut: () => client.delete('/api/admin/session', { headers: { 'X-CSRF-TOKEN': csrf } }) };
  } catch (error) { await client.dispose(); throw error; }
}
export async function participantFixture(environment) {
  const admin = await adminSession(environment.url, environment.password);
  try {
    const response = await admin.post('/api/admin/events', { title: 'Toronto AI Build Event — demo fixture' });
    expect(response.status()).toBe(201); const { id } = await response.json();
    if (!/^[a-f0-9-]{36}$/.test(id)) throw new Error('Invalid fixture event identifier');
    const registration = await admin.post(`/api/admin/events/${id}/roster`, { displayName: 'Alex Morgan' });
    expect(registration.ok()).toBe(true); const { code } = await registration.json();
    await environment.sql(`USE [${environment.database}]; UPDATE Events SET Published = 1 WHERE Id = '${id}';`);
    return { id, code };
  } finally { await admin.client.dispose(); }
}
