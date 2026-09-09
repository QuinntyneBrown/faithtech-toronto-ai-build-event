import { ChangeDetectionStrategy, Component } from "@angular/core";
import { ProjectsComponent } from "@faithtech/domain";

@Component({
  selector: "event-projects-page",
  imports: [ProjectsComponent],
  templateUrl: "./projects-page.component.html",
  styleUrl: "./projects-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectsPageComponent {}
