import { expect, type Page } from '@playwright/test';

export class AdminEventEditorPage {
  constructor(private readonly page: Page) {}
  async expectDraft(title: string) {
    await expect(this.page.getByRole('heading', { name: 'The details make it yours.' })).toBeVisible();
    await expect(this.page.getByLabel('Event title', { exact: true })).toHaveValue(title);
    await expect(this.page.getByText('Draft', { exact: true })).toBeVisible();
  }
}
