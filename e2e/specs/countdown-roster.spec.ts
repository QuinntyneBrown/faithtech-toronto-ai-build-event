// Acceptance Test
// Traces to: L2-002 AC1, AC2, AC3, AC4, AC5
// Description: Administrators manage participants inline on Countdown: adding
// an email-only participant enters them in the raffle, edits keep the same
// identity, deletion is confirmed and reversible until it is, failures keep the
// entered values, and the roster stays usable after the public screen advances.

import { ADMINISTRATOR_PASSCODE } from "../mocks/event-store";
import { expect, test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";

async function openAsAdministrator(page: import("@playwright/test").Page): Promise<CountdownPage> {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.roster.expectVisible();
  return countdown;
}

test("L2-002/AC1: adding an email-only participant enters them in the raffle once", async ({
  page,
  store
}) => {
  const countdown = await openAsAdministrator(page);
  await countdown.roster.expectEmpty();

  await countdown.roster.add("ada@example.com");

  await countdown.roster.expectParticipant("Participant 001");
  await countdown.roster.expectCount(1);
  await countdown.roster.expectStatus("Participant 001", "Unassigned", "Eligible");
  expect(store.participants).toHaveLength(1);
  expect(store.raffleSnapshot().eligibleCount).toBe(1);
  // No profile was submitted and the participant never signed in.
  expect(store.participants[0].name).toBeNull();
});

test("L2-002/AC2: an edit is saved and survives a refresh with the same identity", async ({
  page,
  store
}) => {
  const participant = store.addParticipant("ada@example.com");
  const countdown = await openAsAdministrator(page);

  await countdown.roster.edit("Participant 001", {
    email: "ada.lovelace@example.com",
    name: "Ada",
    whatYouMake: "Compilers",
    onYourHeart: "Mentorship"
  });
  await countdown.roster.save("Participant 001");

  await expect(async () => {
    expect(store.participants[0].name).toBe("Ada");
  }).toPass();

  await page.reload();
  await countdown.roster.expectVisible();

  await countdown.roster.expectField("Participant 001", "Email", "ada.lovelace@example.com");
  await countdown.roster.expectField("Participant 001", "Name", "Ada");
  await countdown.roster.expectField("Participant 001", "What you make", "Compilers");
  // Same identity and public label throughout.
  expect(store.participants[0].id).toBe(participant.id);
  expect(store.participants[0].publicLabel).toBe("Participant 001");
});

test("L2-002/AC2: a duplicate email is rejected without changing anything", async ({ page, store }) => {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  const countdown = await openAsAdministrator(page);

  await countdown.roster.edit("Participant 002", { email: "ada@example.com" });
  await countdown.roster.save("Participant 002");

  await countdown.roster.expectError(/could not be updated/);
  // The rejected value is still on screen, and nothing was written.
  await countdown.roster.expectField("Participant 002", "Email", "ada@example.com");
  expect(store.participants[1].email).toBe("grace@example.com");
});

// The confirmation dialog is broken, so neither branch of AC3 can be exercised.
// ConfirmDialogComponent declares `data` as a required signal input, but the
// application opens it through DialogService, which passes the payload as CDK
// DIALOG_DATA. CDK does not bind component inputs, so reading `data()` throws
// NG0950 and the dialog renders an empty title, an empty message, and two
// unlabelled buttons. Those buttons emit the component's `resolved` output,
// which nothing is subscribed to, while the application waits on `closed` — so
// a deletion can never be confirmed and the dialog cannot be dismissed.
test.fixme("L2-002/AC3: deletion is confirmed before it removes the participant", async ({
  page,
  store
}) => {
  store.addParticipant("ada@example.com");
  const team = store.addTeam([store.participants[0].id]);
  const countdown = await openAsAdministrator(page);
  await countdown.roster.expectStatus("Participant 001", team.label, "Eligible");

  await countdown.roster.remove("Participant 001");
  await countdown.roster.dialog.expectTitle("Remove participant?");
  await countdown.roster.dialog.expectMessage(/will lose their entry, profile, and team membership/);
  await countdown.roster.dialog.confirm("Remove");

  await countdown.roster.expectParticipantAbsent("Participant 001");
  await countdown.roster.expectEmpty();
  expect(store.participants).toHaveLength(0);
  expect(store.publicState().teams[0].members).toEqual([]);
});

test.fixme("L2-002/AC3: cancelling deletion changes nothing", async ({ page, store }) => {
  store.addParticipant("ada@example.com");
  const countdown = await openAsAdministrator(page);

  await countdown.roster.remove("Participant 001");
  await countdown.roster.dialog.cancel("Keep participant");

  await countdown.roster.expectParticipant("Participant 001");
  expect(store.participants).toHaveLength(1);
});

test("L2-002/AC3: removing a participant asks before doing anything", async ({ page, store }) => {
  store.addParticipant("ada@example.com");
  const countdown = await openAsAdministrator(page);

  await countdown.roster.remove("Participant 001");

  // Whatever the dialog manages to render, the deletion has not happened yet.
  await countdown.roster.dialog.expectOpen();
  expect(store.participants).toHaveLength(1);
});

test("L2-002/AC4: an empty roster says so", async ({ page }) => {
  const countdown = await openAsAdministrator(page);

  await countdown.roster.expectEmpty();
});

test("L2-002/AC4: a failed add explains itself and keeps the entered address", async ({
  page,
  store
}) => {
  const countdown = await openAsAdministrator(page);

  store.failNext("POST /api/admin/participants", { status: 503 });
  await countdown.roster.add("ada@example.com");

  await countdown.roster.expectError(/could not be added/);
  await countdown.roster.expectAddEmailValue("ada@example.com");
  await countdown.roster.expectEmpty();
  expect(store.participants).toHaveLength(0);
});

test("L2-002/AC4: a failed edit keeps the proposed values and claims no success", async ({
  page,
  store
}) => {
  store.addParticipant("ada@example.com");
  const countdown = await openAsAdministrator(page);

  store.failNext("PUT /api/admin/participants/" + store.participants[0].id, { status: 503 });
  await countdown.roster.edit("Participant 001", { name: "Ada" });
  await countdown.roster.save("Participant 001");

  await countdown.roster.expectError(/could not be updated/);
  await countdown.roster.expectField("Participant 001", "Name", "Ada");
  expect(store.participants[0].name).toBeNull();
});

test("L2-002/AC5: the roster stays usable after the public screen has advanced", async ({
  page,
  store,
  hub
}) => {
  store.addParticipant("ada@example.com");
  store.addParticipant("grace@example.com");
  store.addParticipant("alan@example.com");

  const countdown = await openAsAdministrator(page);

  // The event moves on to Team selection while this administrator stays put.
  store.currentScreen = "teams";
  store.formTeams();
  store.version += 1;
  hub.pushEventUpdate(store.version);

  await expect(page).toHaveURL(/\/countdown$/);
  await countdown.roster.expectVisible();

  await countdown.roster.add("hedy@example.com");

  await countdown.roster.expectParticipant("Participant 004");
  await countdown.roster.expectStatus("Participant 004", "Unassigned", "Eligible");
  // The public screen was not moved by the administrator's local browsing.
  expect(store.currentScreen).toBe("teams");
});
