import { NativeControlStateDirective } from "@mock/components";
import {
  Component,
  ChangeDetectionStrategy,
  DestroyRef,
  computed,
  inject,
  signal,
} from "@angular/core";
import {
  BadgeComponent,
  CardComponent,
  CsButtonDirective,
} from "@quinntyne/cornerstone";
import { RaffleStageComponent } from "@mock/components";
import { EVENT_SERVICE } from "../../data/event-service.token";
@Component({
  selector: "mock-raffle-page",
  imports: [
    NativeControlStateDirective,
    BadgeComponent,
    CardComponent,
    CsButtonDirective,
    RaffleStageComponent,
  ],
  templateUrl: "./raffle-page.component.html",
  styleUrl: "./raffle-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RafflePageComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly now = signal(Date.now());
  readonly fallback = signal(false);
  readonly latest = computed(() => this.event.state().draws.at(-1) || null);
  readonly active = computed(
    () => !!this.latest() && this.now() < this.latest()!.reveal + 5000,
  );
  readonly eligible = computed(
    () =>
      this.event
        .state()
        .participants.filter(
          (p) => !this.event.state().draws.some((d) => d.winnerId === p.id),
        ).length,
  );
  readonly history = computed(() =>
    [...this.event.state().draws]
      .filter((d) => d.reveal <= this.now())
      .reverse(),
  );
  constructor() {
    const interval = setInterval(() => this.now.set(Date.now()), 100);
    inject(DestroyRef).onDestroy(() => clearInterval(interval));
  }
}
