import { ChangeDetectionStrategy, Component } from "@angular/core";
import { CountdownComponent } from "@faithtech/domain";

@Component({
  selector: "event-countdown-page",
  imports: [CountdownComponent],
  templateUrl: "./countdown-page.component.html",
  styleUrl: "./countdown-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CountdownPageComponent {}
