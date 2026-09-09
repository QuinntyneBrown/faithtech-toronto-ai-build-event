// Acceptance Test
// Traces to: L2-044 AC2, AC3, AC4, AC5; L2-039 AC3
// Description: The client converges on the newest saved version whatever order
// the notifications arrive in, replaces stale state on reconnection without
// replaying anything, refuses to overwrite another editor silently, and does
// not duplicate work when a response is lost. Private views clear the moment
// their session is invalidated.
//
// These cover the client half only. Real SignalR delivery, and the two-second
// budget for it, belong to the API integration tests.

import { expect, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import { CountdownPage } from "../page-objects/countdown.page";
import { ProjectsPage } from "../page-objects/projects.page";
import { RafflePage } from "../page-objects/raffle.page";

test("L2-044/AC2: a repeated notification does not disturb the rendered state", async ({
  page,
  store,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectWelcome(store.welcome);

  store.welcome = "The evening has been reshaped.";
  store.version += 1;
  hub.pushEventUpdate(store.version);
  await countdown.expectWelcome("The evening has been reshaped.");

  // The same version again, twice.
  hub.pushEventUpdate(store.version);
  hub.pushEventUpdate(store.version);

  await countdown.expectWelcome("The evening has been reshaped.");
  await countdown.banner.expectHidden();
});

test("L2-044/AC2: an older notification never rolls the screen back", async ({
  page,
  store,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  // Entry is enabled only once the hub is connected, so this is the point at
  // which a pushed notification can actually be received.
  await countdown.expectEntryEnabled();

  store.welcome = "The newest saved welcome.";
  store.version += 5;
  hub.pushEventUpdate(store.version);
  await countdown.expectWelcome("The newest saved welcome.");

  // A late, out-of-order notification for a version already superseded.
  hub.pushEventUpdate(store.version - 3);

  await countdown.expectWelcome("The newest saved welcome.");
  await countdown.expectEntryEnabled();
});

test("L2-044/AC3: reconnecting replaces state that changed while disconnected", async ({
  page,
  store,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectWelcome(store.welcome);

  hub.goOffline();
  await countdown.banner.expectVisible();
  await countdown.expectEntryDisabled();

  // Several changes land while this browser is not listening.
  store.welcome = "First change.";
  store.version += 1;
  store.welcome = "Second change.";
  store.version += 1;
  store.purpose = "A third change.";
  store.version += 1;

  hub.goOnline();
  await countdown.banner.retryLiveUpdates();

  await countdown.expectWelcome("Second change.");
  await countdown.expectInformation("A third change.");
  await countdown.banner.expectHidden();
  await countdown.expectEntryEnabled();
});

test("L2-044/AC3: reconnecting after a completed draw does not replay it", async ({
  page,
  store,
  hub
}) => {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 1_000;

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectWaiting();

  hub.goOffline();
  await raffle.banner.expectVisible();

  // The draw happens and finishes entirely while this browser is away.
  store.seedDraw(30_000);

  hub.goOnline();
  await raffle.banner.retryLiveUpdates();

  await raffle.expectWinner("Participant 001");
  // Joining late shows the saved result, it does not re-run the celebration.
  await raffle.expectParticlesStopped();
});

test("L2-044/AC4: a conflicting save is refused and the draft is kept for reapplication", async ({
  page,
  store
}) => {
  const project = store.addProject({
    title: "RTR",
    description: "The committed description.",
    repositoryUrl: null,
    demoUrl: null
  });
  await signInAsAdministrator(page);
  store.currentScreen = "projects";

  const projects = new ProjectsPage(page);
  await projects.open();

  // Another administrator committed first, so this save is stale.
  store.failNext("PUT /api/admin/projects/" + project.id, { status: 409 });
  await projects.edit("RTR", { description: "My competing description." });
  await projects.save("RTR");

  await projects.expectError(/was not saved/);
  // Nothing was overwritten, and the rejected draft is still here to reapply.
  expect(store.projects[0].description).toBe("The committed description.");
  await projects.expectEditValue("RTR", "Description", "My competing description.");

  // Reapplying explicitly is all it takes.
  await projects.save("RTR");

  await expect(async () => {
    expect(store.projects[0].description).toBe("My competing description.");
  }).toPass();
});

test("L2-044/AC5: an entry whose response is lost is not duplicated by a retry", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  // The server commits the entry, then the response is lost on the way back.
  store.failNext("POST /api/participant/entries", { abort: true, commit: true });
  await countdown.enter("ada@example.com");

  await countdown.expectEntryError(/could not enter you into the raffle/);
  expect(store.participants).toHaveLength(1);

  await countdown.enter("ada@example.com");

  await countdown.expectEntered("Participant 001");
  expect(store.participants).toHaveLength(1);
  expect(store.raffleSnapshot().eligibleCount).toBe(1);
});

test("L2-044/AC5: a lost roster addition is not duplicated by a retry", async ({ page, store }) => {
  const countdown = await signInAsAdministrator(page);

  store.failNext("POST /api/admin/participants", { abort: true, commit: true });
  await countdown.roster.add("ada@example.com");

  await countdown.roster.expectError(/could not be added/);
  expect(store.participants).toHaveLength(1);

  // The administrator refreshes rather than adding again.
  await countdown.roster.refresh();

  await countdown.roster.expectParticipant("Participant 001");
  await countdown.roster.expectCount(1);
});

test("L2-039/AC3: an invalidated entry session clears the private view at once", async ({
  page,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");
  await countdown.profile.expectOffered();

  hub.invalidateParticipantSession();

  await countdown.profile.expectNotOffered();
  await countdown.expectEntryPrompt();
  // Public viewing is unaffected.
  await countdown.expectWelcome("Welcome to AI Build Night.");
});

test("L2-039/AC3: a reconnecting private view must reauthorize before showing content", async ({
  page,
  store,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  // The session is gone server-side, and the private transport drops.
  store.failAlways("GET /api/participant/session", { status: 401 });
  hub.participant.goOffline();

  await countdown.expectEntryPrompt();

  await page.reload();

  await countdown.expectEntryPrompt();
  await countdown.profile.expectNotOffered();
});
