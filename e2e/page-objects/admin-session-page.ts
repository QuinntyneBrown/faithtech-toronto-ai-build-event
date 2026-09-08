import { expect, type Page } from '@playwright/test';

export class AdminSessionPage {
  constructor(private readonly page: Page) {}
  async expectSignedIn() { await expect(this.page.getByRole('heading', { name: 'Administrator session' })).toBeVisible(); }
  async signOut() { await this.page.getByRole('button', { name: 'Sign out', exact: true }).click(); }
  async restoreFromHistory() { await this.page.goBack(); }
  async refresh() { await this.page.reload(); }
  async retry() { await this.page.getByRole('button', { name: 'Retry', exact: true }).click(); }
  async expectPendingSignOut() { await expect(this.page.getByRole('alert')).toContainText('Sign-out is pending'); await this.expectPrivateContentCleared(); }
  async expectPrivateContentCleared() { await expect(this.page.getByRole('heading', { name: 'Administrator session' })).toHaveCount(0); }
}
