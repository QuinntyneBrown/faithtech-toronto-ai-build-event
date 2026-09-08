import { expect, type Page } from '@playwright/test';

export class AdminAccessPage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.goto('/sign-in'); }
  async signIn(username: string, password: string) {
    await this.page.getByLabel('Username', { exact: true }).fill(username);
    await this.page.getByLabel('Password', { exact: true }).fill(password);
    await this.page.getByRole('button', { name: 'Sign in', exact: true }).click();
  }
  async expectSignedIn() { await expect(this.page.getByRole('heading', { name: 'Administrator session' })).toBeVisible(); }
  async expectSignIn() { await expect(this.page.getByRole('heading', { name: 'Welcome back.' })).toBeVisible(); }
  async expectDenied() {
    await expect(this.page.getByRole('alert')).toContainText('Check your username and password');
    await expect(this.page.getByRole('heading', { name: 'Administrator session' })).toHaveCount(0);
  }
  async signOut() { await this.page.getByRole('button', { name: 'Sign out', exact: true }).click(); }
}
