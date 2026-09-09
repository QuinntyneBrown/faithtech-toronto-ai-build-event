import { expect, type Locator, type Page } from "@playwright/test";

/**
 * The inline administrator login. It is an overlay on the Countdown screen,
 * not a screen of its own, which is why it is a panel rather than a page.
 */
export class AdministratorLoginPanel {
  private readonly openControl: Locator;
  private readonly passcodeField: Locator;
  private readonly submit: Locator;
  private readonly signOutControl: Locator;
  private readonly form: Locator;

  constructor(private readonly page: Page) {
    this.openControl = page.getByRole("button", { name: "Administrator login", exact: true });
    this.passcodeField = page.locator("#administrator-passcode");
    this.form = page.locator('form[aria-label="Administrator login"]');
    this.submit = this.form.getByRole("button", { name: "Sign in", exact: true });
    this.signOutControl = page.getByRole("button", { name: "Sign out", exact: true });
  }

  async open(): Promise<void> {
    await this.openControl.click();
    await expect(this.passcodeField).toBeVisible();
  }

  async submitPasscode(passcode: string): Promise<void> {
    await this.ensureOpen();
    await this.passcodeField.fill(passcode);
    await this.submit.click();
  }

  /** Types key by key, so the field's own length limit applies. */
  async typePasscode(passcode: string): Promise<void> {
    await this.passcodeField.pressSequentially(passcode);
  }

  async signIn(passcode: string): Promise<void> {
    await this.submitPasscode(passcode);
  }

  async signOut(): Promise<void> {
    await this.signOutControl.click();
  }

  async expectSignedIn(): Promise<void> {
    await expect(
      this.page.getByText("Administrator controls are enabled in this browser.", { exact: true })
    ).toBeVisible();
  }

  /**
   * Signed out covers two shapes: the collapsed "Administrator login" button,
   * and the expanded form, which stays expanded when a session ends while it
   * is open.
   */
  async expectSignedOut(): Promise<void> {
    await expect(
      this.page.getByText("Administrator controls are enabled in this browser.", { exact: true })
    ).toHaveCount(0);
    await expect(this.openControl.or(this.passcodeField)).toBeVisible();
  }

  /** Opens the form only when it is still collapsed. */
  async ensureOpen(): Promise<void> {
    if (await this.openControl.count()) await this.open();
    await expect(this.passcodeField).toBeVisible();
  }

  async expectError(message: string | RegExp): Promise<void> {
    await expect(this.form.getByRole("alert")).toHaveText(message);
  }

  /** The passcode field keeps only what the browser accepted, e.g. four digits. */
  async expectPasscodeValue(value: string): Promise<void> {
    await expect(this.passcodeField).toHaveValue(value);
  }
}
