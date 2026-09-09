import { expect, type Locator, type Page } from "@playwright/test";
import { ConnectionBanner } from "./connection-banner";

/**
 * The Team selection screen: the formed teams, their members, and the inline
 * administrator control that assigns a project to each team.
 */
export class TeamsPage {
  readonly banner: ConnectionBanner;

  private readonly advance: Locator;

  constructor(private readonly page: Page) {
    this.banner = new ConnectionBanner(page);
    this.advance = page.getByRole("button", { name: "Open raffle", exact: true });
  }

  async open(): Promise<void> {
    await this.page.goto("/teams");
  }

  async expectCurrent(): Promise<void> {
    await expect(this.page.getByRole("heading", { level: 1, name: "Team selection" })).toBeVisible();
  }

  async expectLoading(): Promise<void> {
    await expect(this.page.getByText("Forming teams…", { exact: true })).toBeVisible();
  }

  async expectEmpty(): Promise<void> {
    await expect(
      this.page.getByRole("heading", { name: "No teams have formed yet", exact: true })
    ).toBeVisible();
  }

  card(label: string): Locator {
    return this.page
      .locator("cs-card")
      .filter({ has: this.page.getByRole("heading", { level: 2, name: label, exact: true }) });
  }

  async expectTeamLabels(labels: string[]): Promise<void> {
    await expect(this.page.locator("cs-card h2")).toHaveText(labels);
  }

  async expectMembers(label: string, members: string[]): Promise<void> {
    await expect(this.card(label).locator("li")).toHaveText(members);
  }

  async expectTeamSizes(sizes: number[]): Promise<void> {
    await expect(async () => {
      const cards = await this.page.locator("cs-card").filter({ has: this.page.locator("h2") }).all();
      const counts: number[] = [];
      for (const card of cards) counts.push(await card.locator("li").count());
      expect(counts).toEqual(sizes);
    }).toPass();
  }

  async expectAssignedProject(label: string, title: string): Promise<void> {
    await expect(this.card(label).getByText(`Project: ${title}`, { exact: true })).toBeVisible();
  }

  async expectNoAssignedProject(label: string): Promise<void> {
    await expect(this.card(label).getByText(/^Project: /)).toHaveCount(0);
  }

  private projectSelect(label: string): Locator {
    return this.card(label).getByLabel("Assigned project", { exact: true });
  }

  async assignProject(label: string, title: string): Promise<void> {
    await this.projectSelect(label).selectOption({ label: title });
  }

  async clearProject(label: string): Promise<void> {
    await this.projectSelect(label).selectOption({ label: "No project assigned" });
  }

  async expectProjectSelection(label: string, optionLabel: string): Promise<void> {
    await expect(this.projectSelect(label).locator("option:checked")).toHaveText(optionLabel);
  }

  async expectAssignmentAvailable(label: string): Promise<void> {
    await expect(this.projectSelect(label)).toBeVisible();
  }

  async expectAssignmentHidden(label: string): Promise<void> {
    await expect(this.projectSelect(label)).toHaveCount(0);
  }

  async expectError(message: string | RegExp): Promise<void> {
    await expect(this.page.getByRole("alert").filter({ hasText: message })).toBeVisible();
  }

  // -------------------------------------------------------------- advancing

  async expectAdvanceVisible(): Promise<void> {
    await expect(this.advance).toBeVisible();
  }

  async expectAdvanceHidden(): Promise<void> {
    await expect(this.advance).toHaveCount(0);
  }

  async openRaffle(): Promise<void> {
    await this.advance.click();
  }
}
