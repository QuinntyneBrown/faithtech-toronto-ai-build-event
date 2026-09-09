import { NativeControlStateDirective } from "@mock/components";
import {
  Component,
  ChangeDetectionStrategy,
  computed,
  inject,
  signal,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  AlertComponent,
  BadgeComponent,
  CardComponent,
  EmptyStateComponent,
  TableDirective,
  CsButtonDirective,
  FieldComponent,
  CsInputDirective,
  CsTextareaDirective,
} from "@quinntyne/cornerstone";
import { ReviewDialogComponent } from "@mock/components";
import { EVENT_SERVICE } from "../../data/event-service.token";
import { Participant } from "../../models/participant";
@Component({
  selector: "mock-participant-manager",
  imports: [
    NativeControlStateDirective,
    FormsModule,
    AlertComponent,
    BadgeComponent,
    CardComponent,
    EmptyStateComponent,
    TableDirective,
    CsButtonDirective,
    FieldComponent,
    CsInputDirective,
    CsTextareaDirective,
    ReviewDialogComponent,
  ],
  templateUrl: "./participant-manager.component.html",
  styleUrl: "./participant-manager.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParticipantManagerComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly draft = signal<Participant | null>(null);
  readonly deleting = signal<Participant | null>(null);
  readonly search = signal("");
  readonly visible = computed(() => {
    const query = this.search().trim().toLowerCase();
    return this.event
      .state()
      .participants.filter((p) =>
        [p.name, p.email, p.label].some((v) => v.toLowerCase().includes(query)),
      );
  });
  edit(p?: Participant): void {
    this.event.dismiss();
    this.draft.set(
      p
        ? { ...p }
        : {
            id: crypto.randomUUID(),
            label: "",
            email: "",
            name: "",
            makes: "",
            heart: "",
            teamId: null,
          },
    );
  }
  change(field: "email" | "name" | "makes" | "heart", value: string): void {
    this.draft.update((p) => p && { ...p, [field]: value });
  }
  team(id: string | null): string {
    return (
      this.event.state().teams.find((t) => t.id === id)?.name || "Unassigned"
    );
  }
  won(id: string): boolean {
    return this.event.state().draws.some((d) => d.winnerId === id);
  }
  async save(): Promise<void> {
    const participant = this.draft();
    if (
      participant &&
      (await this.event.dispatch({ type: "participantSave", participant }))
    )
      this.draft.set(null);
  }
  async remove(): Promise<void> {
    const p = this.deleting();
    if (
      p &&
      (await this.event.dispatch({ type: "participantDelete", id: p.id }))
    )
      this.deleting.set(null);
  }
}
