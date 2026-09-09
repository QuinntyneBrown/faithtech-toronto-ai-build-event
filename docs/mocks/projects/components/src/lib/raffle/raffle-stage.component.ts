import { NativeControlStateDirective } from "../native-control-state.directive";
import {
  Component,
  ChangeDetectionStrategy,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from "@angular/core";
import { BadgeComponent, CsButtonDirective } from "@quinntyne/cornerstone";
import { RaffleResult } from "./raffle-result";
import { startConfetti } from "./start-confetti";
@Component({
  selector: "mock-raffle-stage",
  imports: [NativeControlStateDirective, BadgeComponent, CsButtonDirective],
  templateUrl: "./raffle-stage.component.html",
  styleUrl: "./raffle-stage.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RaffleStageComponent {
  readonly result = input<RaffleResult | null>(null);
  readonly forceFallback = input(false);
  readonly now = signal(Date.now());
  readonly reduced = signal(false);
  readonly stopped = signal(false);
  readonly fallback = signal(false);
  private readonly canvas = viewChild<ElementRef<HTMLCanvasElement>>("canvas");
  private readonly arrivedAfterReveal = signal(false);
  readonly drawing = computed(
    () => !!this.result() && this.now() < this.result()!.reveal,
  );
  readonly celebrating = computed(
    () =>
      !!this.result() &&
      !this.drawing() &&
      this.now() < this.result()!.reveal + 5000 &&
      !this.reduced() &&
      !this.stopped() &&
      !this.arrivedAfterReveal(),
  );
  readonly cycling = computed(() => {
    const draw = this.result();
    if (!draw || !this.drawing()) return "";
    if (this.reduced() || this.stopped()) return "Drawing…";
    const elapsed = Math.max(0, this.now() - draw.start) / 1000;
    const index = Math.floor(26 * (1 - Math.exp(-elapsed * 0.7)));
    return draw.candidates[index % draw.candidates.length] || "Drawing…";
  });
  readonly pieces = Array.from({ length: 60 }, (_, i) => ({
    id: i,
    x: (i * 37) % 100,
    delay: -(i % 7) / 3,
    drift: (i % 2 ? 1 : -1) * ((i % 8) + 2),
    color: ["var(--cs-lime)", "var(--cs-success)", "var(--cs-ink)"][i % 3],
  }));
  constructor() {
    const destroy = inject(DestroyRef),
      query = matchMedia("(prefers-reduced-motion: reduce)");
    this.reduced.set(query.matches);
    const onMotion = () => this.reduced.set(query.matches);
    query.addEventListener("change", onMotion);
    const timer = setInterval(() => this.now.set(Date.now()), 50);
    let previous = "";
    effect(() => {
      const draw = this.result();
      if (draw?.id !== previous) {
        previous = draw?.id || "";
        this.stopped.set(false);
        this.arrivedAfterReveal.set(!!draw && Date.now() >= draw.reveal);
      }
    });
    effect((cleanup) => {
      if (!this.celebrating()) return;
      const canvas = this.canvas()?.nativeElement;
      if (!canvas) return;
      if (this.forceFallback()) {
        this.fallback.set(true);
        return;
      }
      let cancelled = false,
        stop = () => {};
      this.fallback.set(false);
      void startConfetti(canvas, () => this.fallback.set(true))
        .then((dispose) => {
          if (cancelled) dispose();
          else stop = dispose;
        })
        .catch(() => {
          if (!cancelled) this.fallback.set(true);
        });
      cleanup(() => {
        cancelled = true;
        stop();
      });
    });
    destroy.onDestroy(() => {
      clearInterval(timer);
      query.removeEventListener("change", onMotion);
    });
  }
}
