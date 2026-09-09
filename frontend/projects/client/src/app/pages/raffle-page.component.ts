import { ChangeDetectionStrategy, Component } from "@angular/core";
import { RaffleComponent } from "@faithtech/domain";

@Component({
  selector: "event-raffle-page",
  imports: [RaffleComponent],
  templateUrl: "./raffle-page.component.html",
  styleUrl: "./raffle-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RafflePageComponent {}
