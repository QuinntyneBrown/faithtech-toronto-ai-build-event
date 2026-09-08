import { AfterViewInit, Component, ElementRef, inject, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EVENT_SERVICE, EventDetail, EventFailure, LocalTimeInput } from '@faithtech/api';

@Component({ selector: 'ft-copy-event-dialog', imports: [FormsModule], templateUrl: './copy-event-dialog.html', styleUrl: './copy-event-dialog.css' })
export class CopyEventDialog implements AfterViewInit {
  private readonly service = inject(EVENT_SERVICE);
  readonly sourceId = input.required<string>();
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private operationId = crypto.randomUUID();
  readonly closed = output<void>();
  readonly copied = output<EventDetail>();
  readonly local = signal('');
  readonly offsetMinutes = signal<number | null>(null);
  readonly error = signal('');
  readonly fieldError = signal('');
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  ngAfterViewInit() { this.dialog().nativeElement.showModal(); }
  close() {
    if (this.busy()) return;
    this.dialog().nativeElement.close(); this.closed.emit();
  }
  async save() {
    if (this.busy() || !this.local()) return;
    this.busy.set(true); this.error.set(''); this.fieldError.set('');
    const start: LocalTimeInput = { local: this.local(), offsetMinutes: this.offsetMinutes() };
    try {
      const result = await this.service.copy(this.sourceId(), start, this.operationId);
      this.dialog().nativeElement.close(); this.copied.emit(result);
    } catch (error) {
      if (error instanceof EventFailure && error.status === 422) {
        this.fieldError.set(error.errors['start']?.join(' ') ?? '');
        this.error.set(this.fieldError() || 'Check the new start date and time.');
        this.operationId = crypto.randomUUID();
      } else if (error instanceof EventFailure && error.status === 404) {
        this.error.set('This event is no longer available to copy.');
        this.operationId = crypto.randomUUID();
      } else {
        this.uncertain.set(true);
        this.error.set('The copy could not be confirmed. Retry to check its outcome.');
      }
    } finally { this.busy.set(false); }
  }
}
