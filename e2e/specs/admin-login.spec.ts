// Acceptance Test
// Traces to: L2-038 AC1, AC2, AC3, AC4, AC5; L2-041 AC4; L2-042 AC4
// Description: The shared four-digit passcode enables administrator controls
// in one browser only, refuses everything else, clears privileged state when
// the session ends, and explains refusals without disclosing the passcode.

import { ADMINISTRATOR_PASSCODE } from "../mocks/event-store";
import { expect, openCompanion, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";

test("L2-038/AC1: the provisioned passcode enables administrator controls", async ({ page }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.roster.expectHidden();

  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);

  await countdown.login.expectSignedIn();
  await countdown.roster.expectVisible();
  await countdown.expectAdvanceVisible();
});

for (const rejected of ["42", "abcd", "0043", " 042"]) {
  test(`L2-038/AC1: "${rejected}" does not authenticate`, async ({ page }) => {
    const countdown = new CountdownPage(page);
    await countdown.open();

    await countdown.login.signIn(rejected);

    await countdown.login.expectError("That passcode was not accepted.");
    await countdown.roster.expectHidden();
    await countdown.expectAdvanceHidden();
  });
}

test("L2-038/AC1: more than four digits cannot be entered at all", async ({ page }) => {
  // The field is capped at four characters, so a fifth digit is never submitted
  // rather than being submitted and refused.
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.open();

  await countdown.login.typePasscode("00423");

  await countdown.login.expectPasscodeValue("0042");
});

test("L2-038/AC2: a browser without a session sees and reaches no privileged surface", async ({
  page,
  store
}) => {
  store.addParticipant("ada@example.com");

  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.roster.expectHidden();
  await countdown.expectAdvanceHidden();
  // The private address is never rendered to a public viewer.
  await expect(page.locator("body")).not.toContainText("ada@example.com");
});

test("L2-038/AC3: signing in grants privileges to that browser only", async ({
  page,
  store,
  context
}) => {
  store.addParticipant("ada@example.com");

  const administrator = new CountdownPage(page);
  await administrator.open();
  await administrator.login.signIn(ADMINISTRATOR_PASSCODE);
  await administrator.roster.expectVisible();

  const viewer = await openCompanion(context, store);
  const publicView = new CountdownPage(viewer.page);
  await publicView.open();

  await publicView.login.expectSignedOut();
  await publicView.roster.expectHidden();
  await expect(viewer.page.locator("body")).not.toContainText("ada@example.com");
});

test("L2-038/AC4: signing out clears the privileged view", async ({ page }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.roster.expectVisible();

  await countdown.login.signOut();

  await countdown.login.expectSignedOut();
  await countdown.roster.expectHidden();
  await countdown.expectAdvanceHidden();
});

test("L2-038/AC4: an invalidated administrator session clears the privileged view", async ({
  page,
  hub
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.roster.expectVisible();

  hub.invalidateAdministratorSession();

  await countdown.roster.expectHidden();
  await countdown.login.expectSignedOut();
});

test("L2-038/AC5: with no passcode provisioned, no guess grants access", async ({ page, store }) => {
  store.provisionedPasscode = null;

  const countdown = new CountdownPage(page);
  await countdown.open();

  for (const guess of ["0000", "1234", ADMINISTRATOR_PASSCODE]) {
    await countdown.login.submitPasscode(guess);
    await countdown.login.expectError("That passcode was not accepted.");
  }

  await countdown.roster.expectHidden();
  // Public viewing is unaffected by the missing configuration.
  await countdown.expectWelcome(store.welcome);
  await countdown.expectEntryEnabled();
});

test("L2-042/AC4: a refused passcode is explained without disclosing the passcode", async ({
  page
}) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.login.signIn("1234");

  await countdown.login.expectError("That passcode was not accepted.");
  await expect(page.locator("body")).not.toContainText(ADMINISTRATOR_PASSCODE);
});

test("L2-042/AC4: throttled sign-in explains the retry delay", async ({ page, store }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  store.failNext("POST /api/admin/session", { status: 429, headers: { "retry-after": "45" } });
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);

  await countdown.login.expectError("Too many attempts. Try again in 45 seconds.");
  await countdown.roster.expectHidden();
});

test("L2-042/AC4: an ended session asks for the passcode again", async ({ page, hub }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.roster.expectVisible();

  hub.invalidateAdministratorSession();

  await countdown.login.expectSignedOut();
  await countdown.roster.expectHidden();
  await countdown.expectAdvanceHidden();
});

test("L2-041/AC4: history navigation after signing out exposes no cached roster", async ({
  page,
  store
}) => {
  store.addParticipant("ada@example.com");

  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.roster.expectVisible();
  await countdown.roster.expectParticipant("Participant 001");

  await countdown.login.signOut();
  await countdown.roster.expectHidden();

  await page.goBack();
  await page.goForward();

  await countdown.roster.expectHidden();
  await expect(page.locator("body")).not.toContainText("ada@example.com");
});
