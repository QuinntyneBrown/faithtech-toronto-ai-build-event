import { ChangeDetectionStrategy, Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CardComponent, CsButtonDirective, CsInputDirective, EmptyStateComponent, FieldComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_SERVICE, ROSTER_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-participant-roster",
  imports: [CardComponent, CsButtonDirective, CsInputDirective, EmptyStateComponent, FieldComponent, FormsModule],
  templateUrl: "./participant-roster.component.html",
  styleUrl: "./participant-roster.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ParticipantRosterComponent {
  readonly roster = inject(ROSTER_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly event = inject(EVENT_SERVICE);
  readonly email = signal("");

  refresh(): void { this.roster.load(); }

  add(): void {
    const version = this.event.state()?.version;
    if (version) this.roster.add(this.email(), version);
  }
}
