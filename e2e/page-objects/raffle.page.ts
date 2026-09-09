import { expect, type Locator, type Page } from "@playwright/test";
import { ConnectionBanner } from "./connection-banner";

/**
 * The Raffle screen.
 *
 * The presentation lives inside Cornerstone's raffle stage, which moves through
 * three states on a timeline driven by the clock the application passes in:
 * waiting, drawing, and revealed. Knowing which text belongs to which state is
 * this object's job.
 */
export class RafflePage {
  readonly banner: ConnectionBanner;

  private readonly stage: Locator;
  private readonly drawControl: Locator;
  private readonly stopControl: Locator;

  constructor(private readonly page: Page) {
    this.banner = new ConnectionBanner(page);
    this.stage = page.getByRole("region", { name: "Raffle draw" });
    this.drawControl = page.getByRole("button", { name: "DRAW NAME", exact: true });
    this.stopControl = page.getByRole("button", { name: "Stop effects", exact: true });
  }

  async open(): Promise<void> {
    await this.page.goto("/raffle");
  }

  async expectCurrent(): Promise<void> {
    await expect(this.page.getByRole("heading", { level: 1, name: "Raffle" })).toBeVisible();
  }

  async expectEligibleCount(text: string): Promise<void> {
    await expect(this.page.getByText(text, { exact: true })).toBeVisible();
  }

  // ------------------------------------------------------------ stage states

  async expectWaiting(): Promise<void> {
    await expect(
      this.stage.getByRole("heading", { name: "Ready for the draw", exact: true })
    ).toBeVisible();
  }

  async expectDrawing(): Promise<void> {
    await expect(this.stage.getByText("The draw is underway", { exact: true })).toBeVisible();
  }

  /** The announcement is a live region, so it must not be the cycling name. */
  async expectDrawingAnnouncement(): Promise<void> {
    await expect(
      this.stage.getByRole("status").filter({ hasText: "Drawing a name" })
    ).toBeVisible();
  }

  async expectCyclingName(): Promise<void> {
    await expect(this.stage.locator("p.cycling")).toBeVisible();
  }

  async readCyclingName(): Promise<string> {
    return (await this.stage.locator("p.cycling").innerText()).trim();
  }

  async expectStaticDrawingLabel(): Promise<void> {
    await expect(this.stage.locator("p.cycling")).toHaveText("Drawing…");
  }

  async expectWinner(label: string): Promise<void> {
    await expect(this.stage.locator("h2.winner")).toHaveText(label);
  }

  /** The winner must be readable text, not only an animation. */
  async expectWinnerAnnounced(label: string): Promise<void> {
    await expect(this.stage.getByRole("status").filter({ hasText: label })).toBeVisible();
  }

  async expectCelebrationCopy(): Promise<void> {
    await expect(this.stage.getByText("Congratulations", { exact: true })).toBeVisible();
  }

  async expectParticlesRunning(): Promise<void> {
    await expect(this.stopControl).toBeVisible();
  }

  async expectParticlesStopped(): Promise<void> {
    await expect(this.stopControl).toHaveCount(0);
  }

  async stopEffects(): Promise<void> {
    await this.stopControl.click();
  }

  // ------------------------------------------------------------------ drawing

  async draw(): Promise<void> {
    await this.drawControl.click();
  }

  async expectDrawVisible(): Promise<void> {
    await expect(this.drawControl).toBeVisible();
  }

  async expectDrawHidden(): Promise<void> {
    await expect(this.drawControl).toHaveCount(0);
  }

  async expectDrawEnabled(): Promise<void> {
    await expect(this.drawControl).toBeEnabled();
  }

  async expectDrawDisabled(): Promise<void> {
    await expect(this.drawControl).toBeDisabled();
  }

  async expectPreviousWinners(labels: string[]): Promise<void> {
    await expect(this.page.locator("ol li")).toHaveText(labels);
  }

  async expectNoPreviousWinners(): Promise<void> {
    await expect(this.page.getByRole("heading", { name: "Previous winners", exact: true })).toHaveCount(0);
  }

  async expectError(message: string | RegExp): Promise<void> {
    await expect(this.page.getByRole("alert").filter({ hasText: message })).toBeVisible();
  }
}
