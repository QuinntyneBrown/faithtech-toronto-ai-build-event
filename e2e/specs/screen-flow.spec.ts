// Acceptance Test
// Traces to: L2-007 AC1, AC2, AC3, AC4, AC5, AC6
// Description: Exactly four screens advance forward, only when an
// administrator says so. Public browsers follow the saved current screen,
// deep links and refreshes cannot reach a future stage or replay a past one,
// and the clock never advances anything on its own.

import { ADMINISTRATOR_PASSCODE } from "../mocks/event-store";
import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";
import { ProjectsPage } from "../page-objects/projects.page";
import { RafflePage } from "../page-objects/raffle.page";
import { TeamsPage } from "../page-objects/teams.page";

test("L2-007/AC1: closing Countdown moves connected public browsers to Projects", async ({
  page,
  store,
  hub,
  context
}) => {
  store.addProject({
    title: "Reconciliation Through Relationships",
    description: "Learning and matching across the city.",
    repositoryUrl: null,
    demoUrl: null
  });

  const administrator = new CountdownPage(page);
  await administrator.open();
  await administrator.login.signIn(ADMINISTRATOR_PASSCODE);

  const viewer = await openCompanion(context, store);
  const viewerCountdown = new CountdownPage(viewer.page);
  await viewerCountdown.open();
  await viewerCountdown.expectEntryPrompt();

  await administrator.openProjects();

  await expect(async () => expect(store.currentScreen).toBe("projects")).toPass();
  viewer.hub.pushEventUpdate(store.version);

  await expect(viewer.page).toHaveURL(/\/projects$/);
  await new ProjectsPage(viewer.page).expectCurrent();
});

test("L2-007/AC1: public entry is closed once Projects opens", async ({ page, store, context }) => {
  const administrator = new CountdownPage(page);
  await administrator.open();
  await administrator.login.signIn(ADMINISTRATOR_PASSCODE);

  await administrator.openProjects();
  await expect(async () => expect(store.currentScreen).toBe("projects")).toPass();

  // A viewer arriving now is taken to Projects and is offered no entry form.
  const viewer = await openCompanion(context, store);
  await viewer.page.goto("/");

  await expect(viewer.page).toHaveURL(/\/projects$/);
  await expect(viewer.page.locator("#entry-email")).toHaveCount(0);
  expect(store.participants).toHaveLength(0);
});

test("L2-007/AC2: Projects then Team selection then Raffle advance in order", async ({
  page,
  store
}) => {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  store.addParticipant("alan@example.com");

  // Administrator controls live on Countdown, and a viewer is held on the
  // current screen, so the only way in is to sign in before the event moves.
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.openProjects();
  await expect(async () => expect(store.currentScreen).toBe("projects")).toPass();

  const projects = new ProjectsPage(page);
  await projects.open();
  await projects.expectCurrent();

  await projects.formTeams();
  await expect(async () => expect(store.currentScreen).toBe("teams")).toPass();

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectCurrent();
  await teams.openRaffle();
  await expect(async () => expect(store.currentScreen).toBe("raffle")).toPass();

  // Opening the raffle draws nobody.
  expect(store.latestResult).toBeNull();
  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectWaiting();
});

test("L2-007/AC3: a stale advance is refused so no screen is skipped", async ({ page, store }) => {
  const administrator = new CountdownPage(page);
  await administrator.open();
  await administrator.login.signIn(ADMINISTRATOR_PASSCODE);

  // Another administrator advanced first, so this browser's version is stale.
  store.currentScreen = "projects";
  store.version += 1;

  await administrator.openProjects();

  await administrator.expectError("The screen did not advance. Refresh and try again.");
  expect(store.currentScreen).toBe("projects");
});

test("L2-007/AC4: a deep link to a future screen is returned to the saved one", async ({
  page,
  store
}) => {
  store.currentScreen = "countdown";

  const raffle = new RafflePage(page);
  await raffle.open();

  await expect(page).toHaveURL(/\/countdown$/);
  await new CountdownPage(page).expectEntryPrompt();
  expect(store.currentScreen).toBe("countdown");
});

test("L2-007/AC4: a refreshed browser lands on the saved screen without replaying", async ({
  page,
  store
}) => {
  store.addProject({
    title: "Reconciliation Through Relationships",
    description: "Learning and matching across the city.",
    repositoryUrl: null,
    demoUrl: null
  });
  store.currentScreen = "projects";
  const versionBefore = store.version;

  await page.goto("/");
  await expect(page).toHaveURL(/\/projects$/);

  await page.reload();

  await expect(page).toHaveURL(/\/projects$/);
  await new ProjectsPage(page).expectCurrent();
  expect(store.version).toBe(versionBefore);
});

test("L2-007/AC5: an administrator browses an earlier screen while viewers stay live", async ({
  page,
  store,
  hub,
  context
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);

  // The event moves on; this administrator keeps working on Countdown.
  store.currentScreen = "projects";
  store.version += 1;
  hub.pushEventUpdate(store.version);

  await expect(page).toHaveURL(/\/countdown$/);
  await countdown.roster.expectVisible();

  const viewer = await openCompanion(context, store);
  await viewer.page.goto("/");
  await expect(viewer.page).toHaveURL(/\/projects$/);
  expect(store.currentScreen).toBe("projects");
});

test("L2-007/AC6: the current screen does not change on its own", async ({ page, store }) => {
  store.countdownTargetUtc = new Date(Date.now() - 60_000).toISOString();

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectCountdownComplete();

  await page.waitForTimeout(2_000);

  expect(store.currentScreen).toBe("countdown");
  await expect(page).toHaveURL(/\/countdown$/);
  await countdown.expectEntryPrompt();
});

test.fixme(
  "L2-007/AC5: a locally browsed screen shows a current-screen indicator and a way back to live",
  async ({ page, store }) => {
    // Not implemented: an administrator can browse an earlier screen, but
    // nothing on it says which screen the event is actually showing, and there
    // is no "Return to live" action anywhere in the application.
    store.currentScreen = "projects";

    const countdown = new CountdownPage(page);
    await countdown.open();
    await countdown.login.signIn(ADMINISTRATOR_PASSCODE);

    await expect(page.getByRole("status")).toContainText("Projects");
    await page.getByRole("button", { name: "Return to live", exact: true }).click();
    await expect(page).toHaveURL(/\/projects$/);
  }
);
