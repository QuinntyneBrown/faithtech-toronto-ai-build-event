// Acceptance Test
// Traces to: L2-005 AC1, AC2, AC3, AC4
// Description: Countdown welcomes an unregistered viewer, counts down to the
// configured target against server time rather than the device clock, stops at
// zero without advancing, represents targets beyond a day, and stays honest
// when the clock cannot be synchronized.

import { expect, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";

test("L2-005/AC1: an unregistered viewer sees the welcome, the details, and a running countdown", async ({
  page,
  store
}) => {
  store.countdownTargetUtc = new Date(Date.now() + 90_000).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.expectWelcome(store.welcome);
  await countdown.expectInformation(store.purpose, store.venue);
  await countdown.expectEntryPrompt();
  await countdown.expectCountdownRunning();

  // No login was needed to see any of it.
  await countdown.expectAdvanceHidden();
  await countdown.roster.expectHidden();

  const first = await countdown.readRemainingSeconds();
  expect(first).toBeLessThanOrEqual(90);
  expect(first).toBeGreaterThan(80);

  await expect(async () => {
    expect(await countdown.readRemainingSeconds()).toBeLessThan(first);
  }).toPass({ timeout: 5_000 });
});

test("L2-005/AC1: a device clock ten minutes out does not shift the remaining time", async ({
  page,
  store
}) => {
  // The server is authoritative and reports a time ten minutes behind this
  // device. A countdown driven by the device clock would already read zero.
  const offset = -10 * 60_000;
  store.serverClockOffsetMs = offset;
  store.countdownTargetUtc = new Date(Date.now() + offset + 90_000).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectCountdownRunning();

  const remaining = await countdown.readRemainingSeconds();
  expect(remaining).toBeLessThanOrEqual(90);
  expect(remaining).toBeGreaterThan(80);
});

test("L2-005/AC2: reaching the target shows zero and waits for an administrator", async ({
  page,
  store
}) => {
  store.countdownTargetUtc = new Date(Date.now() - 5_000).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.expectCountdownRunning();
  expect(await countdown.readRemainingSeconds()).toBe(0);
  await countdown.expectCountdownComplete();

  // Entry stays open and the event has not moved on by itself.
  await countdown.expectEntryPrompt();
  await countdown.expectEntryEnabled();
  expect(store.currentScreen).toBe("countdown");
  await expect(page).toHaveURL(/\/countdown$/);
});

test("L2-005/AC3: a target beyond a day is shown as days plus the remaining time", async ({
  page,
  store
}) => {
  const target = 50 * 3_600_000 + 90_000;
  store.countdownTargetUtc = new Date(Date.now() + target).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectCountdownRunning();

  await countdown.expectUnitPresent("Days");
  await countdown.expectUnitValue("Days", "02");
  await countdown.expectUnitValue("Hours", "02");

  const remaining = await countdown.readRemainingSeconds();
  expect(remaining).toBeGreaterThan(50 * 3_600 - 10);
});

test("L2-005/AC4: an unavailable clock shows the scheduled time and says so", async ({ page, store }) => {
  store.failAlways("GET /api/event/time", { abort: true });
  store.countdownTargetUtc = new Date(Date.now() + 90_000).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.expectWelcome(store.welcome);
  await countdown.expectClockUnavailable();
  await countdown.expectScheduledTime(/UTC/);
});

test("L2-005/AC4: the clock recovers once server time is available again", async ({
  page,
  store,
  hub
}) => {
  store.failAlways("GET /api/event/time", { abort: true });
  store.countdownTargetUtc = new Date(Date.now() + 90_000).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectClockUnavailable();

  store.clearFailure("GET /api/event/time");
  store.version += 1;
  hub.pushEventUpdate(store.version);

  await countdown.expectCountdownRunning();
});
