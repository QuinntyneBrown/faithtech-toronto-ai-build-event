import { expect, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

export class AdminSchedulePage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.getByRole('link', { name: 'Edit schedule', exact: true }).click(); }
  async expectEmpty() { await expect(this.page.getByText('No stages yet.', { exact: true })).toBeVisible(); }
  async setWindow(kind: 'selection' | 'demo presentation', start: string, end: string) {
    await this.page.getByRole('checkbox', { name: 'Enable ' + kind, exact: true }).check();
    const label = kind[0].toUpperCase() + kind.slice(1);
    await this.page.getByLabel(label + ' start', { exact: true }).fill(start);
    await this.page.getByLabel(label + ' end', { exact: true }).fill(end);
  }
  async disableWindow(kind: string) { await this.page.getByRole('checkbox', { name: 'Enable ' + kind, exact: true }).uncheck(); }
  async expectWindow(kind: string, start: string, end: string) {
    const label = kind[0].toUpperCase() + kind.slice(1);
    await expect(this.page.getByLabel(label + ' start', { exact: true })).toHaveValue(start);
    await expect(this.page.getByLabel(label + ' end', { exact: true })).toHaveValue(end);
  }
  async expectWindowDisabled(kind: string) { await expect(this.page.getByRole('checkbox', { name: 'Enable ' + kind, exact: true })).not.toBeChecked(); }
  async expectError(message: string) { await expect(this.page.getByRole('alert')).toContainText(message); }
  async followValidation(label: string, message: string, summaryLabel = label, expectSummaryFocus = true) {
    const summary = this.page.getByRole('alert');
    if (expectSummaryFocus) await expect(summary).toBeFocused();
    await summary.getByRole('link', { name: summaryLabel + ': ' + message, exact: true }).click();
    const field = this.page.getByLabel(label, { exact: true });
    await expect(field).toBeFocused();
    await expect(field).toHaveAttribute('aria-invalid', 'true');
    await expect(field).toHaveAccessibleDescription(message);
  }
  async removeStage(name: string) { await this.page.getByRole('button', { name: 'Remove ' + name, exact: true }).click(); }
  async expectNoValidation() { await expect(this.page.getByRole('alert')).toHaveCount(0); }
  async expectStagesHeadingFocused() { await expect(this.page.getByRole('heading', { name: 'Stages and content', exact: true })).toBeFocused(); }
  async correctStageStart(local: string) {
    await this.page.getByLabel('Stage start', { exact: true }).fill(local);
    await this.page.getByRole('button', { name: 'Apply stage', exact: true }).click();
  }
  async retrySave() { await this.page.getByRole('button', { name: 'Retry schedule save', exact: true }).click(); }
  async expectUncertain() {
    await this.expectError('could not be confirmed');
    await expect(this.page.getByLabel('Timezone', { exact: true })).toBeDisabled();
    await expect(this.page.getByRole('checkbox', { name: 'Enable selection', exact: true })).toBeDisabled();
  }
  async reapply() { await this.page.getByRole('button', { name: 'Reapply my schedule', exact: true }).click(); }
  async beginStage() { await this.page.getByRole('button', { name: 'Add stage', exact: true }).click(); }
  async expectStageFocused() { await expect(this.page.getByLabel('Stage name', { exact: true })).toBeFocused(); }
  async cancelStage() { await this.page.getByRole('button', { name: 'Cancel', exact: true }).click(); }
  async expectAddFocused() { await expect(this.page.getByRole('button', { name: 'Add stage', exact: true })).toBeFocused(); }
  async returnToSettings() { await this.page.getByRole('link', { name: 'Event settings', exact: true }).click(); }
  async keepEditing() {
    await expect(this.page.getByRole('button', { name: 'Keep editing', exact: true })).toBeFocused();
    await this.page.getByRole('button', { name: 'Keep editing', exact: true }).click();
  }
  async expectAccessible(width: number) {
    await this.page.setViewportSize({ width, height: 1000 });
    expect((await new AxeBuilder({ page: this.page }).analyze()).violations).toEqual([]);
    expect(await this.page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  }
  async capture(path: string) { await this.page.screenshot({ path, fullPage: true }); }
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
