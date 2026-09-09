// Acceptance Test
// Traces to: L2-040 AC1, AC2, AC3, AC4
// Description: Text, email addresses, and links are validated at their stated
// limits. Boundary values save, oversized or malformed values are refused
// without a partial write and without losing the draft, saved text is rendered
// literally, and only absolute HTTPS links are accepted.

import { expect, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import { CountdownPage } from "../page-objects/countdown.page";
import { ProjectsPage } from "../page-objects/projects.page";

const DESCRIPTION = "A project description.";

test("L2-040/AC1: a title at the 200-character limit saves", async ({ page, store }) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  const title = "T".repeat(200);
  await projects.add({ title, description: DESCRIPTION });

  await expect(async () => expect(store.projects).toHaveLength(1)).toPass();
  expect(store.projects[0].title).toHaveLength(200);
});

test("L2-040/AC1: a title one character over the limit is refused with the draft kept", async ({
  page,
  store
}) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  const title = "T".repeat(201);
  await projects.add({ title, description: DESCRIPTION });

  await projects.expectError(/was not saved/);
  await projects.expectNewProjectValues({ title, description: DESCRIPTION });
  expect(store.projects).toHaveLength(0);
});

test("L2-040/AC1: a description one character over the limit is refused", async ({ page, store }) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.add({ title: "A project", description: "D".repeat(2_001) });

  await projects.expectError(/was not saved/);
  expect(store.projects).toHaveLength(0);
});

test("L2-040/AC1: whitespace-only required text is refused", async ({ page, store }) => {
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.add({ title: "   ", description: DESCRIPTION });

  await projects.expectError(/was not saved/);
  expect(store.projects).toHaveLength(0);
});

test("L2-040/AC1: an address longer than the limit cannot be entered", async ({ page }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.expectEmailLengthLimit(254);
});

test("L2-040/AC2: plus tags and subdomains are separate accepted addresses", async ({
  page,
  store
}) => {
  const countdown = await signInAsAdministrator(page);

  await countdown.roster.add("ada+raffle@example.com");
  await countdown.roster.expectParticipant("Participant 001");

  await countdown.roster.add("ada@mail.example.com");
  await countdown.roster.expectParticipant("Participant 002");

  expect(store.participants.map(participant => participant.email)).toEqual([
    "ada+raffle@example.com",
    "ada@mail.example.com"
  ]);
});

test("L2-040/AC2: casing and surrounding whitespace resolve to one address", async ({
  page,
  store
}) => {
  const countdown = await signInAsAdministrator(page);

  await countdown.roster.add("ada@example.com");
  await countdown.roster.expectParticipant("Participant 001");

  await countdown.roster.add("  ADA@Example.com  ");

  await countdown.roster.expectError(/could not be added/);
  expect(store.participants).toHaveLength(1);
});

for (const malformed of ["ada@", "@example.com", "ada@@example.com", "ada example@test.com"]) {
  test(`L2-040/AC2: "${malformed}" is refused`, async ({ page, store }) => {
    const countdown = await signInAsAdministrator(page);

    await countdown.roster.add(malformed);

    await countdown.roster.expectError(/could not be added/);
    expect(store.participants).toHaveLength(0);
  });
}

test("L2-040/AC3: markup and script text is rendered literally and never executed", async ({
  page,
  store
}) => {
  const hostile = '<img src=x onerror="window.__executed = true"> OR 1=1; --';
  store.addProject({
    title: "Injection check",
    description: hostile,
    repositoryUrl: null,
    demoUrl: null
  });
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  await projects.expectProject("Injection check", hostile);
  expect(await page.evaluate(() => (window as Record<string, unknown>).__executed)).toBeUndefined();
  await expect(page.locator("cs-card img")).toHaveCount(0);
});

for (const rejected of [
  "http://example.com/rtr",
  "/relative/path",
  "https://user:secret@example.com/rtr",
  "javascript:alert(1)"
]) {
  test(`L2-040/AC4: the link "${rejected}" is refused`, async ({ page, store }) => {
    store.addProject({
      title: "Links",
      description: DESCRIPTION,
      repositoryUrl: null,
      demoUrl: null
    });
    await signInAsAdministrator(page);
    store.currentScreen = "projects";

    const projects = new ProjectsPage(page);
    await projects.open();

    await projects.edit("Links", { repositoryUrl: rejected });
    await projects.save("Links");

    await projects.expectError(/was not saved/);
    expect(store.projects[0].repositoryUrl).toBeNull();
  });
}

test("L2-040/AC4: clearing an optional link removes it", async ({ page, store }) => {
  store.addProject({
    title: "Links",
    description: DESCRIPTION,
    repositoryUrl: "https://example.com/rtr",
    demoUrl: null
  });
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();
  await projects.expectLink("Links", "Repository", "https://example.com/rtr");

  await projects.edit("Links", { repositoryUrl: "" });
  await projects.save("Links");

  await expect(async () => expect(store.projects[0].repositoryUrl).toBeNull()).toPass();
  await projects.expectLinkAbsent("Links", "Repository");
});
