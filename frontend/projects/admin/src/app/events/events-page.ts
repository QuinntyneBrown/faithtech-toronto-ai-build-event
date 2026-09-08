import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AdministratorSessionPanel, EventListPanel } from '@faithtech/domain';
import { CreateEventDialog } from './create-event-dialog';

@Component({ selector: 'ft-events-page', imports: [AdministratorSessionPanel, EventListPanel, CreateEventDialog, RouterLink],
  templateUrl: './events-page.html', styleUrl: './events-page.css' })
export class EventsPage {
  private readonly router = inject(Router);
  private readonly list = viewChild.required(EventListPanel);
  private readonly createButton = viewChild.required<ElementRef<HTMLButtonElement>>('createButton');
  readonly creating = signal(false);
  signedOut() { void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
  close() { this.creating.set(false); this.createButton().nativeElement.focus(); }
  saved() { this.close(); void this.list().refresh(); }
}
