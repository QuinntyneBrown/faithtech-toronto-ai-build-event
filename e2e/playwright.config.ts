import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./specs",
  testIgnore: "dashboard.spec.ts",
  fullyParallel: true,
  retries: 0,
  expect: { timeout: 10_000 },
  use: {
    baseURL: "http://127.0.0.1:4210",
    channel: "chrome",
    trace: "retain-on-failure"
  },
  webServer: {
    command: "npm --prefix ../frontend exec -- ng serve client --port 4210 --host 127.0.0.1",
    url: "http://127.0.0.1:4210",
    reuseExistingServer: !process.env.CI,
    timeout: 180_000
  }
});
