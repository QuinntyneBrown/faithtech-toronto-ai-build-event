import { expect, type Page } from '@playwright/test';

export class ClientAccessPage {
  constructor(private readonly page: Page) {}
  async open(eventId: string) { await this.page.goto(`/events/${eventId}/access`); }
  async expectUnavailable() { await expect(this.page.getByRole('heading', { name: "This event isn't available.", exact: true })).toBeVisible(); }
  async expectTitle(title: string) { await expect(this.page.getByText(title, { exact: false })).toBeVisible(); }
  async signIn(email: string, code: string) {
    await this.page.getByLabel('Email address', { exact: true }).fill(email);
    await this.page.getByLabel('Entry code', { exact: true }).fill(code);
    await this.page.getByRole('button', { name: 'Join the event', exact: true }).click();
  }
  async expectJoined() { await expect(this.page.getByRole('heading', { name: "You're in.", exact: true })).toBeVisible(); }
  async expectDenied() { await expect(this.page.getByRole('alert')).toBeVisible(); }
  async expectCodeCleared() { await expect(this.page.getByLabel('Entry code', { exact: true })).toHaveValue(''); }
}
