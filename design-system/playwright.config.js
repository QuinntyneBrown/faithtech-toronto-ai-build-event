import { defineConfig } from '@playwright/test';
export default defineConfig({
  testDir:'./tests',timeout:90000,fullyParallel:false,workers:1,
  use:{baseURL:'http://127.0.0.1:4317',channel:'chrome',headless:true,trace:'retain-on-failure'},
  webServer:{command:'node scripts/serve.mjs',url:'http://127.0.0.1:4317/design-system/',reuseExistingServer:true},
  reporter:[['list'],['html',{open:'never'}]]
});
