// Acceptance Test
// Traces to: L2-036 AC1, AC2, AC3, AC4; L2-034 AC1
// Description: Every action can be completed from the keyboard, rejected
// submissions announce themselves and keep what was typed, the raffle's
// ticking label never floods assistive technology, contrast meets the stated
// thresholds, and a complete journey raises no blocking browser error.

import AxeBuilder from "@axe-core/playwright";
import { expect, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import { ADMINISTRATOR_PASSCODE } from "../mocks/event-store";
import type { EventStore } from "../mocks/event-store";
import { CountdownPage } from "../page-objects/countdown.page";
import { ProjectsPage } from "../page-objects/projects.page";
import { RafflePage } from "../page-objects/raffle.page";
import { TeamsPage } from "../page-objects/teams.page";

function seedEveryScreen(store: EventStore): void {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  store.addParticipant("alan@example.com");
  store.addProject({
    title: "Reconciliation Through Relationships",
    description: "Relationship, learning, and matching across the city.",
    repositoryUrl: "https://example.com/rtr",
    demoUrl: null
  });
  store.formTeams();
}

test("L2-036/AC1: a participant can enter using only the keyboard", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectEntryEnabled();

  await countdown.expectSubmitReachableByTab();
  await countdown.enterByKeyboard("ada@example.com");

  await countdown.expectEntered("Participant 001");
  expect(store.participants).toHaveLength(1);
});

test("L2-036/AC1: optional details can be saved using only the keyboard", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enterByKeyboard("ada@example.com");
  await countdown.profile.expectOffered();

  await countdown.profile.fillByKeyboard({ name: "Ada" });
  await countdown.profile.saveByKeyboard();

  await expect(async () => expect(store.participants[0].name).toBe("Ada")).toPass();
});

test("L2-036/AC1: an administrator can sign in and add a participant from the keyboard", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.login.signInByKeyboard(ADMINISTRATOR_PASSCODE);
  await countdown.login.expectSignedIn();

  await countdown.roster.addByKeyboard("ada@example.com");

  await countdown.roster.expectParticipant("Participant 001");
  expect(store.participants).toHaveLength(1);
});

test("L2-036/AC2: a rejected submission announces itself and keeps what was typed", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  store.failNext("POST /api/participant/entries", { status: 503 });
  await countdown.enter("ada@example.com");

  // The message is in a live region, so it is announced rather than only shown.
  await expect(page.getByRole("alert").filter({ hasText: /could not enter you/ })).toBeVisible();
  await countdown.expectEmailValue("ada@example.com");
});

test("L2-036/AC3: the ticking raffle label is hidden from assistive technology", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 5_000;
  store.seedDraw();

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectDrawing();

  // The name that changes many times a second is not announced.
  await expect(page.locator("p.cycling")).toHaveAttribute("aria-hidden", "true");
  // A single steady live region carries the announcement instead.
  await raffle.expectDrawingAnnouncement();
  await expect(page.getByRole("status")).toHaveCount(1);
});

test("L2-036/AC3: the revealed winner is announced in one live region", async ({ page, store }) => {
  seedEveryScreen(store);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 0;
  store.seedDraw();

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.expectWinnerAnnounced("Participant 001");
  await expect(page.getByRole("status")).toHaveCount(1);
});

for (const screen of ["countdown", "projects", "teams", "raffle"] as const) {
  test(`L2-036/AC4: the ${screen} screen meets the contrast thresholds`, async ({ page, store }) => {
    seedEveryScreen(store);
    if (screen === "raffle") {
      store.drawRevealDelayMs = 0;
      store.seedDraw();
    }
    store.currentScreen = screen;

    await page.goto("/" + screen);
    await expect(page).toHaveURL(new RegExp("/" + screen + "$"));

    const results = await new AxeBuilder({ page }).withTags(["wcag2aa", "wcag2a"]).analyze();

    expect(results.violations.map(violation => violation.id)).toEqual([]);
  });
}

test("L2-036/AC4: the administrator surfaces on Countdown meet the same thresholds", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  const countdown = await signInAsAdministrator(page);
  await countdown.roster.expectVisible();

  const results = await new AxeBuilder({ page }).withTags(["wcag2aa", "wcag2a"]).analyze();

  expect(results.violations.map(violation => violation.id)).toEqual([]);
});

test("L2-034/AC1: a complete journey raises no blocking browser error", async ({ page, store }) => {
  const failures: string[] = [];
  page.on("pageerror", error => failures.push("pageerror: " + error.message));
  page.on("console", message => {
    if (message.type() !== "error") return;
    // A browser with no session probes for one and is told 401. The browser
    // logs that, but it is the designed answer, not a blocking error.
    if (/Failed to load resource: the server responded with a status of 401/.test(message.text())) {
      return;
    }
    failures.push("console: " + message.text());
  });

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");
  await countdown.profile.fill({ name: "Ada" });
  await countdown.profile.save();

  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.roster.expectVisible();
  await countdown.roster.add("grace@example.com");
  await countdown.roster.add("alan@example.com");

  await countdown.openProjects();
  const projects = new ProjectsPage(page);
  await projects.open();
  await projects.add({ title: "RTR", description: "Reconciliation Through Relationships." });
  await projects.expectProject("RTR", "Reconciliation Through Relationships.");

  await projects.formTeams();
  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectCurrent();
  await teams.assignProject("Team 01", "RTR");
  await teams.expectAssignedProject("Team 01", "RTR");

  await teams.openRaffle();
  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.draw();
  await raffle.expectCurrent();

  expect(failures).toEqual([]);
});
