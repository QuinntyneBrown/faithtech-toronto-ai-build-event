// Acceptance Test
// Traces to: L2-035 AC1, AC2, AC3
// Description: Every screen works from a small phone to a projected desktop.
// Nothing forces the page to scroll sideways at any supported width, the
// primary actions stay reachable on the smallest viewport, and a revealed
// winner is readable on the presentation display without scrolling.

import { expect, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import type { EventStore } from "../mocks/event-store";
import { CountdownPage } from "../page-objects/countdown.page";
import { RafflePage } from "../page-objects/raffle.page";

const WIDTHS = [320, 575, 576, 767, 768, 991, 992, 1199, 1200, 1440, 1920];

function seedEveryScreen(store: EventStore): void {
  store.addParticipant("ada@example.com", { name: "Ada" });
  store.addParticipant("grace@example.com");
  store.addParticipant("alan@example.com");
  store.addProject({
    title: "Reconciliation Through Relationships",
    description:
      "A long description that has to wrap sensibly on a narrow phone as well as on a projector.",
    repositoryUrl: "https://example.com/rtr",
    demoUrl: "https://example.com/rtr/demo"
  });
  store.formTeams();
}

async function expectNoSidewaysScrolling(page: import("@playwright/test").Page): Promise<void> {
  const overflow = await page.evaluate(() => {
    const root = document.documentElement;
    return root.scrollWidth - root.clientWidth;
  });
  expect(overflow).toBeLessThanOrEqual(1);
}

for (const screen of ["countdown", "projects", "teams", "raffle"] as const) {
  test(`L2-035/AC1: the ${screen} screen never scrolls sideways at any supported width`, async ({
    page,
    store
  }) => {
    seedEveryScreen(store);
    if (screen === "raffle") store.seedDraw();
    store.currentScreen = screen;

    await page.goto("/" + screen);
    await expect(page).toHaveURL(new RegExp("/" + screen + "$"));

    for (const width of WIDTHS) {
      await page.setViewportSize({ width, height: 900 });
      await expectNoSidewaysScrolling(page);
    }
  });
}

test("L2-035/AC1: the populated administrator roster never scrolls the page sideways", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  const countdown = await signInAsAdministrator(page);
  await countdown.roster.expectVisible();

  for (const width of WIDTHS) {
    await page.setViewportSize({ width, height: 900 });
    await expectNoSidewaysScrolling(page);
  }
});

test("L2-035/AC1: empty and error states hold up at every width", async ({ page, store }) => {
  store.failAlways("GET /api/event/state", { status: 503 });

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectError("We could not load the event. Please try again.");

  for (const width of WIDTHS) {
    await page.setViewportSize({ width, height: 900 });
    await expectNoSidewaysScrolling(page);
  }
});

test("L2-035/AC2: the primary actions stay reachable on the smallest viewport", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  await page.setViewportSize({ width: 320, height: 568 });

  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.expectEntryPrompt();
  await countdown.enter("hedy@example.com");

  await countdown.expectEntered("Participant 004");
  await countdown.profile.expectOffered();
  await expectNoSidewaysScrolling(page);
});

test("L2-035/AC3: a revealed winner is readable on the presentation display", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 0;
  store.seedDraw();
  await page.setViewportSize({ width: 1920, height: 1080 });

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectWinner("Ada");

  // Visible in the viewport without scrolling to it.
  const winner = page.locator("h2.winner");
  const box = await winner.boundingBox();
  expect(box).not.toBeNull();
  expect(box!.y).toBeGreaterThanOrEqual(0);
  expect(box!.y + box!.height).toBeLessThanOrEqual(1080);

  // The public view carries no private roster fields.
  await expect(page.locator("body")).not.toContainText("ada@example.com");
  await expect(page.locator("body")).not.toContainText("grace@example.com");
});
