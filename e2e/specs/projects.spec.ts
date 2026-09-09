// Acceptance Test
// Traces to: L2-011 AC1, AC2; L2-012 AC1, AC2, AC3
// Description: The Projects screen shows the RTR project cards to everyone and
// lets an administrator maintain them inline. Missing links never block
// viewing, supplied links open safely, and a rejected save keeps the draft
// without changing the saved catalogue.

import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import { ProjectsPage } from "../page-objects/projects.page";

const RTR = {
  title: "Reconciliation Through Relationships",
  description: "Relationship, learning, and matching across the city.",
  repositoryUrl: null,
  demoUrl: null
};

test("L2-011/AC1: the RTR card is readable without an invented repository or demo", async ({
  page,
  store
}) => {
  store.addProject(RTR);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();
  await projects.expectCurrent();

  await projects.expectProject(RTR.title, RTR.description);
  await projects.expectLinkAbsent(RTR.title, "Repository");
  await projects.expectLinkAbsent(RTR.title, "Demo");
});

test("L2-011/AC2: an empty catalogue says so and offers the administrator an add action", async ({
  page,
  store
}) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.expectEmpty();
  await projects.expectAddAvailable();
});

test("L2-011/AC2: a supplied link opens its target without control of this page", async ({
  page,
  store
}) => {
  store.addProject({
    ...RTR,
    repositoryUrl: "https://example.com/rtr",
    demoUrl: "https://example.com/rtr/demo"
  });
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.expectLink(RTR.title, "Repository", "https://example.com/rtr");
  await projects.expectLink(RTR.title, "Demo", "https://example.com/rtr/demo");
  await projects.expectLinkIsolated(RTR.title, "Repository");
  await projects.expectLinkIsolated(RTR.title, "Demo");
});

test("L2-012/AC1: an added project reaches other viewers and survives a refresh", async ({
  page,
  store,
  context
}) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  const viewer = await openCompanion(context, store);
  const viewerProjects = new ProjectsPage(viewer.page);
  await viewer.page.goto("/");
  await viewerProjects.expectEmpty();

  await projects.add({ title: RTR.title, description: RTR.description });

  await projects.expectProject(RTR.title, RTR.description);
  viewer.hub.pushEventUpdate(store.version);
  await viewerProjects.expectProject(RTR.title, RTR.description);

  await page.reload();
  await projects.expectProject(RTR.title, RTR.description);
});

test("L2-012/AC1: an edited project updates its card", async ({ page, store }) => {
  store.addProject(RTR);
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.edit(RTR.title, { description: "Now with mentors from across the city." });
  await projects.save(RTR.title);

  await projects.expectProject(RTR.title, "Now with mentors from across the city.");
  await expect(async () => {
    expect(store.projects[0].description).toBe("Now with mentors from across the city.");
  }).toPass();
});

test("L2-012/AC2: a blank required field is refused and the draft is kept", async ({
  page,
  store
}) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.add({ title: "", description: RTR.description });

  await projects.expectError(/was not saved/);
  await projects.expectNewProjectValues({ title: "", description: RTR.description });
  await projects.expectEmpty();
  expect(store.projects).toHaveLength(0);
});

test("L2-012/AC2: an invalid link is refused and the catalogue is unchanged", async ({
  page,
  store
}) => {
  store.addProject(RTR);
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.edit(RTR.title, { repositoryUrl: "http://example.com/insecure" });
  await projects.save(RTR.title);

  await projects.expectError(/was not saved/);
  await projects.expectEditValue(RTR.title, "Repository URL", "http://example.com/insecure");
  expect(store.projects[0].repositoryUrl).toBeNull();
});

test("L2-012/AC2: a failed save keeps the proposed values", async ({ page, store }) => {
  store.addProject(RTR);
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  store.failNext("PUT /api/admin/projects/" + store.projects[0].id, { status: 503 });
  await projects.edit(RTR.title, { description: "A description that never lands." });
  await projects.save(RTR.title);

  await projects.expectError(/was not saved/);
  await projects.expectEditValue(RTR.title, "Description", "A description that never lands.");
  expect(store.projects[0].description).toBe(RTR.description);
});

test("L2-012/AC3: removing a project asks before doing anything", async ({ page, store }) => {
  store.addProject(RTR);
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.remove(RTR.title);

  await projects.dialog.expectOpen();
  expect(store.projects).toHaveLength(1);
});

// Blocked by the same confirm-dialog defect as participant deletion:
// ConfirmDialogComponent needs its data as a bound input, but DialogService
// supplies it as CDK DIALOG_DATA, so the dialog throws NG0950, renders no
// labels, and never resolves through `closed`.
test.fixme("L2-012/AC3: a confirmed removal clears the card and every assignment", async ({
  page,
  store
}) => {
  const project = store.addProject(RTR);
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  const first = store.addTeam([store.participants[0].id], project.id);
  const second = store.addTeam([store.participants[1].id], project.id);
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.remove(RTR.title);
  await projects.dialog.expectTitle("Remove project?");
  await projects.dialog.confirm("Remove");

  await projects.expectProjectAbsent(RTR.title);
  expect(store.teams.find(team => team.id === first.id)?.projectId).toBeNull();
  expect(store.teams.find(team => team.id === second.id)?.projectId).toBeNull();
  expect(store.publicState().teams[0].members).toHaveLength(1);
});
