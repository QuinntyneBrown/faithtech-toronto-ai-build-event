import { expect, type Locator, type Page } from "@playwright/test";

export interface ProfileDetails {
  name?: string;
  whatYouMake?: string;
  onYourHeart?: string;
}

/**
 * The optional introduction offered after a participant has entered. It is a
 * section of the Countdown screen, scoped by the region label the app gives it.
 */
export class ProfileFormPanel {
  private readonly root: Locator;
  private readonly submit: Locator;

  constructor(private readonly page: Page) {
    this.root = page.getByRole("region", { name: "Optional introduction" });
    this.submit = this.root.getByRole("button", { name: "Save my details", exact: true });
  }

  async expectOffered(): Promise<void> {
    await expect(this.root).toBeVisible();
    await expect(this.root.getByRole("heading", { name: "A little about you", exact: true })).toBeVisible();
  }

  async expectNotOffered(): Promise<void> {
    await expect(this.root).toHaveCount(0);
  }

  async expectPrivacyNotice(): Promise<void> {
    await expect(
      this.root.getByText(
        "Your name may appear with your participant label. Your email and answers remain private to you and the hosts.",
        { exact: true }
      )
    ).toBeVisible();
  }

  async fill(details: ProfileDetails): Promise<void> {
    if (details.name !== undefined) {
      await this.root.getByLabel("Name", { exact: true }).fill(details.name);
    }
    if (details.whatYouMake !== undefined) {
      await this.root.getByLabel("What you make", { exact: true }).fill(details.whatYouMake);
    }
    if (details.onYourHeart !== undefined) {
      await this.root.getByLabel("What's on your heart", { exact: true }).fill(details.onYourHeart);
    }
  }

  async save(): Promise<void> {
    await this.submit.click();
  }

  /** Types without a pointer, for the keyboard-only criteria. */
  async fillByKeyboard(details: ProfileDetails): Promise<void> {
    if (details.name !== undefined) {
      const field = this.root.getByLabel("Name", { exact: true });
      await field.focus();
      await field.pressSequentially(details.name);
    }
  }

  async saveByKeyboard(): Promise<void> {
    await this.submit.focus();
    await this.submit.press("Enter");
  }

  async expectValues(details: Required<ProfileDetails>): Promise<void> {
    await expect(this.root.getByLabel("Name", { exact: true })).toHaveValue(details.name);
    await expect(this.root.getByLabel("What you make", { exact: true })).toHaveValue(details.whatYouMake);
    await expect(this.root.getByLabel("What's on your heart", { exact: true })).toHaveValue(
      details.onYourHeart
    );
  }

  async expectError(message: string | RegExp): Promise<void> {
    await expect(this.root.getByRole("alert")).toHaveText(message);
  }

  async expectSaveDisabled(): Promise<void> {
    await expect(this.submit).toBeDisabled();
  }

  async expectSaveEnabled(): Promise<void> {
    await expect(this.submit).toBeEnabled();
  }
}
