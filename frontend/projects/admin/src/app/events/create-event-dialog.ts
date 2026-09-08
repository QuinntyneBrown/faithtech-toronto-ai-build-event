import { AfterViewInit, Component, ElementRef, inject, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EVENT_SERVICE, ServiceFailure } from '@faithtech/api';

@Component({ selector: 'ft-create-event-dialog', imports: [FormsModule], templateUrl: './create-event-dialog.html', styleUrl: './create-event-dialog.css' })
export class CreateEventDialog implements AfterViewInit {
  private readonly service = inject(EVENT_SERVICE);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  private operationId = crypto.randomUUID();
  readonly closed = output<void>();
  readonly saved = output<void>();
  readonly title = signal('');
  readonly error = signal('');
  readonly busy = signal(false);
  readonly uncertain = signal(false);
  readonly discard = signal(false);
  ngAfterViewInit() { this.dialog().nativeElement.showModal(); }
  requestClose() {
    if (this.busy() || this.uncertain()) return;
    if (this.title().trim()) this.discard.set(true);
    else this.closed.emit();
  }
  async save() {
    if (this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    try { await this.service.createDraft(this.title(), this.operationId); this.saved.emit(); }
    catch (error) {
      if (error instanceof ServiceFailure && error.status === 422) {
        this.error.set('Event title must contain at most 200 characters.');
        this.operationId = crypto.randomUUID();
      } else {
        this.uncertain.set(true);
        this.error.set('The save could not be confirmed. Retry to check this same draft.');
      }
    } finally { this.busy.set(false); }
  }
}
