import { expect, type Locator, type Page } from "@playwright/test";
import { AdministratorLoginPanel } from "./administrator-login.panel";
import { ConnectionBanner } from "./connection-banner";
import { ProfileFormPanel } from "./profile-form.panel";
import { RosterPanel } from "./roster.panel";

/**
 * The Countdown screen: welcome copy, the clock, public email entry, and the
 * inline administrator surfaces that live on it.
 */
export class CountdownPage {
  readonly banner: ConnectionBanner;
  readonly login: AdministratorLoginPanel;
  readonly roster: RosterPanel;
  readonly profile: ProfileFormPanel;

  private readonly emailField: Locator;
  private readonly submit: Locator;
  private readonly leave: Locator;
  private readonly advance: Locator;
  private readonly timer: Locator;

  constructor(private readonly page: Page) {
    this.banner = new ConnectionBanner(page);
    this.login = new AdministratorLoginPanel(page);
    this.roster = new RosterPanel(page);
    this.profile = new ProfileFormPanel(page);

    this.emailField = page.locator("#entry-email");
    this.submit = page.getByRole("button", { name: "Count me in", exact: true });
    this.leave = page.getByRole("button", { name: "Leave this browser", exact: true });
    this.advance = page.getByRole("button", { name: "Open projects", exact: true });
    this.timer = page.getByRole("timer", { name: "Time until welcome" });
  }

  async open(): Promise<void> {
    await this.page.goto("/countdown");
  }

  // ------------------------------------------------------------ information

  async expectTitle(title: string): Promise<void> {
    await expect(this.page.getByRole("heading", { level: 1, name: title })).toBeVisible();
  }

  async expectWelcome(welcome: string): Promise<void> {
    await expect(this.page.getByRole("heading", { level: 2, name: welcome })).toBeVisible();
  }

  async expectInformation(...lines: string[]): Promise<void> {
    for (const line of lines) {
      await expect(this.page.getByText(line, { exact: true })).toBeVisible();
    }
  }

  async expectLoading(): Promise<void> {
    await expect(this.page.getByText("Preparing the evening…", { exact: true })).toBeVisible();
  }

  async expectError(message: string): Promise<void> {
    await expect(this.page.getByRole("alert").filter({ hasText: message })).toBeVisible();
  }

  // ------------------------------------------------------------------ clock

  async expectCountdownRunning(): Promise<void> {
    await expect(this.timer).toBeVisible();
  }

  private unitValue(label: string): Locator {
    return this.timer.locator(".unit").filter({ hasText: label }).locator(".value");
  }

  async expectUnitPresent(label: string): Promise<void> {
    await expect(this.unitValue(label)).toBeVisible();
  }

  /** Unit values are rendered zero padded, e.g. "07". */
  async expectUnitValue(label: string, value: string): Promise<void> {
    await expect(this.unitValue(label)).toHaveText(value);
  }

  /** Reads the seconds unit so a test can prove the clock is decreasing. */
  async readSeconds(): Promise<number> {
    return Number(await this.unitValue("Seconds").innerText());
  }

  /**
   * The whole remaining time in seconds, rebuilt from the rendered units.
   * Days are only rendered once there is at least one, so they are optional.
   */
  async readRemainingSeconds(): Promise<number> {
    const read = async (label: string, factor: number) => {
      const unit = this.unitValue(label);
      return (await unit.count()) ? Number(await unit.innerText()) * factor : 0;
    };
    return (
      (await read("Days", 86_400)) +
      (await read("Hours", 3_600)) +
      (await read("Minutes", 60)) +
      (await read("Seconds", 1))
    );
  }

  async expectUnitAbsent(label: string): Promise<void> {
    await expect(this.timer.locator(".unit").filter({ hasText: label })).toHaveCount(0);
  }

  async expectCountdownComplete(): Promise<void> {
    await expect(this.page.getByText("The countdown is complete.", { exact: true })).toBeVisible();
  }

  async expectClockUnavailable(): Promise<void> {
    await expect(this.timer).toHaveCount(0);
    await expect(
      this.page.getByRole("status").filter({ hasText: "Current countdown time is unavailable" })
    ).toBeVisible();
  }

  async expectScheduledTime(text: string | RegExp): Promise<void> {
    await expect(this.page.getByText(text)).toBeVisible();
  }

  // ------------------------------------------------------------------ entry

  async enter(email: string): Promise<void> {
    await this.emailField.fill(email);
    await this.submit.click();
  }

  async expectEntryEnabled(): Promise<void> {
    await expect(this.submit).toBeEnabled();
  }

  async expectEntryDisabled(): Promise<void> {
    await expect(this.submit).toBeDisabled();
  }

  async expectEntryPrompt(): Promise<void> {
    await expect(this.page.getByRole("heading", { name: "Start with hello.", exact: true })).toBeVisible();
  }

  async expectPrivacyNotice(): Promise<void> {
    await expect(
      this.page.getByText("Enter your email to join the raffle. Your email stays with the hosts.", {
        exact: true
      })
    ).toBeVisible();
  }

  async expectEntered(publicLabel: string): Promise<void> {
    await expect(
      this.page.getByRole("heading", { name: "You have been entered into the raffle.", exact: true })
    ).toBeVisible();
    await expect(this.page.getByText(publicLabel, { exact: true })).toBeVisible();
  }

  async expectNotEntered(): Promise<void> {
    await expect(
      this.page.getByRole("heading", { name: "You have been entered into the raffle.", exact: true })
    ).toHaveCount(0);
  }

  async expectEntryError(message: string | RegExp): Promise<void> {
    await expect(this.page.getByRole("alert").filter({ hasText: message })).toBeVisible();
  }

  async expectEmailValue(email: string): Promise<void> {
    await expect(this.emailField).toHaveValue(email);
  }

  async leaveThisBrowser(): Promise<void> {
    await this.leave.click();
  }

  // -------------------------------------------------------------- advancing

  async expectAdvanceVisible(): Promise<void> {
    await expect(this.advance).toBeVisible();
  }

  async expectAdvanceHidden(): Promise<void> {
    await expect(this.advance).toHaveCount(0);
  }

  async expectAdvanceDisabled(): Promise<void> {
    await expect(this.advance).toBeDisabled();
  }

  async openProjects(): Promise<void> {
    await this.advance.click();
  }

  async expectLeaveDisabled(): Promise<void> {
    await expect(this.leave).toBeDisabled();
  }
}
