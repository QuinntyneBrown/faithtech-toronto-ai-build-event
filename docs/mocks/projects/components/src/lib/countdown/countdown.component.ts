import {
  Component,
  ChangeDetectionStrategy,
  computed,
  input,
} from "@angular/core";
@Component({
  selector: "mock-countdown",
  templateUrl: "./countdown.component.html",
  styleUrl: "./countdown.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CountdownComponent {
  readonly target = input.required<number>();
  readonly now = input.required<number>();
  readonly units = computed(() => {
    const s = Math.max(0, Math.ceil((this.target() - this.now()) / 1000));
    return [
      { label: "Days", value: Math.floor(s / 86400) },
      { label: "Hours", value: Math.floor(s / 3600) % 24 },
      { label: "Minutes", value: Math.floor(s / 60) % 60 },
      { label: "Seconds", value: s % 60 },
    ].filter((unit, i) => i > 0 || unit.value > 0);
  });
  pad(value: number): string {
    return String(value).padStart(2, "0");
  }
}
