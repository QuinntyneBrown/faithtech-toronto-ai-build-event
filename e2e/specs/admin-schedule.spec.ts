import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';
import { AdminEventsPage } from '../page-objects/admin-events-page';
import { AdminSchedulePage } from '../page-objects/admin-schedule-page';
import { expect, type Page } from '@playwright/test';

async function openSchedule(page: Page) {
  const access = new AdminAccessPage(page);
  await access.open(); await access.signIn('host@example.com', 'host-demo');
  await new AdminSessionPage(page).expectSignedIn();
  const events = new AdminEventsPage(page);
  await events.open(); await events.createDraft('Overnight build'); await events.openEvent('Overnight build');
  const schedule = new AdminSchedulePage(page);
  await schedule.open(); await schedule.expectEmpty();
  return schedule;
}

test('L2-008/036: given a proposed schedule, applying the reference requires confirmation and renders its saved values', async ({ page, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.addStage('Custom draft', '2026-09-09T23:00', '2026-09-10T01:00', 'Keep until confirmed');
  await schedule.requestReference(); await schedule.cancelReference();
  await schedule.expectStage('Custom draft', 'Keep until confirmed'); expect(schedules.references).toBe(0);
  await schedule.requestReference(); await schedule.confirmReference(); await schedule.expectSaved();
  await schedule.expectStage('Reference arrival', 'Welcome from the reference service');
  await schedule.expectWindow('selection', '2026-09-09T18:05', '2026-09-09T18:15');
  await schedule.expectWindow('demo presentation', '2026-09-09T20:30', '2026-09-09T20:50');
  await page.reload(); await schedule.expectStage('Reference arrival', 'Welcome from the reference service');
  expect(schedules.references).toBe(1);
});

test('L2-036/AC2: given invalid schedule timing, validation focuses a linked summary and retains the draft', async ({ page, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  schedules.validationErrors = { end: ['The event end needs correction.'] };
  await schedule.save(); await schedule.followValidation('Event end', 'The event end needs correction.');
  schedules.validationErrors = null;
  await schedule.save(); await schedule.expectSaved();
  expect([...schedules.schedules.values()][0].end?.local).toBe('2026-09-10T01:00');
});

for (const kind of ['selection', 'demo presentation'] as const) {
  test(`L2-036/AC2: given invalid ${kind} timing, the summary reaches the retained window field`, async ({ page, schedules }) => {
    const schedule = await openSchedule(page);
    await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
    await schedule.setWindow(kind, '2026-09-09T23:15', '2026-09-10T00:15');
    const field = kind === 'selection' ? 'selection.end' : 'presentation.end';
    schedules.validationErrors = { [field]: ['The window end needs correction.'] };
    await schedule.save(); await schedule.followValidation(kind[0].toUpperCase() + kind.slice(1) + ' end', 'The window end needs correction.');
    await schedule.expectWindow(kind, '2026-09-09T23:15', '2026-09-10T00:15');
    schedules.validationErrors = null; await schedule.save(); await schedule.expectSaved();
  });
}

test('L2-036/AC2: given invalid stage timing, the summary opens the affected stage field for correction', async ({ page, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.addStage('Arrival', '2026-09-09T23:00', '2026-09-10T00:00', 'Welcome');
  await schedule.addStage('Build', '2026-09-10T00:00', '2026-09-10T01:00', 'Keep this guidance');
  schedules.validationErrors = { 'stages[1].start': ['Correct the stage start.'] };
  await schedule.save(); await schedule.followValidation('Stage start', 'Correct the stage start.', 'Build — Stage start');
  await schedule.correctStageStart('2026-09-10T00:05'); schedules.validationErrors = null;
  await schedule.save(); await schedule.expectSaved();
  await schedule.expectStage('Arrival', 'Welcome'); await schedule.expectStage('Build', 'Keep this guidance');
  expect([...schedules.schedules.values()][0].stages[1].start?.local).toBe('2026-09-10T00:05');
});

test('L2-036: given a stage error, removing an earlier stage keeps its identity and removing the affected stage clears feedback', async ({ page, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.addStage('Arrival', '2026-09-09T23:00', '2026-09-10T00:00', 'Welcome');
  await schedule.addStage('Build', '2026-09-10T00:00', '2026-09-10T01:00', 'Retained');
  schedules.validationErrors = { 'stages[1].start': ['Correct the stage start.'] };
  await schedule.save(); await schedule.expectError('Correct the stage start.'); await schedule.removeStage('Arrival');
  await schedule.followValidation('Stage start', 'Correct the stage start.', 'Build — Stage start', false);
  await schedule.cancelStage(); await schedule.removeStage('Build');
  await schedule.expectNoValidation(); await schedule.expectStagesHeadingFocused();
  schedules.validationErrors = null; await schedule.save(); await schedule.expectSaved();
});

test('L2-036: given an invalid window, disabling it removes feedback for the removed controls', async ({ page, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.setWindow('selection', '2026-09-09T23:15', '2026-09-10T00:15');
  schedules.validationErrors = { 'selection.end': ['Correct the window.'] };
  await schedule.save(); await schedule.expectError('Correct the window.'); await schedule.disableWindow('selection');
  await schedule.expectNoValidation();
});

test('L2-044: given a lost schedule response, retry confirms one committed save', async ({ page, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00'); schedules.loseNextResponse = true;
  await schedule.save(); await schedule.expectUncertain(); await schedule.retrySave(); await schedule.expectSaved();
  expect(schedules.saves).toBe(1);
});

test('L2-044: given a stale schedule, explicit reapplication retains the proposed timings', async ({ page, events, schedules }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  const current = [...events.events.values()][0]; events.events.set(current.id, { ...current, version: '2' });
  await schedule.save(); await schedule.expectError('Another administrator changed');
  expect(schedules.saves).toBe(0);
  await schedule.reapply(); await schedule.save(); await schedule.expectSaved(); expect(schedules.saves).toBe(1);
});

test('L2-036: given an unsaved schedule and stage dialog, cancellation preserves edits and keyboard focus', async ({ page }) => {
  const schedule = await openSchedule(page);
  await schedule.beginStage(); await schedule.expectStageFocused(); await schedule.cancelStage(); await schedule.expectAddFocused();
  await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.returnToSettings(); await schedule.keepEditing(); await schedule.save(); await schedule.expectSaved();
});

for (const width of [320, 575, 576, 767, 768, 991, 992, 1199, 1200, 1440]) {
  test(`L2-035/036: schedule empty populated validation and overlay states work at ${width}px`, async ({ page }) => {
    const schedule = await openSchedule(page);
    await schedule.expectAccessible(width); await schedule.beginStage(); await schedule.expectAccessible(width); await schedule.cancelStage();
    await schedule.setEventTimes('UTC', '2026-09-09T23:00', '2026-09-10T01:00');
    await schedule.addStage('Arrival', '2026-09-09T23:00', '2026-09-10T00:00', 'A'.repeat(200));
    await schedule.addStage('Overlapping stage', '2026-09-09T23:30', '2026-09-10T01:00', 'Literal <script> text');
    await schedule.expectAccessible(width); await schedule.save(); await schedule.expectError('overlap'); await schedule.expectAccessible(width);
    if (width === 1440) await schedule.capture('test-results/schedule-desktop.png');
  });
}

test('L2-006: given independent activity windows, saving and disabling them survives refresh', async ({ page }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('America/Toronto', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.setWindow('selection', '2026-09-09T23:10', '2026-09-09T23:40');
  await schedule.setWindow('demo presentation', '2026-09-10T00:30', '2026-09-10T00:50');
  await schedule.save(); await schedule.expectSaved(); await page.reload();
  await schedule.expectWindow('selection', '2026-09-09T23:10', '2026-09-09T23:40');
  await schedule.expectWindow('demo presentation', '2026-09-10T00:30', '2026-09-10T00:50');
  await schedule.disableWindow('selection'); await schedule.save(); await schedule.expectSaved(); await page.reload();
  await schedule.expectWindowDisabled('selection');
  await schedule.expectWindow('demo presentation', '2026-09-10T00:30', '2026-09-10T00:50');
});

test('L2-006/007: given an empty schedule, an administrator saves distinct overnight stages and reopens their content', async ({ page }) => {
  const schedule = await openSchedule(page);
  await schedule.setEventTimes('America/Toronto', '2026-09-09T23:00', '2026-09-10T01:00');
  await schedule.addStage('Arrival', '2026-09-09T23:00', '2026-09-10T00:00', 'Welcome, builders.');
  await schedule.addStage('Build', '2026-09-10T00:00', '2026-09-10T01:00', 'Build something useful.');
  await schedule.save(); await schedule.expectSaved(); await page.reload();
  await schedule.expectStage('Arrival', 'Welcome, builders.');
  await schedule.expectStage('Build', 'Build something useful.');
});
