import { expect, type Locator, type Page } from "@playwright/test";
import { ConfirmDialog } from "./confirm-dialog";

export interface ParticipantEdit {
  email?: string;
  name?: string;
  whatYouMake?: string;
  onYourHeart?: string;
}

/**
 * The administrator-only participant roster on the Countdown screen.
 *
 * Every row repeats the same field labels and button names, so each row is
 * scoped through its own edit form, which the application labels with the
 * participant's public label.
 */
export class RosterPanel {
  readonly dialog: ConfirmDialog;

  private readonly heading: Locator;
  private readonly addForm: Locator;
  private readonly emailField: Locator;
  private readonly addSubmit: Locator;
  private readonly refreshControl: Locator;

  constructor(private readonly page: Page) {
    this.dialog = new ConfirmDialog(page);
    this.heading = page.getByRole("heading", { name: "Participant roster", exact: true });
    this.addForm = page.locator('form[aria-label="Add participant"]');
    this.emailField = page.locator("#roster-email");
    this.addSubmit = this.addForm.getByRole("button", { name: "Add participant", exact: true });
    this.refreshControl = page.getByRole("button", { name: "Refresh roster", exact: true });
  }

  /** The edit form for one participant, scoped by their public label. */
  row(publicLabel: string): Locator {
    return this.page.locator(`form[aria-label="Edit ${publicLabel}"]`);
  }

  async expectVisible(): Promise<void> {
    await expect(this.heading).toBeVisible();
  }

  async expectHidden(): Promise<void> {
    await expect(this.heading).toHaveCount(0);
  }

  async expectEmpty(): Promise<void> {
    await expect(this.page.getByRole("heading", { name: "No participants yet", exact: true })).toBeVisible();
  }

  async add(email: string): Promise<void> {
    await this.emailField.fill(email);
    await this.addSubmit.click();
  }

  /** Types and submits without a pointer, for the keyboard-only criteria. */
  async addByKeyboard(email: string): Promise<void> {
    await this.emailField.focus();
    await this.emailField.pressSequentially(email);
    await this.emailField.press("Enter");
  }

  async expectAddEnabled(): Promise<void> {
    await expect(this.addSubmit).toBeEnabled();
  }

  async expectAddDisabled(): Promise<void> {
    await expect(this.addSubmit).toBeDisabled();
  }

  async expectAddEmailValue(email: string): Promise<void> {
    await expect(this.emailField).toHaveValue(email);
  }

  async refresh(): Promise<void> {
    await this.refreshControl.click();
  }

  async expectParticipant(publicLabel: string): Promise<void> {
    await expect(this.row(publicLabel)).toBeVisible();
  }

  async expectParticipantAbsent(publicLabel: string): Promise<void> {
    await expect(this.row(publicLabel)).toHaveCount(0);
  }

  async expectCount(count: number): Promise<void> {
    await expect(this.page.locator('form[aria-label^="Edit Participant"]')).toHaveCount(count);
  }

  async expectField(publicLabel: string, label: string, value: string): Promise<void> {
    await expect(this.row(publicLabel).getByLabel(label, { exact: true })).toHaveValue(value);
  }

  async edit(publicLabel: string, edit: ParticipantEdit): Promise<void> {
    const row = this.row(publicLabel);
    if (edit.email !== undefined) await row.getByLabel("Email", { exact: true }).fill(edit.email);
    if (edit.name !== undefined) await row.getByLabel("Name", { exact: true }).fill(edit.name);
    if (edit.whatYouMake !== undefined) {
      await row.getByLabel("What you make", { exact: true }).fill(edit.whatYouMake);
    }
    if (edit.onYourHeart !== undefined) {
      await row.getByLabel("On your heart", { exact: true }).fill(edit.onYourHeart);
    }
  }

  async save(publicLabel: string): Promise<void> {
    await this.row(publicLabel).getByRole("button", { name: "Save participant", exact: true }).click();
  }

  async remove(publicLabel: string): Promise<void> {
    await this.row(publicLabel).getByRole("button", { name: "Remove participant", exact: true }).click();
  }

  async removeAndConfirm(publicLabel: string): Promise<void> {
    await this.remove(publicLabel);
    await this.dialog.confirm("Remove");
  }

  async removeAndCancel(publicLabel: string): Promise<void> {
    await this.remove(publicLabel);
    await this.dialog.cancel("Keep participant");
  }

  /** Reads the "<team>; <raffle status>" line the roster renders per row. */
  async expectStatus(publicLabel: string, team: string, raffle: "Eligible" | "Winner"): Promise<void> {
    await expect(this.row(publicLabel).getByText(`${team}; ${raffle}`, { exact: true })).toBeVisible();
  }

  teamSelect(publicLabel: string): Locator {
    return this.row(publicLabel).getByLabel("Team", { exact: true });
  }

  async moveToTeam(publicLabel: string, optionLabel: string): Promise<void> {
    await this.teamSelect(publicLabel).selectOption({ label: optionLabel });
  }

  async expectTeamSelection(publicLabel: string, optionLabel: string): Promise<void> {
    await expect(this.teamSelect(publicLabel).locator("option:checked")).toHaveText(optionLabel);
  }

  async expectError(message: string | RegExp): Promise<void> {
    await expect(this.page.getByRole("alert").filter({ hasText: message })).toBeVisible();
  }
}
