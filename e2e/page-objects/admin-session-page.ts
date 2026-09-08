import { expect, type Page } from '@playwright/test';

export class AdminSessionPage {
  constructor(private readonly page: Page) {}
  async expectSignedIn() { await expect(this.page.getByRole('heading', { name: 'Administrator session' })).toBeVisible(); }
  async signOut() { await this.page.getByRole('button', { name: 'Sign out', exact: true }).click(); }
  async restoreFromHistory() { await this.page.goBack(); }
  async expectPrivateContentCleared() { await expect(this.page.getByRole('heading', { name: 'Administrator session' })).toHaveCount(0); }
}
