// Acceptance Test
// Traces to: DB-L2-001, DB-L2-002, DB-L2-003
// Description: Inspect, review, persist and restore the throwaway project burndown.
import { test } from '@playwright/test';
import { DashboardPage } from '../page-objects/dashboard.page';

// Traces to: DB-L2-004
test('development estimate reflects saved work and stays independent of filters', async ({ page }) => {
  const dashboard = new DashboardPage(page); await dashboard.open(); await dashboard.expectDevelopment(66);
  await dashboard.inspect('L2-009'); await dashboard.save('review', 'Team controls implemented; acceptance pending.');
  await dashboard.expectDevelopment(67); await dashboard.expectRemaining(34);
  await dashboard.search('nothing-matches'); await dashboard.expectDevelopment(67);
  await page.reload(); await dashboard.expectDevelopment(67);
});

test('audit is searchable and filtering never changes the full-scope totals', async ({ page }) => {
  const dashboard = new DashboardPage(page);
  await dashboard.open(); await dashboard.expectBaseline();
  await dashboard.search('nothing-matches-this'); await dashboard.expectNoMatches();
  await dashboard.expectRemaining(34);
});
test('completion needs evidence, survives reload and can be reopened', async ({ page }) => {
  const dashboard = new DashboardPage(page);
  await dashboard.open(); await dashboard.inspect('L2-009');
  await dashboard.save('done', ''); await dashboard.expectMessage(/evidence/i);
  await dashboard.save('done', 'Verified keyboard and pointer moves in Chrome.');
  await dashboard.expectRemaining(33); await page.reload();
  await dashboard.expectRemaining(33); await dashboard.inspect('L2-009');
  await dashboard.save('progress', 'Reopened after a regression.'); await dashboard.expectRemaining(34);
});
test('export restores progress after browser storage is cleared', async ({ page }, testInfo) => {
  const dashboard = new DashboardPage(page);
  await dashboard.open(); await dashboard.inspect('L2-003'); await dashboard.save('done', 'Entry acceptance verified.');
  const download = await dashboard.export();
  const path = testInfo.outputPath('progress.json'); await download.saveAs(path);
  await page.evaluate(() => localStorage.clear()); await page.reload(); await dashboard.expectRemaining(34);
  await dashboard.import(path); await dashboard.expectRemaining(33);
});
test('mobile inspection stays reachable and restores focus', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  const dashboard = new DashboardPage(page); await dashboard.open(); await dashboard.expectFits();
  await dashboard.inspect('L2-009'); await dashboard.expectFits();
  await dashboard.closeInspector(); await dashboard.expectFocusOn('L2-009');
});
test('invalid imports preserve current accepted reviews', async ({ page }) => {
  const dashboard = new DashboardPage(page); await dashboard.open();
  await dashboard.inspect('L2-003'); await dashboard.save('done', 'Entry acceptance verified.');
  await dashboard.importInvalid(); await dashboard.expectMessage(/Import rejected/);
  await dashboard.expectRemaining(33);
});
test('corrupt storage shows a usable baseline and a warning', async ({ page }) => {
  const dashboard = new DashboardPage(page); await dashboard.corruptStorage(); await dashboard.open();
  await dashboard.expectBaseline(); await dashboard.expectMessage(/unreadable/);
});
test('storage failure keeps reviews usable and warns to export', async ({ page }) => {
  const dashboard = new DashboardPage(page); await dashboard.denyStorage(); await dashboard.open();
  await dashboard.expectBaseline(); await dashboard.inspect('L2-003');
  await dashboard.save('done', 'Entry acceptance verified.'); await dashboard.expectRemaining(33);
  await dashboard.expectMessage(/session only/); await dashboard.export();
});
