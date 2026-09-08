import { expect, type Page } from '@playwright/test';

export class AdminSchedulePage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.getByRole('link', { name: 'Edit schedule', exact: true }).click(); }
  async expectEmpty() { await expect(this.page.getByText('No stages yet.', { exact: true })).toBeVisible(); }
  async setEventTimes(timezone: string, start: string, end: string) {
    await this.page.getByLabel('Timezone', { exact: true }).fill(timezone);
    await this.page.getByLabel('Event start', { exact: true }).fill(start);
    await this.page.getByLabel('Event end', { exact: true }).fill(end);
  }
  async addStage(name: string, start: string, end: string, content: string) {
    await this.page.getByRole('button', { name: 'Add stage', exact: true }).click();
    await this.page.getByLabel('Stage name', { exact: true }).fill(name);
    await this.page.getByLabel('Stage start', { exact: true }).fill(start);
    await this.page.getByLabel('Stage end', { exact: true }).fill(end);
    await this.page.getByLabel('Stage instructions', { exact: true }).fill(content);
    await this.page.getByRole('button', { name: 'Apply stage', exact: true }).click();
  }
  async save() { await this.page.getByRole('button', { name: 'Save schedule', exact: true }).click(); }
  async expectSaved() { await expect(this.page.getByText('Schedule saved.', { exact: true })).toBeVisible(); }
  async expectStage(name: string, content: string) {
    const row = this.page.getByRole('row').filter({ has: this.page.getByRole('button', { name: 'Edit ' + name, exact: true }) });
    await expect(row).toContainText(content);
  }
}
