import { expect, type Page } from '@playwright/test';

export class AdminEventEditorPage {
  constructor(private readonly page: Page) {}
  async editContent(title: string, venue: string, waiting: string) {
    await this.page.getByLabel('Event title', { exact: true }).fill(title);
    await this.page.getByLabel('Venue name', { exact: true }).fill(venue);
    await this.page.getByLabel('Waiting content', { exact: true }).fill(waiting);
  }
  async save() { await this.page.getByRole('button', { name: 'Save draft', exact: true }).click(); }
  async setDirections(url: string) { await this.page.getByLabel('Directions link', { exact: true }).fill(url); }
  async expectInvalidDirections() {
    await expect(this.page.getByLabel('Directions link', { exact: true })).toHaveAttribute('aria-invalid', 'true');
    await expect(this.page.getByText('Use an absolute HTTPS URL without credentials.', { exact: true })).toBeVisible();
  }
  async expectConflict(currentTitle: string) {
    await expect(this.page.getByRole('alert')).toContainText('Another administrator saved changes');
    await expect(this.page.getByRole('region', { name: 'Current saved values' })).toContainText(currentTitle);
  }
  async reapply() { await this.page.getByRole('button', { name: 'Reapply my changes', exact: true }).click(); }
  async expectUnconfirmed() {
    await expect(this.page.getByRole('alert')).toContainText('could not be confirmed');
    await expect(this.page.getByLabel('Event title', { exact: true })).toBeDisabled();
  }
  async retrySave() { await this.page.getByRole('button', { name: 'Retry save', exact: true }).click(); }
  async expectSaved() { await expect(this.page.getByRole('status')).toHaveText('Draft saved.'); }
  async returnToEvents() { await this.page.getByRole('link', { name: 'All events' }).click(); }
  async expectContent(venue: string, waiting: string) {
    await expect(this.page.getByLabel('Venue name', { exact: true })).toHaveValue(venue);
    await expect(this.page.getByLabel('Waiting content', { exact: true })).toHaveValue(waiting);
  }
  async expectDraft(title: string) {
    await expect(this.page.getByRole('heading', { name: 'The details make it yours.' })).toBeVisible();
    await expect(this.page.getByLabel('Event title', { exact: true })).toHaveValue(title);
    await expect(this.page.getByText('Draft', { exact: true })).toBeVisible();
  }
}
