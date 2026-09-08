import { expect, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

export class AdminAccessPage {
  constructor(private readonly page: Page) {}
  async open() { await this.page.goto('/sign-in'); }
  async signIn(username: string, password: string) {
    await this.page.getByLabel('Username', { exact: true }).fill(username);
    await this.page.getByLabel('Password', { exact: true }).fill(password);
    await this.page.getByRole('button', { name: 'Sign in', exact: true }).click();
  }
  async expectSignIn() { await expect(this.page.getByRole('heading', { name: 'Welcome back.' })).toBeVisible(); }
  async expectDenied() {
    await expect(this.page.getByRole('alert')).toContainText('Check your username and password');
    await expect(this.page).toHaveURL(/\/sign-in$/);
  }
  async expectAccessible() {
    const result = await new AxeBuilder({ page: this.page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa']).analyze();
    expect(result.violations).toEqual([]);
    expect(await this.page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
  }
  async useViewport(width: number) { await this.page.setViewportSize({ width, height: 900 }); }
  async capture(path: string) { await this.page.screenshot({ path, fullPage: true }); }
  async keyboardSignIn() {
    await this.page.keyboard.press('Tab');
    await expect(this.page.getByRole('link', { name: 'Skip to content' })).toBeFocused();
    await this.page.keyboard.press('Tab');
    await this.page.keyboard.type('host@example.com');
    await this.page.keyboard.press('Tab');
    await this.page.keyboard.type('host-demo');
    await this.page.keyboard.press('Tab');
    await this.page.keyboard.press('Enter');
  }
}
