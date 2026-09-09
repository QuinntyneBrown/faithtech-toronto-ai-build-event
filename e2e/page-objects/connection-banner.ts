import { expect, type Locator, type Page } from "@playwright/test";

/**
 * The application shell's live-updates banner. It is the only place the app
 * tells a viewer that its authority is stale, and it gates every control that
 * changes server state.
 */
export class ConnectionBanner {
  private readonly retry: Locator;

  constructor(private readonly page: Page) {
    this.retry = page.getByRole("button", { name: "Retry live updates", exact: true });
  }

  async expectHidden(): Promise<void> {
    await expect(this.retry).toHaveCount(0);
  }

  async expectVisible(): Promise<void> {
    await expect(this.retry).toBeVisible();
  }

  async expectMessage(text: string | RegExp): Promise<void> {
    await expect(this.page.locator("main > cs-card").getByRole("alert")).toHaveText(text);
  }

  async retryLiveUpdates(): Promise<void> {
    await this.retry.click();
  }
}
