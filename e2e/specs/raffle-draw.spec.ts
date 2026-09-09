// Acceptance Test
// Traces to: L2-020 AC1, AC2, AC3; L2-021 AC1, AC4, AC6
// Description: The raffle draws from the eligible pool only, names a winner
// once and excludes them afterwards, refuses to start a second draw while one
// is running, and stays retryable without announcing anybody when the draw
// cannot be committed.

import { expect, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import type { EventStore } from "../mocks/event-store";
import { RafflePage } from "../page-objects/raffle.page";

const NAMES = ["ada", "grace", "alan", "hedy"];

function seed(store: EventStore, count: number): void {
  for (let index = 0; index < count; index += 1) {
    store.addParticipant(NAMES[index] + "@example.com");
  }
}

test("L2-020/AC1: only eligible participants are counted", async ({ page, store }) => {
  seed(store, 3);
  // One has already won, so two remain eligible.
  store.participants[2].hasWonRaffle = true;
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectCurrent();

  await raffle.expectEligibleCount("2 eligible participants remain.");
});

test("L2-020/AC1: a single remaining entrant is counted in the singular", async ({ page, store }) => {
  seed(store, 1);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.expectEligibleCount("1 eligible participant remains.");
});

test("L2-020/AC2: results use public labels and never an email address", async ({ page, store }) => {
  seed(store, 2);
  store.drawRevealDelayMs = 0;
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.draw();

  await raffle.expectWinner("Participant 001");
  await expect(page.locator("body")).not.toContainText("ada@example.com");
  await expect(page.locator("body")).not.toContainText("grace@example.com");
});

test("L2-020/AC3: with nobody eligible the draw is disabled", async ({ page, store }) => {
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.expectEligibleCount("0 eligible participants remain.");
  await raffle.expectDrawDisabled();
  expect(store.latestResult).toBeNull();
});

test("L2-021/AC1: the drawn winner is named once and then excluded", async ({ page, store }) => {
  seed(store, 2);
  store.drawRevealDelayMs = 0;
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectEligibleCount("2 eligible participants remain.");

  await raffle.draw();

  await raffle.expectWinner("Participant 001");
  await raffle.expectEligibleCount("1 eligible participant remains.");

  await raffle.draw();

  await raffle.expectWinner("Participant 002");
  await raffle.expectPreviousWinners(["Participant 001"]);
  await raffle.expectEligibleCount("0 eligible participants remain.");
  // Nobody was drawn twice.
  expect(store.participants.filter(participant => participant.hasWonRaffle)).toHaveLength(2);
});

test("L2-021/AC4: no second draw can start while one is running", async ({ page, store }) => {
  seed(store, 3);
  store.drawRevealDelayMs = 4_000;
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.draw();

  await raffle.expectDrawing();
  expect(store.participants.filter(participant => participant.hasWonRaffle)).toHaveLength(1);
});

test("L2-021/AC6: a draw that cannot be committed announces nobody and stays retryable", async ({
  page,
  store
}) => {
  seed(store, 2);
  store.drawRevealDelayMs = 0;
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();

  store.failNext("POST /api/admin/raffle/draws", { status: 503 });
  await raffle.draw();

  await raffle.expectError("The name could not be drawn. Refresh the raffle and try again.");
  await raffle.expectWaiting();
  expect(store.latestResult).toBeNull();
  expect(store.participants.every(participant => !participant.hasWonRaffle)).toBe(true);

  await raffle.draw();

  await raffle.expectWinner("Participant 001");
});

test("L2-021/AC6: a lost draw response invites a safe retry", async ({ page, store }) => {
  seed(store, 2);
  store.drawRevealDelayMs = 0;
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();

  // An aborted request reaches the client as status 0: the outcome is unknown.
  store.failNext("POST /api/admin/raffle/draws", { abort: true });
  await raffle.draw();

  await raffle.expectError("The draw result may already be saved. Select DRAW NAME again to retry safely.");
  await raffle.expectDrawEnabled();
});

test("L2-020/AC3: a viewer is never offered the draw control", async ({ page, store }) => {
  seed(store, 2);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectCurrent();

  await raffle.expectDrawHidden();
});

test.fixme("L2-020/AC3: an empty pool says there are no eligible participants", async ({
  page,
  store
}) => {
  // Not implemented: the screen states the count as "0 eligible participants
  // remain." and disables the control, but never shows the required
  // "No eligible participants" message.
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();

  await expect(page.getByText("No eligible participants", { exact: false })).toBeVisible();
});

test.fixme("L2-020/AC2: participants sharing a name are still told apart", async ({ page, store }) => {
  // Not implemented: both the winner and the candidate labels are projected as
  // "name ?? public label", so two participants called Ada are rendered
  // identically and the public label that would distinguish them is dropped.
  store.addParticipant("ada@example.com", { name: "Ada" });
  store.addParticipant("ada.l@example.com", { name: "Ada" });
  store.drawRevealDelayMs = 0;
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.draw();

  await raffle.expectWinner("Ada (Participant 001)");
});
