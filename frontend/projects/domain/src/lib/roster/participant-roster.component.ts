import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { CardComponent, CsButtonDirective, EmptyStateComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, ROSTER_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-participant-roster",
  imports: [CardComponent, CsButtonDirective, EmptyStateComponent],
  templateUrl: "./participant-roster.component.html",
  styleUrl: "./participant-roster.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ParticipantRosterComponent {
  readonly roster = inject(ROSTER_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);

  refresh(): void { this.roster.load(); }
}
