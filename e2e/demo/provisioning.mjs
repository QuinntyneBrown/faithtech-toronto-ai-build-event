import { record } from './capture.mjs';
import { adminSession } from './api-client.mjs';
import { TerminalPage } from './terminal-page.mjs';
const lines = [
  'The provisioning command line tool prepares storage and controls administrator accounts. This display streams output from the actual local executable.',
  'Migration completed successfully. This isolated database was initialized during setup; running migration again confirms it can safely apply any pending schema changes.',
  'The new operator account was created. Its password arrived through redirected standard input and was never echoed or passed as a command argument.',
  'A real API sign in verifies that the provisioned account can authenticate. No authentication response has been simulated.',
  'Disabling the operator completed successfully. A request using its existing authenticated session is now denied, confirming that disabling revokes access.',
  'The tool has completed migration, account creation, and account disabling. This disposable database and the owned API process are cleaned up after the recording.',
];
export async function provisioning(directory, environment) {
  return record('provisioning', directory, lines, async ({ page, say, expect }) => {
    const terminal = new TerminalPage(page); let session;
    const execute = async args => {
      await terminal.command(`dotnet FaithTechTorontoAiBuildEvent.Provisioning.dll ${args.join(' ')}`);
      let output = '', pending = Promise.resolve();
      await environment.cli(args, chunk => { output += chunk; const text = output; pending = pending.then(() => terminal.result(text)); });
      await pending; expect(output).toContain('Operation completed.');
    };
    try {
      await terminal.open('FaithTech · Provisioning', 'Working directory: Provisioning/bin/Debug/net10.0 · live subprocess output');
      await terminal.command('Commands: migrate | create-admin <username> | disable-admin <username>'); await say(0, 'Operator tooling');
      await execute(['migrate']); await say(1, 'Apply migrations');
      await execute(['create-admin', 'demo-operator']); await say(2, 'Provision an operator');
      session = await adminSession(environment.url, environment.password, 'demo-operator');
      await terminal.command('POST /api/admin/session\nGET /api/admin/session');
      expect((await session.client.get('/api/admin/session')).status()).toBe(200);
      await terminal.result('POST: HTTP 204 No Content\nGET:  HTTP 200 OK\nProvisioned account authenticated.'); await say(3, 'Verify authentication');
      await execute(['disable-admin', 'demo-operator']);
      expect((await session.client.get('/api/admin/session')).status()).toBe(401);
      await terminal.result('Operation completed.\n\nGET /api/admin/session: HTTP 401 Unauthorized'); await say(4, 'Revoke access'); await say(5, 'Completed operator workflow');
    } finally { await session?.client.dispose(); }
  });
}
