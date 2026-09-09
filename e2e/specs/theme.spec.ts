// Acceptance Test
// Traces to: L2-032 AC1, AC2
// Description: The companion commits to Cornerstone's light theme. Every
// screen renders in it whatever the operating system prefers, a preference
// that changes while viewing does not switch it, and no theme selector is
// offered.

import { expect, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import type { EventStore } from "../mocks/event-store";

async function expectLightTheme(page: import("@playwright/test").Page): Promise<void> {
  await expect(page.locator("html")).toHaveClass(/cs-theme-light/);
  await expect(page.locator("html")).not.toHaveClass(/cs-theme-dark/);
}

function seedEveryScreen(store: EventStore): void {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  store.addProject({
    title: "RTR",
    description: "Reconciliation Through Relationships.",
    repositoryUrl: null,
    demoUrl: null
  });
  store.formTeams();
}

for (const colorScheme of ["light", "dark"] as const) {
  test(`L2-032/AC1: every screen uses the light theme with a ${colorScheme} preference`, async ({
    page,
    store
  }) => {
    seedEveryScreen(store);
    store.seedDraw();
    await page.emulateMedia({ colorScheme });

    for (const screen of ["countdown", "projects", "teams", "raffle"] as const) {
      store.currentScreen = screen;
      await page.goto("/" + screen);
      await expectLightTheme(page);
    }
  });
}

test("L2-032/AC1: the administrator overlays keep the light theme", async ({ page, store }) => {
  seedEveryScreen(store);
  await page.emulateMedia({ colorScheme: "dark" });

  const countdown = await signInAsAdministrator(page);
  await countdown.roster.expectVisible();

  await expectLightTheme(page);
});

test("L2-032/AC2: a preference that changes while viewing does not switch the theme", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  await page.emulateMedia({ colorScheme: "light" });
  await page.goto("/countdown");
  await expectLightTheme(page);

  await page.emulateMedia({ colorScheme: "dark" });

  await expectLightTheme(page);
});

test("L2-032/AC2: a stored dark preference does not survive into the theme", async ({
  page,
  store
}) => {
  seedEveryScreen(store);
  await page.goto("/countdown");
  await page.evaluate(() => localStorage.setItem("cs-theme", "dark"));
  await page.emulateMedia({ colorScheme: "dark" });

  await page.reload();

  await expectLightTheme(page);
});

test("L2-032/AC2: no theme selector is offered anywhere", async ({ page, store }) => {
  seedEveryScreen(store);
  const countdown = await signInAsAdministrator(page);
  await countdown.roster.expectVisible();

  await expect(page.getByRole("button", { name: /theme|dark mode|light mode/i })).toHaveCount(0);
  await expect(page.getByRole("switch")).toHaveCount(0);
});
