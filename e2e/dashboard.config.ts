import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './specs', testMatch: 'dashboard.spec.ts', fullyParallel: true,
  use: { baseURL: 'http://127.0.0.1:4317', channel: 'chrome', trace: 'retain-on-failure' },
  webServer: { command: 'node ../docs/dashboard/serve.mjs', url: 'http://127.0.0.1:4317', reuseExistingServer: !process.env.CI },
});
