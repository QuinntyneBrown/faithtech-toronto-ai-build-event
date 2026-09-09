import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal } from "@angular/core";
import { CardComponent, CsButtonDirective, RaffleResult as CornerstoneRaffleResult, RaffleStageComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_SERVICE, RAFFLE_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-raffle",
  imports: [CardComponent, CsButtonDirective, RaffleStageComponent],
  templateUrl: "./raffle.component.html",
  styleUrl: "./raffle.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RaffleComponent {
  readonly raffle = inject(RAFFLE_SERVICE);
  readonly event = inject(EVENT_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly now = signal(this.event.serverNow());
  readonly stageResult = computed<CornerstoneRaffleResult | null>(() => {
    const result = this.raffle.state()?.latestResult;
    return result ? {
      id: result.drawId,
      label: "Raffle draw",
      winnerName: result.winnerLabel,
      candidates: result.candidateLabels,
      start: Date.parse(result.startedAtUtc),
      reveal: Date.parse(result.revealAtUtc)
    } : null;
  });

  constructor() {
    this.event.load();
    this.administrator.load();
    effect(() => {
      if (this.event.state()?.version) this.raffle.load();
    });
    const timer = window.setInterval(() => this.now.set(this.event.serverNow()), 250);
    inject(DestroyRef).onDestroy(() => window.clearInterval(timer));
  }

  draw(): void {
    const version = this.event.state()?.version;
    if (version) {
      this.raffle.draw(version);
    }
  }
}
