import { ChangeDetectionStrategy, Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CardComponent, CsButtonDirective, CsInputDirective, CsTextareaDirective, DialogService, EmptyStateComponent, FieldComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_FLOW_SERVICE, EVENT_SERVICE, ProjectCard, ProjectInput, PROJECT_SERVICE } from "@faithtech/api";
import { ConfirmationDialogComponent } from "@faithtech/components";

@Component({
  selector: "event-projects",
  imports: [CardComponent, CsButtonDirective, CsInputDirective, CsTextareaDirective, EmptyStateComponent, FieldComponent, FormsModule],
  templateUrl: "./projects.component.html",
  styleUrl: "./projects.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly projects = inject(PROJECT_SERVICE);
  readonly flow = inject(EVENT_FLOW_SERVICE);
  readonly newProject = signal<ProjectInput>({ title: "", description: "", repositoryUrl: null, demoUrl: null });
  readonly drafts = signal<Record<string, ProjectInput>>({});
  private readonly dialogs = inject(DialogService);

  constructor() {
    this.event.load();
  }

  draftFor(project: ProjectCard): ProjectInput { return this.drafts()[project.id] ?? project; }
  updateDraft(project: ProjectCard, field: keyof ProjectInput, value: string): void { this.drafts.update(drafts => ({ ...drafts, [project.id]: { ...this.draftFor(project), [field]: value || null } })); }
  updateNew(field: keyof ProjectInput, value: string): void { this.newProject.update(project => ({ ...project, [field]: value || null })); }
  add(): void { const version = this.event.state()?.version; if (version) this.projects.add(this.newProject(), version); }
  save(project: ProjectCard): void { const version = this.event.state()?.version; if (version) this.projects.update(project.id, this.draftFor(project), version); }
  remove(project: ProjectCard): void {
    const version = this.event.state()?.version;
    if (!version) return;
    const assignedTeams = this.event.state()?.teams.filter(team => team.projectId === project.id).map(team => team.label) ?? [];
    const assignmentNotice = assignedTeams.length
      ? ` Assigned teams that will be cleared: ${assignedTeams.join(", ")}.`
      : " No teams are currently assigned.";
    const dialog = this.dialogs.open(ConfirmationDialogComponent, { data: { title: "Remove project?", message: `${project.title} will no longer be available for team assignment.${assignmentNotice}`, confirmLabel: "Remove", cancelLabel: "Keep project", tone: "danger" } });
    dialog.closed.subscribe(result => { if (result === "confirm") this.projects.remove(project.id, version); });
  }
  advance(): void { const state = this.event.state(); if (state) this.flow.advance("projects", "teams", state.version); }
}
