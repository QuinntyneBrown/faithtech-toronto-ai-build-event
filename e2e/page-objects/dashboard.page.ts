import { expect, Page } from '@playwright/test';

export class DashboardPage {
  constructor(readonly page: Page) {}
  async open() { await this.page.goto('/'); await expect(this.page.getByRole('heading', { name: 'Project completion' })).toBeVisible(); }
  async expectBaseline() { await expect(this.page.getByTestId('remaining')).toHaveText('34'); await expect(this.page.getByRole('button', { name: /L2-009/ })).toBeVisible(); }
  async search(text: string) { await this.page.getByRole('searchbox', { name: 'Search requirements' }).fill(text); }
  async expectNoMatches() { await expect(this.page.getByText('No requirements match these filters.')).toBeVisible(); }
  async inspect(id: string) { await this.page.getByRole('button', { name: new RegExp(id) }).click(); }
  async save(status: string, note: string) {
    await this.page.getByLabel('Review status', { exact: true }).selectOption(status);
    await this.page.getByLabel('Evidence / review note').fill(note);
    await this.page.getByRole('button', { name: 'Save review' }).click();
  }
  async expectRemaining(count: number) { await expect(this.page.getByTestId('remaining')).toHaveText(String(count)); }
  async expectMessage(text: RegExp) { await expect(this.page.getByRole('alert')).toContainText(text); }
  async closeInspector() { await this.page.getByRole('button', { name: 'Close inspector' }).click(); }
  async expectFocusOn(id: string) { await expect(this.page.getByRole('button', { name: new RegExp(id) })).toBeFocused(); }
  async expectFits() { expect(await this.page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true); }
  async export() {
    const download = this.page.waitForEvent('download');
    await this.page.getByRole('button', { name: 'Export progress' }).click();
    return await download;
  }
  async import(path: string) { await this.page.getByLabel('Import progress').setInputFiles(path); }
  async importInvalid() { await this.page.getByLabel('Import progress').setInputFiles({ name: 'invalid.json', mimeType: 'application/json', buffer: Buffer.from('{}') }); }
  async corruptStorage() { await this.page.addInitScript(() => localStorage.setItem('faithtech-completion-v1', '{invalid')); }
  async denyStorage() { await this.page.addInitScript(() => { Storage.prototype.setItem = () => { throw new Error('Storage disabled'); }; }); }
}
