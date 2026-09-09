// Acceptance Test
// Traces to: L2-003 AC1, AC2, AC3, AC4, AC5; L2-004 AC1, AC3; L2-039 AC4
// Description: Public entry on Countdown takes an email only, confirms raffle
// entry with a stable public label, keeps that entry private to the browser
// that created it, and refuses invalid or closed entry without pretending to
// have succeeded.

import { expect, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";

test("L2-003/AC1: an unused valid email is entered into the raffle and offered optional details", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectEntryPrompt();

  await countdown.enter("ada@example.com");

  await countdown.expectEntered("Participant 001");
  await countdown.profile.expectOffered();
  expect(store.participants.map(participant => participant.email)).toEqual(["ada@example.com"]);
  expect(store.raffleSnapshot().eligibleCount).toBe(1);
});

test("L2-003/AC2: the creating browser keeps one entry across a repeated submission", async ({
  page,
  store
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  await page.reload();

  await countdown.expectEntered("Participant 001");
  expect(store.participants).toHaveLength(1);
});

test("L2-003/AC4: a malformed email is refused without confirming an entry", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.enter("not-an-email");

  await countdown.expectEntryError(/could not enter you into the raffle/);
  await countdown.expectNotEntered();
  await countdown.profile.expectNotOffered();
  expect(store.participants).toHaveLength(0);
});

test("L2-003/AC4: a failed save keeps the entered address and stays retryable", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  store.failNext("POST /api/participant/entries", { status: 503 });
  await countdown.enter("ada@example.com");

  await countdown.expectEntryError(/could not enter you into the raffle/);
  await countdown.expectNotEntered();
  await countdown.expectEmailValue("ada@example.com");
  expect(store.participants).toHaveLength(0);

  await countdown.enter("ada@example.com");

  await countdown.expectEntered("Participant 001");
  expect(store.participants).toHaveLength(1);
});

test("L2-003/AC4: throttled entry explains how long to wait", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  store.failNext("POST /api/participant/entries", {
    status: 429,
    headers: { "retry-after": "30" }
  });
  await countdown.enter("ada@example.com");

  await countdown.expectEntryError("Too many entry attempts. Try again in 30 seconds.");
  await countdown.expectNotEntered();
});

test("L2-004/AC1: a refreshed browser restores the entry and its saved details", async ({ page }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  await countdown.profile.fill({ name: "Ada", whatYouMake: "Compilers", onYourHeart: "Mentorship" });
  await countdown.profile.save();

  await page.reload();

  await countdown.expectEntered("Participant 001");
  await countdown.profile.expectValues({
    name: "Ada",
    whatYouMake: "Compilers",
    onYourHeart: "Mentorship"
  });
});

test("L2-004/AC3: leaving the browser clears the private entry but keeps the public screen", async ({
  page
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  await countdown.leaveThisBrowser();

  await countdown.expectEntryPrompt();
  await countdown.profile.expectNotOffered();
  await countdown.expectWelcome("Welcome to AI Build Night.");
});

test("L2-004/AC3: an invalidated entry session clears private content", async ({ page, hub }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.enter("ada@example.com");
  await countdown.expectEntered("Participant 001");

  hub.invalidateParticipantSession();

  await countdown.expectEntryPrompt();
  await countdown.profile.expectNotOffered();
});

test.fixme(
  "L2-003/AC3: a known email submitted without its session is told to ask an administrator",
  async ({ page, store }) => {
    // Not implemented: EntryService maps every failure to one generic string,
    // so the required "This email is already entered; ask an administrator to
    // update your details" guidance never reaches the screen.
    store.addParticipant("ada@example.com");

    const countdown = new CountdownPage(page);
    await countdown.open();
    await countdown.enter("ada@example.com");

    await countdown.expectEntryError(
      "This email is already entered; ask an administrator to update your details."
    );
    await countdown.expectNotEntered();
  }
);

test.fixme("L2-003/AC5: entry after Countdown closes is refused as closed", async ({ page, store }) => {
  // Not implemented: closing the screen produces the same generic entry
  // failure, so "Entry is closed; ask an administrator for help" is never shown.
  const countdown = new CountdownPage(page);
  await countdown.open();

  store.currentScreen = "projects";
  await countdown.enter("ada@example.com");

  await countdown.expectEntryError("Entry is closed; ask an administrator for help.");
});

test.fixme(
  "L2-039/AC4: the entry form explains enrolment, label use, and privacy before submission",
  async ({ page }) => {
    // Partly implemented: the form explains raffle enrolment and that the email
    // stays with the hosts, but says nothing before submission about the public
    // participant label or the privacy of the optional answers.
    const countdown = new CountdownPage(page);
    await countdown.open();

    await countdown.expectPrivacyNotice();
    await countdown.expectInformation(
      "You will be shown a public participant label. Your optional answers stay private."
    );
  }
);
