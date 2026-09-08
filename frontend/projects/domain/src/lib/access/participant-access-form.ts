import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ENTRY_SERVICE, EntryFailure, EntryResult } from '@faithtech/api';

@Component({
  selector: 'ft-participant-access-form', imports: [FormsModule],
  templateUrl: './participant-access-form.html', styleUrl: './participant-access-form.css',
})
export class ParticipantAccessForm {
  private readonly service = inject(ENTRY_SERVICE);
  readonly eventId = input.required<string>();
  readonly returnTo = input<string>();
  readonly authenticated = output<EntryResult>();
  readonly email = signal('');
  readonly code = signal('');
  readonly busy = signal(false);
  readonly error = signal('');
  readonly fieldError = signal('');

  async submit() {
    if (this.busy()) return;
    this.busy.set(true); this.error.set(''); this.fieldError.set('');
    try {
      const result = await this.service.authenticate(this.eventId(), this.email().trim(), this.code(), this.returnTo());
      this.code.set('');
      this.authenticated.emit(result);
    } catch (error) {
      this.code.set('');
      if (error instanceof EntryFailure && error.status === 422) {
        this.fieldError.set(error.errors['email']?.join(' ') ?? '');
        this.error.set(this.fieldError() || "We couldn't match those details. Check your email and entry code.");
      } else if (error instanceof EntryFailure && error.status === 429) {
        this.error.set(`Too many attempts. Try again in ${error.retryAfterSeconds} seconds.`);
      } else {
        this.error.set("We couldn't match those details. Check your email and entry code, or speak to your host.");
      }
    } finally { this.busy.set(false); }
  }
}
