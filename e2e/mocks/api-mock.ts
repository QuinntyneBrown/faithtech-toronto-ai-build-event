import type { Page } from "@playwright/test";
import type { BrowserSession } from "./browser-session";
import type { EventStore } from "./event-store";
import { HUB_PATHS } from "./hub-mock";

/**
 * Answers every `/api/**` call from the in-memory store.
 *
 * Handlers are unlimited and idempotent on purpose: the client re-reads the
 * public state on every screen, after every mutation, and whenever the tab
 * becomes visible again, so a one-shot route would starve it.
 */
export async function installApiMock(
  page: Page,
  store: EventStore,
  session: BrowserSession
): Promise<void> {
  await page.route("**/api/**", async route => {
    const request = route.request();
    const method = request.method();
    const path = new URL(request.url()).pathname;

    // Two of the SignalR hubs live under /api. Their negotiation belongs to the
    // hub mock, not the store.
    if (HUB_PATHS.some(hub => path.startsWith(hub))) {
      await route.fallback();
      return;
    }

    let body: unknown = null;
    const raw = request.postData();
    if (raw) {
      try {
        body = JSON.parse(raw);
      } catch {
        body = raw;
      }
    }

    const forced = store.consumeForced(method + " " + path);
    if (forced?.abort) {
      // Reaches the client as status 0, the "lost response" path. With `commit`
      // the write is applied first, so the server has committed and the client
      // never found out.
      if (forced.commit) store.handle(method, path, body, session);
      await route.abort("failed");
      return;
    }

    const outcome =
      forced?.status === undefined ? store.handle(method, path, body, session) : forced;
    const payload = outcome.json === undefined ? "" : JSON.stringify(outcome.json);

    await route.fulfill({
      status: outcome.status ?? 200,
      headers: { "content-type": "application/json", ...(forced?.headers ?? {}) },
      body: payload
    });
  });
}
