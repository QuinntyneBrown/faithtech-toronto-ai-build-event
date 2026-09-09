import { expect, type Locator, type Page } from "@playwright/test";
import { ConfirmDialog } from "./confirm-dialog";
import { ConnectionBanner } from "./connection-banner";

export interface ProjectDetails {
  title?: string;
  description?: string;
  repositoryUrl?: string;
  demoUrl?: string;
}

/**
 * The Projects screen: the RTR project cards and the inline administrator
 * controls that maintain them.
 */
export class ProjectsPage {
  readonly banner: ConnectionBanner;
  readonly dialog: ConfirmDialog;

  private readonly advance: Locator;
  private readonly addSubmit: Locator;

  constructor(private readonly page: Page) {
    this.banner = new ConnectionBanner(page);
    this.dialog = new ConfirmDialog(page);
    this.advance = page.getByRole("button", { name: "Form teams", exact: true });
    this.addSubmit = page.getByRole("button", { name: "Add project", exact: true });
  }

  async open(): Promise<void> {
    await this.page.goto("/projects");
  }

  async expectCurrent(): Promise<void> {
    await expect(this.page.getByRole("heading", { level: 1, name: "Projects" })).toBeVisible();
  }

  async expectLoading(): Promise<void> {
    await expect(this.page.getByText("Loading projects…", { exact: true })).toBeVisible();
  }

  async expectEmpty(): Promise<void> {
    await expect(
      this.page.getByRole("heading", { name: "No projects added yet", exact: true })
    ).toBeVisible();
  }

  /** One project's card, found by the title it renders. */
  card(title: string): Locator {
    return this.page
      .locator("cs-card")
      .filter({ has: this.page.getByRole("heading", { level: 2, name: title, exact: true }) });
  }

  private editForm(title: string): Locator {
    return this.page.locator(`form[aria-label="Edit ${title}"]`);
  }

  async expectProject(title: string, description: string): Promise<void> {
    await expect(this.card(title)).toBeVisible();
    await expect(this.card(title).getByText(description, { exact: true })).toBeVisible();
  }

  async expectProjectAbsent(title: string): Promise<void> {
    await expect(this.card(title)).toHaveCount(0);
  }

  async expectProjectCount(count: number): Promise<void> {
    await expect(this.page.locator('form[aria-label^="Edit "]')).toHaveCount(count);
  }

  async expectLink(title: string, name: "Repository" | "Demo", href: string): Promise<void> {
    await expect(this.card(title).getByRole("link", { name, exact: true })).toHaveAttribute("href", href);
  }

  async expectLinkAbsent(title: string, name: "Repository" | "Demo"): Promise<void> {
    await expect(this.card(title).getByRole("link", { name, exact: true })).toHaveCount(0);
  }

  /** A supplied link must not hand the opened page control of this one. */
  async expectLinkIsolated(title: string, name: "Repository" | "Demo"): Promise<void> {
    const link = this.card(title).getByRole("link", { name, exact: true });
    await expect(link).toHaveAttribute("target", "_blank");
    await expect(link).toHaveAttribute("rel", /noopener/);
  }

  // ----------------------------------------------------------- administering

  async expectAddAvailable(): Promise<void> {
    await expect(this.page.getByRole("heading", { level: 2, name: "Add project", exact: true })).toBeVisible();
  }

  async expectAddHidden(): Promise<void> {
    await expect(this.addSubmit).toHaveCount(0);
  }

  async add(details: Required<Pick<ProjectDetails, "title" | "description">> & ProjectDetails): Promise<void> {
    await this.page.locator("#new-project-title").fill(details.title);
    await this.page.locator("#new-project-description").fill(details.description);
    if (details.repositoryUrl !== undefined) {
      await this.page.locator("#new-project-repository").fill(details.repositoryUrl);
    }
    if (details.demoUrl !== undefined) {
      await this.page.locator("#new-project-demo").fill(details.demoUrl);
    }
    await this.addSubmit.click();
  }

  async expectNewProjectValues(details: Required<Pick<ProjectDetails, "title" | "description">>): Promise<void> {
    await expect(this.page.locator("#new-project-title")).toHaveValue(details.title);
    await expect(this.page.locator("#new-project-description")).toHaveValue(details.description);
  }

  async edit(title: string, details: ProjectDetails): Promise<void> {
    const form = this.editForm(title);
    if (details.title !== undefined) await form.getByLabel("Title", { exact: true }).fill(details.title);
    if (details.description !== undefined) {
      await form.getByLabel("Description", { exact: true }).fill(details.description);
    }
    if (details.repositoryUrl !== undefined) {
      await form.getByLabel("Repository URL", { exact: true }).fill(details.repositoryUrl);
    }
    if (details.demoUrl !== undefined) {
      await form.getByLabel("Demo URL", { exact: true }).fill(details.demoUrl);
    }
  }

  async expectEditValue(title: string, label: string, value: string): Promise<void> {
    await expect(this.editForm(title).getByLabel(label, { exact: true })).toHaveValue(value);
  }

  async save(title: string): Promise<void> {
    await this.editForm(title).getByRole("button", { name: "Save project", exact: true }).click();
  }

  async remove(title: string): Promise<void> {
    await this.editForm(title).getByRole("button", { name: "Remove project", exact: true }).click();
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

  async formTeams(): Promise<void> {
    await this.advance.click();
  }
}
