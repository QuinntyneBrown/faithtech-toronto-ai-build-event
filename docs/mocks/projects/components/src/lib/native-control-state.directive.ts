import {
  Directive,
  booleanAttribute,
  effect,
  inject,
  input,
} from "@angular/core";
import { CsSelectDirective } from "@quinntyne/cornerstone";

/** Proposed Cornerstone fix: keep native control state aligned with its inputs. */
@Directive({
  selector: "button[csButton],select[csSelect]",
  host: { "[disabled]": "disabled()", "[value]": "nativeValue()" },
})
export class NativeControlStateDirective {
  readonly disabled = input(false, { transform: booleanAttribute });
  readonly nativeValue = input("");
  private readonly select = inject(CsSelectDirective, { optional: true });
  constructor() {
    effect(() => this.select?.setDisabledState(this.disabled()));
  }
}
