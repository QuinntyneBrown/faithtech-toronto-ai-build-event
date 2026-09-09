import type { Page } from "@playwright/test";
import { ADMINISTRATOR_PASSCODE } from "../mocks/event-store";
import { CountdownPage } from "../page-objects/countdown.page";

/**
 * Signs in as an administrator.
 *
 * The login is rendered only on Countdown, and a browser without a session is
 * held on the event's current screen, so this has to happen before the event
 * advances. Tests for later screens sign in first and navigate afterwards.
 */
export async function signInAsAdministrator(page: Page): Promise<CountdownPage> {
  const countdown = new CountdownPage(page);
  await countdown.open();
  await countdown.login.signIn(ADMINISTRATOR_PASSCODE);
  await countdown.login.expectSignedIn();
  return countdown;
}
