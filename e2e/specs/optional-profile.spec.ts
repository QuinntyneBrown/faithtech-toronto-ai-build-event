// Acceptance Test
// Traces to: L2-013 AC1, AC2, AC3, AC4, AC5
// Description: The optional introduction is never a condition of taking part.
// Blank fields keep raffle eligibility, saved values come back, clearing a
// field removes it, a failed save never suggests the raffle entry failed, and
// the answers stay private to the participant and the administrator.

import { ADMINISTRATOR_PASSCODE } from "../mocks/event-store";
import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";

test("L2-013/AC1: leaving every optional field blank keeps the raffle entry", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  await countdown.profile.expectOffered();
  await countdown.profile.save();

  await countdown.expectEntered("Participant 001");
  expect(store.raffleSnapshot().eligibleCount).toBe(1);
  expect(store.participants[0].name).toBeNull();
});

test("L2-013/AC1: no optional detail is needed to follow the event onwards", async ({
  page,
  store,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  store.currentScreen = "projects";
  store.version += 1;
  hub.pushEventUpdate(store.version);

  await expect(page).toHaveURL(/\/projects$/);
  expect(store.raffleSnapshot().eligibleCount).toBe(1);
});

test("L2-013/AC2: a saved subset comes back after a refresh", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");

  await countdown.profile.fill({ name: "Ada", whatYouMake: "Compilers" });
  await countdown.profile.save();
  await expect(async () => expect(store.participants[0].name).toBe("Ada")).toPass();

  await page.reload();

  await countdown.profile.expectValues({
    name: "Ada",
    whatYouMake: "Compilers",
    onYourHeart: ""
  });
});

test("L2-013/AC2: clearing the name removes it and leaves the public label", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.profile.fill({ name: "Ada" });
  await countdown.profile.save();
  await expect(async () => expect(store.participants[0].name).toBe("Ada")).toPass();

  await countdown.profile.fill({ name: "" });
  await countdown.profile.save();

  await expect(async () => expect(store.participants[0].name).toBeNull()).toPass();
  await countdown.expectEntered("Participant 001");
});

test("L2-013/AC3: a failed optional save keeps the draft and confirms the entry stands", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  store.failNext("PUT /api/participant/profile", { status: 503 });
  await countdown.profile.fill({ name: "Ada", whatYouMake: "Compilers" });
  await countdown.profile.save();

  await countdown.profile.expectError("We could not save those details. Your raffle entry is still confirmed.");
  await countdown.profile.expectValues({ name: "Ada", whatYouMake: "Compilers", onYourHeart: "" });
  await countdown.expectEntered("Participant 001");
  expect(store.raffleSnapshot().eligibleCount).toBe(1);
});

test("L2-013/AC4: answers stay private to the participant and the administrator", async ({
  page,
  store,
  context
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.profile.fill({ name: "Ada", onYourHeart: "A private burden" });
  await countdown.profile.save();
  await expect(async () => expect(store.participants[0].onYourHeart).toBe("A private burden")).toPass();

  // Another viewer sees nothing of it.
  const viewer = await openCompanion(context, store);
  const publicView = new CountdownPage(viewer.page);
  await publicView.open();
  await publicView.expectEntryPrompt();
  await expect(viewer.page.locator("body")).not.toContainText("A private burden");
  await expect(viewer.page.locator("body")).not.toContainText("ada@example.com");

  // The administrator can read them on Countdown.
  await publicView.login.signIn(ADMINISTRATOR_PASSCODE);
  await publicView.roster.expectVisible();
  await publicView.roster.expectField("Participant 001", "On your heart", "A private burden");
  await publicView.roster.expectField("Participant 001", "Email", "ada@example.com");
});

test.fixme(
  "L2-013/AC5: an unsaved draft is discarded with notice when Countdown closes",
  async ({ page, store, hub }) => {
    // Not implemented: the transition simply replaces the screen. Nothing tells
    // the participant that the details they had typed were not submitted.
    const countdown = new CountdownPage(page);
    await countdown.open();
    await countdown.enter("ada@example.com");
    await countdown.profile.fill({ name: "Ada" });

    store.currentScreen = "projects";
    store.version += 1;
    hub.pushEventUpdate(store.version);

    await expect(page).toHaveURL(/\/projects$/);
    await expect(page.getByRole("status")).toContainText("were not submitted");
    expect(store.participants[0].name).toBeNull();
    expect(store.raffleSnapshot().eligibleCount).toBe(1);
  }
);
