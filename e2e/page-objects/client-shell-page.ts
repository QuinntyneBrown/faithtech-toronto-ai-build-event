import { expect, type Page } from '@playwright/test';

export class ClientShellPage {
  constructor(private readonly page: Page) {}
  async expectCurrentEvent() { await expect(this.page.getByRole('heading', { name: "You're in.", exact: true })).toBeVisible(); }
  async goTo(section: 'Schedule' | 'Teams & projects' | 'People' | 'Messages' | 'Quiz' | 'Raffle' | 'Showcase') {
    await this.page.getByRole('link', { name: section, exact: true }).click();
  }
  async expectStub(feature: string) {
    await expect(this.page.getByRole('heading', { name: `${feature} isn't ready yet.`, exact: true })).toBeVisible();
  }
  async signOut() { await this.page.getByRole('button', { name: 'Sign out', exact: true }).click(); }
}
