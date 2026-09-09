import { ChangeDetectionStrategy, Component, input } from "@angular/core";
import { BadgeComponent } from "@quinntyne/cornerstone";

@Component({
  selector: "event-header",
  imports: [BadgeComponent],
  templateUrl: "./event-header.component.html",
  styleUrl: "./event-header.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventHeaderComponent {
  readonly title = input.required<string>();
  readonly eventTime = input.required<string>();
}
