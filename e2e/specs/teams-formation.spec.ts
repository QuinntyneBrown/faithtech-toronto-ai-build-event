// Acceptance Test
// Traces to: L2-010 AC1, AC4, AC5
// Description: Opening Team selection forms the grouping once. The screen
// renders every entered identity exactly once, says so when there is nobody,
// keeps manual corrections when another viewer arrives, and leaves Projects
// current with no partial teams when formation fails.

import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import { ProjectsPage } from "../page-objects/projects.page";
import { TeamsPage } from "../page-objects/teams.page";

const NAMES = [
  "ada",
  "grace",
  "alan",
  "hedy",
  "katherine",
  "dorothy",
  "mary",
  "jean",
  "margaret",
  "annie",
  "evelyn",
  "kathleen",
  "frances"
];

function seed(store: { addParticipant: (email: string) => unknown }, count: number): void {
  for (let index = 0; index < count; index += 1) {
    store.addParticipant(NAMES[index] + "@example.com");
  }
}

test("L2-010/AC1: advancing to Team selection forms and renders the grouping once", async ({
  page,
  store
}) => {
  seed(store, 8);

  const countdown = await signInAsAdministrator(page);
  await countdown.openProjects();
  await expect(async () => expect(store.currentScreen).toBe("projects")).toPass();

  const projects = new ProjectsPage(page);
  await projects.open();
  await projects.formTeams();
  await expect(async () => expect(store.currentScreen).toBe("teams")).toPass();

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectCurrent();

  await teams.expectTeamSizes([3, 3, 2]);
  await teams.expectTeamLabels(["Team 01", "Team 02", "Team 03"]);

  // Every entered identity is rendered exactly once.
  const rendered = await page.locator("cs-card li").allInnerTexts();
  expect(rendered).toHaveLength(8);
  expect(new Set(rendered).size).toBe(8);
});

for (const [count, sizes] of [
  [1, [1]],
  [2, [2]],
  [3, [3]],
  [4, [3, 1]],
  [13, [3, 3, 3, 3, 1]]
] as [number, number[]][]) {
  test(`L2-010/AC1: ${count} entered participants render as ${sizes.join("/")}`, async ({
    page,
    store
  }) => {
    seed(store, count);
    store.currentScreen = "teams";
    store.formTeams();

    const teams = new TeamsPage(page);
    await teams.open();
    await teams.expectCurrent();

    await teams.expectTeamSizes(sizes);
    const rendered = await page.locator("cs-card li").allInnerTexts();
    expect(rendered).toHaveLength(count);
    expect(new Set(rendered).size).toBe(count);
  });
}

test("L2-010/AC1: no entered participants shows an explicit empty state", async ({ page, store }) => {
  store.currentScreen = "teams";
  store.formTeams();

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectCurrent();

  await teams.expectEmpty();
});

test("L2-010/AC4: a failed formation leaves Projects current with no partial teams", async ({
  page,
  store
}) => {
  seed(store, 4);
  const countdown = await signInAsAdministrator(page);
  await countdown.openProjects();
  await expect(async () => expect(store.currentScreen).toBe("projects")).toPass();

  const projects = new ProjectsPage(page);
  await projects.open();

  store.failNext("POST /api/admin/event/advance", { status: 503 });
  await projects.formTeams();

  await projects.expectError("The screen did not advance. Refresh and try again.");
  expect(store.currentScreen).toBe("projects");
  expect(store.teams).toHaveLength(0);

  // A retry forms one complete grouping.
  await projects.formTeams();

  await expect(async () => expect(store.currentScreen).toBe("teams")).toPass();
  expect(store.teams).toHaveLength(2);
});

test("L2-010/AC5: manual corrections survive another viewer opening the screen", async ({
  page,
  store,
  context
}) => {
  seed(store, 4);
  store.currentScreen = "teams";
  store.formTeams();
  // An administrator has already moved somebody into the smaller team.
  store.participants[0].teamId = store.teams[1].id;

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectTeamSizes([2, 2]);

  const viewer = await openCompanion(context, store);
  const viewerTeams = new TeamsPage(viewer.page);
  await viewer.page.goto("/");

  await viewerTeams.expectCurrent();
  await viewerTeams.expectTeamSizes([2, 2]);
  // Opening the screen again never reshuffles.
  expect(store.teams).toHaveLength(2);
});
