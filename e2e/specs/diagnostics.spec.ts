// Acceptance Test
// Traces to: L2-034 AC2; L2-045 AC1
// Description: A tab that comes back after being in the background
// resynchronizes to the saved screen rather than showing stale state, and a
// failure gives the person something an operator can trace without exposing
// internals.

import { expect, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";
import { ProjectsPage } from "../page-objects/projects.page";

test("L2-034/AC2: a restored tab resynchronizes to the saved screen", async ({ page, store }) => {
  store.addProject({
    title: "RTR",
    description: "Reconciliation Through Relationships.",
    repositoryUrl: null,
    demoUrl: null
  });

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectEntryEnabled();

  // The event moves on while this tab is in the background and hears nothing.
  store.currentScreen = "projects";
  store.version += 1;

  await page.evaluate(() => window.dispatchEvent(new Event("visibilitychange")));

  await expect(page).toHaveURL(/\/projects$/);
  await new ProjectsPage(page).expectProject("RTR", "Reconciliation Through Relationships.");
});

test("L2-034/AC2: a restored tab shows a completed draw without replaying it", async ({
  page,
  store
}) => {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 1_000;

  await page.goto("/raffle");
  await expect(page).toHaveURL(/\/raffle$/);

  // The whole draw happens and finishes while the tab is in the background.
  store.seedDraw(30_000);

  await page.evaluate(() => window.dispatchEvent(new Event("visibilitychange")));

  await expect(page.locator("h2.winner")).toHaveText("Participant 001");
  await expect(page.getByRole("button", { name: "Stop effects", exact: true })).toHaveCount(0);
});

test.fixme("L2-045/AC1: a failed save gives the person a traceable error reference", async ({
  page,
  store
}) => {
  // Not implemented: failures render one fixed sentence with nothing an
  // operator could correlate to a server-side record. The messages are
  // correctly free of internals, but there is no reference to report.
  const countdown = new CountdownPage(page);
  await countdown.open();

  store.failNext("POST /api/participant/entries", { status: 503 });
  await countdown.enter("ada@example.com");

  await expect(page.getByRole("alert")).toContainText(/reference/i);
});
