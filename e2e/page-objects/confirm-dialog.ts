import { expect, type Locator, type Page } from "@playwright/test";

/**
 * Cornerstone's confirmation dialog.
 *
 * It is rendered into a CDK overlay appended to the document body, outside the
 * application element, and it carries no dialog role, so it is addressed by its
 * component element. Keeping that knowledge here means no spec has to hold it.
 */
export class ConfirmDialog {
  private readonly root: Locator;

  constructor(page: Page) {
    this.root = page.locator("cs-confirm-dialog");
  }

  async expectOpen(): Promise<void> {
    await expect(this.root).toBeVisible();
  }

  async expectTitle(title: string): Promise<void> {
    await expect(this.root.getByRole("heading", { name: title, exact: true })).toBeVisible();
  }

  async expectMessage(message: string | RegExp): Promise<void> {
    await expect(this.root).toContainText(message);
  }

  async confirm(label: string): Promise<void> {
    await this.root.getByRole("button", { name: label, exact: true }).click();
    await expect(this.root).toHaveCount(0);
  }

  async cancel(label: string): Promise<void> {
    await this.root.getByRole("button", { name: label, exact: true }).click();
    await expect(this.root).toHaveCount(0);
  }

  async expectClosed(): Promise<void> {
    await expect(this.root).toHaveCount(0);
  }
}
