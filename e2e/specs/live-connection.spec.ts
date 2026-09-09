// Acceptance Test
// Traces to: L2-044
// Description: With the API and the event-updates hub answered entirely from
// the test process, the companion loads the current event, reports a live
// connection, follows a pushed update, and disables every server-changing
// control while the transport is down.

import { test } from "../fixtures/app-fixture";
import { CountdownPage } from "../page-objects/countdown.page";

test("L2-044: a synchronized companion shows the event and enables entry", async ({ page, store }) => {
  const errors: string[] = [];
  page.on("pageerror", error => errors.push(error.message));

  const countdown = new CountdownPage(page);
  await countdown.open();

  await countdown.expectTitle(store.title);
  await countdown.expectWelcome(store.welcome);
  await countdown.expectInformation(store.purpose, store.venue);
  await countdown.banner.expectHidden();
  await countdown.expectEntryEnabled();

  if (errors.length) throw new Error("Unexpected page errors: " + errors.join(", "));
});

test("L2-044: a pushed event update refreshes the rendered state", async ({ page, store, hub }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectWelcome(store.welcome);

  store.welcome = "The evening has been reshaped.";
  store.version += 1;
  hub.pushEventUpdate(store.version);

  await countdown.expectWelcome("The evening has been reshaped.");
});

test("L2-044: losing the transport warns the viewer and disables changes", async ({ page, hub }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.expectEntryEnabled();

  hub.goOffline();

  await countdown.banner.expectVisible();
  await countdown.expectEntryDisabled();
});

test("L2-044: retrying live updates restores the connection and the controls", async ({ page, hub }) => {
  const countdown = new CountdownPage(page);
  await countdown.open();

  hub.goOffline();
  await countdown.banner.expectVisible();

  hub.goOnline();
  await countdown.banner.retryLiveUpdates();

  await countdown.banner.expectHidden();
  await countdown.expectEntryEnabled();
});
