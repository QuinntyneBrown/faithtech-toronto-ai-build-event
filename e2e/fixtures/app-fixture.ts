import { test as base, type BrowserContext, type Page } from "@playwright/test";
import { installApiMock } from "../mocks/api-mock";
import { EventStore } from "../mocks/event-store";
import { HubMock } from "../mocks/hub-mock";

export interface CompanionFixtures {
  /** The mocked API and its state. Seed it before navigating. */
  store: EventStore;
  /** The mocked realtime hub. Push updates and drop the transport through it. */
  hub: HubMock;
}

/**
 * Installs the API and hub mocks on the default page before anything can
 * navigate, so no test ever reaches a real backend.
 */
export const test = base.extend<CompanionFixtures>({
  store: async ({}, use) => {
    await use(new EventStore());
  },

  hub: async ({}, use) => {
    await use(new HubMock());
  },

  page: async ({ page, store, hub }, use) => {
    await hub.install(page);
    await installApiMock(page, store);
    await use(page);
  }
});

/**
 * Opens a second browser page against the same mocked API, with its own hub
 * connection. Used where a criterion describes one viewer observing another.
 */
export async function openCompanion(
  context: BrowserContext,
  store: EventStore
): Promise<{ page: Page; hub: HubMock }> {
  const page = await context.newPage();
  const hub = new HubMock();
  await hub.install(page);
  await installApiMock(page, store);
  return { page, hub };
}

export { expect } from "@playwright/test";
