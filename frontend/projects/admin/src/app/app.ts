import { Component, HostListener, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { SESSION_SERVICE, ServiceFailure } from '@faithtech/api';

@Component({ selector: 'app-root', imports: [RouterOutlet], templateUrl: './app.html', styleUrl: './app.css' })
export class App {
  private readonly session = inject(SESSION_SERVICE);
  private readonly router = inject(Router);
  private lastInteraction = -Infinity;

  @HostListener('document:pointerdown', ['$event'])
  @HostListener('document:keydown', ['$event'])
  recordActivity(event: Event) {
    if (!event.isTrusted || this.router.url === '/sign-in' || performance.now() - this.lastInteraction < 5000) return;
    this.lastInteraction = performance.now();
    void this.session.interact().catch(error => {
      if (error instanceof ServiceFailure && error.status === 401)
        void this.router.navigateByUrl('/sign-in', { replaceUrl: true });
    });
  }
}
