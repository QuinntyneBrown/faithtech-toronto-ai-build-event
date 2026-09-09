import { Directive, booleanAttribute, effect, inject, input } from '@angular/core';
import { CsSelectDirective } from '@quinntyne/cornerstone';

/** Proposed Cornerstone fix: mirror the disabled input to the native control. */
@Directive({ selector: 'button[csButton],select[csSelect]', host: { '[disabled]': 'disabled()' } })
export class NativeControlStateDirective {
  readonly disabled = input(false, { transform: booleanAttribute });
  private readonly select = inject(CsSelectDirective, { optional: true });
  constructor() { effect(() => this.select?.setDisabledState(this.disabled())); }
}
