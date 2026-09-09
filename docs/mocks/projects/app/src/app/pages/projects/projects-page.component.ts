import { NativeControlStateDirective } from "@mock/components";
import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  CardComponent,
  BadgeComponent,
  AlertComponent,
  EmptyStateComponent,
  CsButtonDirective,
  FieldComponent,
  CsInputDirective,
  CsTextareaDirective,
} from "@quinntyne/cornerstone";
import { ReviewDialogComponent } from "@mock/components";
import { EVENT_SERVICE } from "../../data/event-service.token";
import { Project } from "../../models/project";
@Component({
  selector: "mock-projects-page",
  imports: [
    NativeControlStateDirective,
    FormsModule,
    CardComponent,
    BadgeComponent,
    AlertComponent,
    EmptyStateComponent,
    CsButtonDirective,
    FieldComponent,
    CsInputDirective,
    CsTextareaDirective,
    ReviewDialogComponent,
  ],
  templateUrl: "./projects-page.component.html",
  styleUrl: "./projects-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsPageComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly draft = signal<Project | null>(null);
  readonly deleting = signal<Project | null>(null);
  edit(p?: Project): void {
    this.event.dismiss();
    this.draft.set(
      p
        ? { ...p }
        : {
            id: crypto.randomUUID(),
            title: "",
            description: "",
            repository: "",
            demo: "",
          },
    );
  }
  change(
    field: "title" | "description" | "repository" | "demo",
    value: string,
  ): void {
    this.draft.update((p) => p && { ...p, [field]: value });
  }
  assigned(id: string): string {
    return (
      this.event
        .state()
        .teams.filter((t) => t.projectId === id)
        .map((t) => t.name)
        .join(", ") || "No teams assigned"
    );
  }
  async save(): Promise<void> {
    const project = this.draft();
    if (
      project &&
      (await this.event.dispatch({ type: "projectSave", project }))
    )
      this.draft.set(null);
  }
  async remove(): Promise<void> {
    const p = this.deleting();
    if (p && (await this.event.dispatch({ type: "projectDelete", id: p.id })))
      this.deleting.set(null);
  }
}
