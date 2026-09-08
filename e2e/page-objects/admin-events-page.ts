import { expect, type Page } from '@playwright/test';

export class AdminEventsPage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.getByRole('link', { name: 'Manage events' }).click(); }
  async expectEmpty() { await expect(this.page.getByText('No events yet.')).toBeVisible(); }
  async openEvent(title: string) {
    await this.page.getByRole('listitem').filter({ hasText: title }).getByRole('link', { name: 'Open event' }).click();
  }
  async createDraft(title: string) {
    await this.page.getByRole('button', { name: 'Create event', exact: true }).click();
    await this.page.getByLabel('Event title', { exact: true }).fill(title);
    await this.page.getByRole('button', { name: 'Save draft', exact: true }).click();
  }
  async expectDraft(title: string) {
    const row = this.page.getByRole('listitem').filter({ hasText: title });
    await expect(row).toContainText('Draft');
  }
  async expectDialogClosed() { await expect(this.page.getByRole('dialog')).toHaveCount(0); }
  async cancelUnsavedDraft() {
    await this.page.getByRole('button', { name: 'Create event', exact: true }).click();
    await this.page.getByLabel('Event title', { exact: true }).fill('Unsaved title');
    await this.page.getByRole('button', { name: 'Cancel', exact: true }).click();
    await expect(this.page.getByRole('button', { name: 'Keep editing' })).toBeFocused();
    await this.page.getByRole('button', { name: 'Keep editing' }).click();
    await expect(this.page.getByLabel('Event title', { exact: true })).toBeFocused();
    await expect(this.page.getByLabel('Event title', { exact: true })).toHaveValue('Unsaved title');
    await this.page.getByRole('button', { name: 'Cancel', exact: true }).click();
    await this.page.getByRole('button', { name: 'Discard', exact: true }).click();
    await expect(this.page.getByRole('button', { name: 'Create event', exact: true })).toBeFocused();
  }
}
