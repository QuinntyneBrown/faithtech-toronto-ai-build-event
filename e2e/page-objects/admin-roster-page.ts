import { expect, type Page } from '@playwright/test';

export class AdminRosterPage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.getByRole('link', { name: 'Manage participants', exact: true }).click(); }
  async expectEmpty() { await expect(this.page.getByText('No participants yet.', { exact: true })).toBeVisible(); }
  async add(name: string) {
    await this.page.getByRole('button', { name: 'Add participant', exact: true }).click();
    await this.page.getByLabel('Participant name', { exact: true }).fill(name);
    await this.page.getByRole('button', { name: 'Save participant', exact: true }).click();
  }
  async takeCode(name: string) {
    const dialog = this.page.getByRole('dialog', { name: 'Entry code for ' + name, exact: true });
    const code = dialog.getByLabel('New entry code', { exact: true });
    await expect(code).toBeFocused(); const value = await code.inputValue();
    await dialog.getByRole('button', { name: 'Done', exact: true }).click();
    await expect(this.page.getByLabel('New entry code', { exact: true })).toHaveCount(0);
    await expect(this.page.locator('body')).not.toContainText(value);
    return value;
  }
  async expectParticipants(name: string, count: number) { await expect(this.page.getByRole('cell', { name, exact: true })).toHaveCount(count); }
  async expectNoCode() { await expect(this.page.getByLabel('New entry code', { exact: true })).toHaveCount(0); }
}
