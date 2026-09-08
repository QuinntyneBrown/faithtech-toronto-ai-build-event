import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocalTimeInput, WindowInput } from '@faithtech/api';

@Component({ selector: 'ft-window-editor', imports: [FormsModule], templateUrl: './window-editor.html', styleUrl: './window-editor.css' })
export class WindowEditor {
  readonly label = input.required<string>();
  readonly value = input.required<WindowInput | null>();
  readonly start = input<LocalTimeInput | null>(null);
  readonly end = input<LocalTimeInput | null>(null);
  readonly changed = output<WindowInput | null>();
  enable(enabled: boolean) { this.changed.emit(enabled ? { start: this.start(), end: this.end() } : null); }
  time(field: 'start' | 'end', local: string) { const value = this.value(); if (value) this.changed.emit({ ...value, [field]: local ? { local, offsetMinutes: null } : null }); }
  offset(field: 'start' | 'end', offsetMinutes: number | null) {
    const value = this.value(), time = value?.[field]; if (value && time) this.changed.emit({ ...value, [field]: { ...time, offsetMinutes } });
  }
}
