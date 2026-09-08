import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './specs',
  fullyParallel: true,
  retries: 0,
  expect: { timeout: 5000 },
  use: { channel: 'chrome', trace: 'retain-on-failure' },
  projects: [
    { name: 'admin', use: { baseURL: 'http://127.0.0.1:4200/admin/' }, testMatch: /admin-.*\.spec\.ts/ },
    { name: 'client', use: { baseURL: 'http://127.0.0.1:4201/' }, testMatch: /client-.*\.spec\.ts/ },
  ],
  webServer: [
    {
      command: 'npm --prefix ../frontend exec -- ng serve admin --configuration acceptance --port 4200 --host 127.0.0.1',
      url: 'http://127.0.0.1:4200/admin/',
      reuseExistingServer: false,
      timeout: 120000,
    },
    {
      command: 'npm --prefix ../frontend exec -- ng serve client --configuration acceptance --port 4201 --host 127.0.0.1',
      url: 'http://127.0.0.1:4201/',
      reuseExistingServer: false,
      timeout: 120000,
    },
  ],
});
