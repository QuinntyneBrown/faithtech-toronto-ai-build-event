import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SESSION_SERVICE, ServiceFailure } from '@faithtech/api';

@Component({
  selector: 'ft-administrator-access-form', imports: [FormsModule],
  templateUrl: './administrator-access-form.html', styleUrl: './administrator-access-form.css',
})
export class AdministratorAccessForm {
  private readonly service = inject(SESSION_SERVICE);
  readonly signedIn = output<void>();
  readonly username = signal('');
  readonly password = signal('');
  readonly busy = signal(false);
  readonly error = signal('');

  async submit() {
    if (this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    try {
      await this.service.signIn(this.username().trim(), this.password());
      this.password.set('');
      this.signedIn.emit();
    } catch (error) {
      this.error.set(error instanceof ServiceFailure && error.status === 401
        ? 'Check your username and password and try again.'
        : error instanceof ServiceFailure && error.status === 429
          ? `Too many attempts. Try again in ${error.retryAfterSeconds} seconds.`
          : 'We could not reach the event service. Please try again.');
      this.password.set('');
    } finally { this.busy.set(false); }
  }
}
