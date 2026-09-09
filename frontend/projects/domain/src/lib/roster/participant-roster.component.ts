import { ChangeDetectionStrategy, Component, effect, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CardComponent, ConfirmDialogComponent, CsButtonDirective, CsInputDirective, CsSelectDirective, CsTextareaDirective, DialogService, EmptyStateComponent, FieldComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, AdministratorParticipant, AdministratorParticipantInput, EVENT_SERVICE, ROSTER_SERVICE, TEAM_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-participant-roster",
  imports: [CardComponent, CsButtonDirective, CsInputDirective, CsSelectDirective, CsTextareaDirective, EmptyStateComponent, FieldComponent, FormsModule],
  templateUrl: "./participant-roster.component.html",
  styleUrl: "./participant-roster.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ParticipantRosterComponent {
  readonly roster = inject(ROSTER_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly event = inject(EVENT_SERVICE);
  readonly teams = inject(TEAM_SERVICE);
  readonly email = signal("");
  readonly drafts = signal<Record<string, AdministratorParticipantInput>>({});
  private readonly dialogs = inject(DialogService);

  constructor() {
    effect(() => {
      if (this.administrator.active() && this.event.state()?.version) this.roster.load();
    });
  }

  refresh(): void { this.roster.load(); }

  add(): void {
    const version = this.event.state()?.version;
    if (version) this.roster.add(this.email(), version);
  }

  draftFor(participant: AdministratorParticipant): AdministratorParticipantInput {
    return this.drafts()[participant.id] ?? {
      email: participant.email,
      name: participant.name,
      whatYouMake: participant.whatYouMake,
      onYourHeart: participant.onYourHeart
    };
  }

  updateDraft(participant: AdministratorParticipant, field: keyof AdministratorParticipantInput, value: string): void {
    this.drafts.update(drafts => ({ ...drafts, [participant.id]: { ...this.draftFor(participant), [field]: value || null } }));
  }

  save(participant: AdministratorParticipant): void {
    const version = this.event.state()?.version;
    if (version) this.roster.update(participant.id, this.draftFor(participant), version);
  }

  remove(participant: AdministratorParticipant): void {
    const version = this.event.state()?.version;
    if (!version) return;
    const dialog = this.dialogs.open(ConfirmDialogComponent, {
      data: { title: "Remove participant?", message: `${participant.publicLabel} will lose their entry, profile, and team membership. This cannot be undone.`, confirmLabel: "Remove", cancelLabel: "Keep participant", tone: "danger" }
    });
    dialog.closed.subscribe(result => {
      if (result === "confirm") this.roster.remove(participant.id, version);
    });
  }

  move(participant: AdministratorParticipant, selection: string): void {
    const version = this.event.state()?.version;
    if (!version) return;
    if (selection === "unassigned" || selection === "new") this.teams.moveMember(participant.id, selection, null, version);
    else this.teams.moveMember(participant.id, "existing", selection, version);
  }

  teamSelection(participant: AdministratorParticipant): string {
    return this.event.state()?.teams.find(team => team.label === participant.teamLabel)?.id ?? "unassigned";
  }
}
