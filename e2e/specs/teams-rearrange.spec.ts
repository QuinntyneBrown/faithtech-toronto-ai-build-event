// Acceptance Test
// Traces to: L2-009 AC1, AC2, AC3, AC4, AC5; L2-011 AC3, AC4
// Description: An administrator rearranges team members and assigns projects.
// Moves are atomic and reach other views, a new team gets its own label and no
// project, sizes are never rebalanced behind the administrator, a rejected move
// leaves the confirmed memberships authoritative, and ordinary participants
// have no way to move themselves.

import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import type { EventStore } from "../mocks/event-store";
import type { HubMock } from "../mocks/hub-mock";
import { TeamsPage } from "../page-objects/teams.page";

const NAMES = ["ada", "grace", "alan", "hedy", "katherine", "dorothy", "mary", "jean"];

function seed(store: EventStore, count: number): void {
  for (let index = 0; index < count; index += 1) {
    store.addParticipant(NAMES[index] + "@example.com");
  }
}

/**
 * Forms the teams and tells the open browser about it. The roster's team
 * control lists the teams from the event state, so the state has to move
 * forward before the control can offer them.
 */
function formTeams(store: EventStore, hub: HubMock): void {
  store.currentScreen = "teams";
  store.formTeams();
  store.version += 1;
  hub.pushEventUpdate(store.version);
}

test("L2-009/AC1: moving a member changes one membership and reaches other views", async ({
  page,
  store,
  hub,
  context
}) => {
  seed(store, 6);
  const countdown = await signInAsAdministrator(page);
  formTeams(store, hub);
  await countdown.roster.expectStatus("Participant 001", "Team 01", "Eligible");

  const viewer = await openCompanion(context, store);
  const viewerTeams = new TeamsPage(viewer.page);
  await viewer.page.goto("/");
  await viewerTeams.expectMembers("Team 01", ["Participant 001", "Participant 002", "Participant 003"]);

  await countdown.roster.moveToTeam("Participant 001", "Team 02");

  await countdown.roster.expectStatus("Participant 001", "Team 02", "Eligible");
  viewer.hub.pushEventUpdate(store.version);

  await viewerTeams.expectMembers("Team 01", ["Participant 002", "Participant 003"]);
  await viewerTeams.expectMembers("Team 02", [
    "Participant 001",
    "Participant 004",
    "Participant 005",
    "Participant 006"
  ]);
});

test("L2-009/AC2: an unassigned participant joins an existing team exactly once", async ({
  page,
  store,
  hub
}) => {
  seed(store, 3);
  const countdown = await signInAsAdministrator(page);
  formTeams(store, hub);

  store.addParticipant("hedy@example.com");
  store.version += 1;
  hub.pushEventUpdate(store.version);
  await countdown.roster.expectStatus("Participant 004", "Unassigned", "Eligible");

  await countdown.roster.moveToTeam("Participant 004", "Team 01");

  await countdown.roster.expectStatus("Participant 004", "Team 01", "Eligible");
  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectMembers("Team 01", [
    "Participant 001",
    "Participant 002",
    "Participant 003",
    "Participant 004"
  ]);
});

test("L2-009/AC2: a new team gets its own label and no project", async ({ page, store, hub }) => {
  seed(store, 3);
  const project = store.addProject({
    title: "RTR",
    description: "Reconciliation Through Relationships.",
    repositoryUrl: null,
    demoUrl: null
  });
  const countdown = await signInAsAdministrator(page);
  formTeams(store, hub);
  store.teams[0].projectId = project.id;
  store.version += 1;
  hub.pushEventUpdate(store.version);

  await countdown.roster.moveToTeam("Participant 003", "New team");

  await countdown.roster.expectStatus("Participant 003", "Team 02", "Eligible");
  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectMembers("Team 02", ["Participant 003"]);
  await teams.expectNoAssignedProject("Team 02");
  await teams.expectAssignedProject("Team 01", "RTR");
});

test("L2-009/AC3: a fourth member can be added without any rebalancing", async ({
  page,
  store,
  hub
}) => {
  seed(store, 6);
  const countdown = await signInAsAdministrator(page);
  formTeams(store, hub);
  await countdown.roster.expectStatus("Participant 004", "Team 02", "Eligible");

  await countdown.roster.moveToTeam("Participant 004", "Team 01");

  await countdown.roster.expectStatus("Participant 004", "Team 01", "Eligible");
  const teams = new TeamsPage(page);
  await teams.open();
  // Four and two: nothing was redistributed to even them out.
  await teams.expectTeamSizes([4, 2]);
});

test("L2-009/AC4: a rejected move keeps the confirmed membership and explains the retry", async ({
  page,
  store,
  hub
}) => {
  seed(store, 6);
  const countdown = await signInAsAdministrator(page);
  formTeams(store, hub);
  await countdown.roster.expectStatus("Participant 001", "Team 01", "Eligible");

  store.failNext("POST /api/admin/teams/moves", { status: 503 });
  await countdown.roster.moveToTeam("Participant 001", "Team 02");

  await countdown.roster.expectError("The team change was not saved. Refresh and try again.");
  expect(store.participants[0].teamId).toBe(store.teams[0].id);

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectMembers("Team 01", ["Participant 001", "Participant 002", "Participant 003"]);
  await teams.expectTeamSizes([3, 3]);
});

test("L2-009/AC5: a move leaves each team's project where it was", async ({ page, store, hub }) => {
  seed(store, 6);
  const first = store.addProject({
    title: "Project P",
    description: "The first project.",
    repositoryUrl: null,
    demoUrl: null
  });
  const second = store.addProject({
    title: "Project Q",
    description: "The second project.",
    repositoryUrl: null,
    demoUrl: null
  });
  const countdown = await signInAsAdministrator(page);
  formTeams(store, hub);
  store.teams[0].projectId = first.id;
  store.teams[1].projectId = second.id;
  store.version += 1;
  hub.pushEventUpdate(store.version);
  await countdown.roster.expectStatus("Participant 001", "Team 01", "Eligible");

  await countdown.roster.moveToTeam("Participant 001", "Team 02");

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectAssignedProject("Team 01", "Project P");
  await teams.expectAssignedProject("Team 02", "Project Q");
  await teams.expectMembers("Team 02", [
    "Participant 001",
    "Participant 004",
    "Participant 005",
    "Participant 006"
  ]);
});

test("L2-009/AC5: an ordinary participant is offered no way to move anyone", async ({
  page,
  store
}) => {
  seed(store, 6);
  store.currentScreen = "teams";
  store.formTeams();

  const teams = new TeamsPage(page);
  await teams.open();
  await teams.expectCurrent();

  await teams.expectAssignmentHidden("Team 01");
  await expect(page.locator("select")).toHaveCount(0);
});

test("L2-011/AC3: assigning a project reaches viewers without disturbing anything else", async ({
  page,
  store,
  context
}) => {
  seed(store, 6);
  store.addProject({
    title: "RTR",
    description: "Reconciliation Through Relationships.",
    repositoryUrl: null,
    demoUrl: null
  });
  await signInAsAdministrator(page);
  store.currentScreen = "teams";
  store.formTeams();

  const teams = new TeamsPage(page);
  await teams.open();

  const viewer = await openCompanion(context, store);
  const viewerTeams = new TeamsPage(viewer.page);
  await viewer.page.goto("/");
  await viewerTeams.expectNoAssignedProject("Team 01");

  await teams.assignProject("Team 01", "RTR");

  await teams.expectAssignedProject("Team 01", "RTR");
  viewer.hub.pushEventUpdate(store.version);
  await viewerTeams.expectAssignedProject("Team 01", "RTR");

  // Memberships and the other team are untouched.
  await viewerTeams.expectNoAssignedProject("Team 02");
  await viewerTeams.expectTeamSizes([3, 3]);
});

test("L2-011/AC4: two teams share a project and clearing one leaves the other", async ({
  page,
  store
}) => {
  seed(store, 6);
  store.addProject({
    title: "RTR",
    description: "Reconciliation Through Relationships.",
    repositoryUrl: null,
    demoUrl: null
  });
  await signInAsAdministrator(page);
  store.currentScreen = "teams";
  store.formTeams();

  const teams = new TeamsPage(page);
  await teams.open();

  await teams.assignProject("Team 01", "RTR");
  await teams.expectAssignedProject("Team 01", "RTR");
  await teams.assignProject("Team 02", "RTR");
  await teams.expectAssignedProject("Team 02", "RTR");

  await teams.clearProject("Team 01");

  await teams.expectNoAssignedProject("Team 01");
  await teams.expectAssignedProject("Team 02", "RTR");
});

test.fixme("L2-009/AC1: a member can be dragged from one team onto another", async ({ page }) => {
  // Not implemented: there is no drag and drop anywhere in the application. The
  // criterion's "or selects B using the equivalent control" half is covered by
  // the roster's team control, which the tests above exercise.
  const teams = new TeamsPage(page);
  await teams.open();
  await expect(page.locator("[cdkDrag]")).toHaveCount(1);
});
