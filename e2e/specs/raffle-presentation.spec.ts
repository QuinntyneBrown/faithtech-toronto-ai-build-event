// Acceptance Test
// Traces to: L2-022 AC1, AC2, AC3, AC4, AC5; L2-037 AC1, AC2, AC3
// Description: The draw is presented on the saved timeline. Names cycle and
// settle on the committed winner at the reveal, celebration stops on time, a
// browser joining late picks up the remaining timeline rather than replaying
// it, and reduced motion or failed effects still leave the winner as readable
// text.

import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { signInAsAdministrator } from "../fixtures/administrator";
import type { EventStore } from "../mocks/event-store";
import { RafflePage } from "../page-objects/raffle.page";

const NAMES = ["ada", "grace", "alan", "hedy"];

function seed(store: EventStore, count: number): void {
  for (let index = 0; index < count; index += 1) {
    store.addParticipant(NAMES[index] + "@example.com");
  }
}

test("L2-022/AC1: names cycle and then settle on the committed winner", async ({ page, store }) => {
  seed(store, 4);
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 2_000;

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.draw();

  await raffle.expectDrawing();
  await raffle.expectCyclingName();
  const cycling = await raffle.readCyclingName();
  expect(store.latestResult?.candidateLabels).toContain(cycling);

  await raffle.expectWinner("Participant 001");
  await raffle.expectCelebrationCopy();
});

test("L2-022/AC1: the celebration stops after the saved effects window", async ({ page, store }) => {
  seed(store, 2);
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";
  // The reveal has to still be ahead when the result arrives; a result whose
  // reveal has already passed is treated as a late arrival and never celebrates.
  store.drawRevealDelayMs = 2_000;

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.draw();

  await raffle.expectWinner("Participant 001");
  await raffle.expectParticlesRunning();

  // The stage stops celebrating five seconds after the reveal.
  await raffle.expectParticlesStopped();
  await raffle.expectWinner("Participant 001");
});

test("L2-022/AC2: a browser joining mid-draw picks up the remaining timeline", async ({
  page,
  store
}) => {
  seed(store, 4);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 4_000;
  // The draw started three seconds ago, so only one second is left.
  store.seedDraw(3_000);

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.expectDrawing();

  await raffle.expectWinner("Participant 001");
});

test("L2-022/AC2: a browser arriving after the reveal shows the winner without replaying", async ({
  page,
  store
}) => {
  seed(store, 4);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 1_000;
  // Long past both the reveal and the end of the celebration.
  store.seedDraw(60_000);

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.expectWinner("Participant 001");
  await raffle.expectParticlesStopped();
});

test("L2-022/AC3: the winner is revealed even without a working GPU celebration", async ({
  page,
  store
}) => {
  seed(store, 3);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 0;
  store.seedDraw();

  const raffle = new RafflePage(page);
  await raffle.open();

  // Headless Chrome has no WebGPU, so this is the fallback presentation.
  await raffle.expectWinner("Participant 001");
  await raffle.expectWinnerAnnounced("Participant 001");
});

test("L2-022/AC5: the winner is exposed as announced text, not only animation", async ({
  page,
  store
}) => {
  seed(store, 3);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 0;
  store.seedDraw();

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.expectWinnerAnnounced("Participant 001");
});

test("L2-037/AC3: a draw in progress is announced without relying on the cycling name", async ({
  page,
  store
}) => {
  seed(store, 4);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 5_000;
  store.seedDraw();

  const raffle = new RafflePage(page);
  await raffle.open();

  await raffle.expectDrawing();
  await raffle.expectDrawingAnnouncement();
});

test("L2-037/AC2: stopping the effects keeps the saved result on screen", async ({ page, store }) => {
  seed(store, 3);
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 2_000;

  const raffle = new RafflePage(page);
  await raffle.open();
  await raffle.draw();
  await raffle.expectParticlesRunning();

  await raffle.stopEffects();

  await raffle.expectParticlesStopped();
  // The committed result still arrives at its reveal.
  await raffle.expectWinner("Participant 001");
});

test("L2-037/AC2: one viewer stopping effects does not change another view", async ({
  page,
  store,
  context
}) => {
  seed(store, 3);
  await signInAsAdministrator(page);
  store.currentScreen = "raffle";
  store.drawRevealDelayMs = 1_000;

  const raffle = new RafflePage(page);
  await raffle.open();

  // Both browsers are watching before the draw starts, so both celebrate.
  const viewer = await openCompanion(context, store);
  const viewerRaffle = new RafflePage(viewer.page);
  await viewer.page.goto("/");
  await viewerRaffle.expectWaiting();

  await raffle.draw();
  viewer.hub.pushEventUpdate(store.version);

  await raffle.expectParticlesRunning();
  await viewerRaffle.expectParticlesRunning();

  await raffle.stopEffects();

  await raffle.expectParticlesStopped();
  // The other viewer's own presentation is untouched.
  await viewerRaffle.expectWinner("Participant 001");
  await viewerRaffle.expectParticlesRunning();
});

test.describe("with reduced motion", () => {
  test("L2-022/AC4: cycling and particles are omitted and the winner still appears", async ({
    page,
    store
  }) => {
    seed(store, 4);
    store.currentScreen = "raffle";
    store.drawRevealDelayMs = 3_000;
    store.seedDraw();

    await page.emulateMedia({ reducedMotion: "reduce" });
    const raffle = new RafflePage(page);
    await raffle.open();

    await raffle.expectDrawing();
    await raffle.expectStaticDrawingLabel();
    await raffle.expectParticlesStopped();

    await raffle.expectWinner("Participant 001");
    await raffle.expectWinnerAnnounced("Participant 001");
  });

  test("L2-037/AC1: a revealed winner stays readable with motion suppressed", async ({
    page,
    store
  }) => {
    seed(store, 3);
    store.currentScreen = "raffle";
    store.drawRevealDelayMs = 0;
    store.seedDraw();

    await page.emulateMedia({ reducedMotion: "reduce" });
    const raffle = new RafflePage(page);
    await raffle.open();

    await raffle.expectWinner("Participant 001");
    await raffle.expectParticlesStopped();
  });
});
