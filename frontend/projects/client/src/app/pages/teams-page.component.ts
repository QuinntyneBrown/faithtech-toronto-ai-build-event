import { ChangeDetectionStrategy, Component } from "@angular/core";
import { TeamsComponent } from "@faithtech/domain";

@Component({
  selector: "event-teams-page",
  imports: [TeamsComponent],
  templateUrl: "./teams-page.component.html",
  styleUrl: "./teams-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeamsPageComponent {}
