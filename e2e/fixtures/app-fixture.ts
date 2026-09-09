import { test as base, type BrowserContext, type Page } from "@playwright/test";
import { installApiMock } from "../mocks/api-mock";
import { BrowserSession } from "../mocks/browser-session";
import { EventStore } from "../mocks/event-store";
import { HubMock } from "../mocks/hub-mock";

export interface CompanionFixtures {
  /** The mocked API and its event state. Seed it before navigating. */
  store: EventStore;
  /** This browser's own administrator and entry sessions. */
  session: BrowserSession;
  /** The mocked realtime hubs. Push updates and drop transports through them. */
  hub: HubMock;
}

/** One extra browser page against the same event, with its own sessions. */
export interface Companion {
  page: Page;
  session: BrowserSession;
  hub: HubMock;
}

/**
 * Installs the API and hub mocks on the default page before anything can
 * navigate, so no test ever reaches a real backend.
 *
 * The REST mock is registered last and therefore matches first; it defers the
 * two hub paths that live under `/api` back to the hub mock.
 */
export const test = base.extend<CompanionFixtures>({
  store: async ({}, use) => {
    await use(new EventStore());
  },

  session: async ({}, use) => {
    await use(new BrowserSession());
  },

  hub: async ({}, use) => {
    await use(new HubMock());
  },

  page: async ({ page, store, session, hub }, use) => {
    await hub.install(page);
    await installApiMock(page, store, session);
    await use(page);
  }
});

/**
 * Opens a second browser page against the same event. Its administrator and
 * entry sessions are its own, which is what lets a test prove that signing in
 * on one browser leaves the other a public viewer.
 */
export async function openCompanion(
  context: BrowserContext,
  store: EventStore
): Promise<Companion> {
  const page = await context.newPage();
  const session = new BrowserSession();
  const hub = new HubMock();
  await hub.install(page);
  await installApiMock(page, store, session);
  return { page, session, hub };
}

export { expect } from "@playwright/test";
