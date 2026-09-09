import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { CardComponent, EmptyStateComponent } from "@quinntyne/cornerstone";
import { EVENT_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-teams",
  imports: [CardComponent, EmptyStateComponent],
  templateUrl: "./teams.component.html",
  styleUrl: "./teams.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeamsComponent {
  readonly event = inject(EVENT_SERVICE);

  constructor() {
    this.event.load();
  }
}
