import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './specs',
  fullyParallel: true,
  retries: 0,
  expect: { timeout: 5000 },
  use: { baseURL: 'http://127.0.0.1:4200/admin/', channel: 'chrome', trace: 'retain-on-failure' },
  webServer: {
    command: 'npm --prefix ../frontend exec -- ng serve admin --configuration acceptance --port 4200 --host 127.0.0.1',
    url: 'http://127.0.0.1:4200/admin/',
    reuseExistingServer: false,
    timeout: 120000,
  },
});
