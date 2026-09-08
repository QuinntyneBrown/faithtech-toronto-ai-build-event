import { expect, type Page } from '@playwright/test';

export class AdminRosterPage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.getByRole('link', { name: 'Manage participants', exact: true }).click(); }
  async expectEmpty() { await expect(this.page.getByText('No participants yet.', { exact: true })).toBeVisible(); }
  async add(name: string) {
    await this.beginAdd(name);
    await this.page.getByRole('button', { name: 'Save participant', exact: true }).click();
  }
  async beginAdd(name: string) {
    await this.page.getByRole('button', { name: 'Add participant', exact: true }).click();
    await this.page.getByLabel('Participant name', { exact: true }).fill(name);
  }
  async cancelAdd() { await this.page.getByRole('button', { name: 'Cancel', exact: true }).click(); }
  async keepEditing() {
    const button = this.page.getByRole('button', { name: 'Keep editing', exact: true });
    await expect(button).toBeFocused(); await button.click();
  }
  async discard() { await this.page.getByRole('button', { name: 'Discard changes', exact: true }).click(); }
  async expectName(name: string) { await expect(this.page.getByLabel('Participant name', { exact: true })).toHaveValue(name); }
  async expectAddFocused() { await expect(this.page.getByRole('button', { name: 'Add participant', exact: true })).toBeFocused(); }
  async takeCode(name: string) {
    const dialog = this.page.getByRole('dialog', { name: 'Entry code for ' + name, exact: true });
    const code = dialog.getByLabel('New entry code', { exact: true });
    await expect(code).toBeFocused(); const value = await code.inputValue();
    await dialog.getByRole('button', { name: 'Done', exact: true }).click();
    await expect(this.page.getByLabel('New entry code', { exact: true })).toHaveCount(0);
    await expect(this.page.locator('body')).not.toContainText(value);
    await this.expectAddFocused();
    return value;
  }
  async expectParticipants(name: string, count: number) { await expect(this.page.getByRole('cell', { name, exact: true })).toHaveCount(count); }
  async expectNoCode() { await expect(this.page.getByLabel('New entry code', { exact: true })).toHaveCount(0); }
  async rename(currentName: string, newName: string) {
    await this.page.getByRole('button', { name: 'Rename ' + currentName, exact: true }).click();
    await this.page.getByLabel('New participant name', { exact: true }).fill(newName);
    await this.page.getByRole('button', { name: 'Save name', exact: true }).click();
  }
  async deactivate(name: string) {
    await this.page.getByRole('button', { name: 'Deactivate ' + name, exact: true }).click();
    await this.page.getByRole('button', { name: 'Confirm deactivation', exact: true }).click();
  }
  async expectStatus(name: string, status: 'Active' | 'Inactive') {
    await expect(this.page.getByRole('row', { name: new RegExp(name) }).getByRole('cell', { name: status, exact: true })).toBeVisible();
  }
  async expectDeactivateDisabled(name: string) {
    await expect(this.page.getByRole('button', { name: 'Deactivate ' + name, exact: true })).toBeDisabled();
  }
  async addWithLostResponse(name: string) {
    await this.beginAdd(name);
    await this.page.getByRole('button', { name: 'Save participant', exact: true }).click();
    await this.page.getByRole('button', { name: 'Check addition outcome', exact: true }).click();
  }
  async expectCodeAlreadyIssued(name: string) {
    const dialog = this.page.getByRole('dialog', { name: 'Entry code for ' + name, exact: true });
    await expect(dialog.getByText('a code was already issued', { exact: false })).toBeVisible();
  }
  async replaceCode() { await this.page.getByRole('button', { name: 'Replace the entry code', exact: true }).click(); }
  async reactivate(name: string) { await this.page.getByRole('button', { name: 'Reactivate ' + name, exact: true }).click(); }
  async expectReactivateDisabled(name: string) {
    await expect(this.page.getByRole('button', { name: 'Reactivate ' + name, exact: true })).toBeDisabled();
  }
}
