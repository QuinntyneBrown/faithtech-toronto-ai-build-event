import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { CardComponent, EmptyStateComponent } from "@quinntyne/cornerstone";
import { EVENT_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-projects",
  imports: [CardComponent, EmptyStateComponent],
  templateUrl: "./projects.component.html",
  styleUrl: "./projects.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsComponent {
  readonly event = inject(EVENT_SERVICE);

  constructor() {
    this.event.load();
  }
}
