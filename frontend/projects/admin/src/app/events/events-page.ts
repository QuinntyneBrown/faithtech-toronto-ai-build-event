import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { EventDetail, EventSummary } from '@faithtech/api';
import { AdministratorSessionPanel, EventListPanel } from '@faithtech/domain';
import { CreateEventDialog } from './create-event-dialog';
import { CopyEventDialog } from './copy-event-dialog';

@Component({ selector: 'ft-events-page', imports: [AdministratorSessionPanel, EventListPanel, CreateEventDialog, CopyEventDialog, RouterLink],
  templateUrl: './events-page.html', styleUrl: './events-page.css' })
export class EventsPage {
  private readonly router = inject(Router);
  private readonly list = viewChild.required(EventListPanel);
  private readonly createButton = viewChild.required<ElementRef<HTMLButtonElement>>('createButton');
  readonly creating = signal(false);
  readonly copySource = signal<EventSummary | null>(null);
  private lastFocused: HTMLElement | null = null;
  signedOut() { void this.router.navigateByUrl('/sign-in', { replaceUrl: true }); }
  close() { this.creating.set(false); this.createButton().nativeElement.focus(); }
  saved() { this.close(); void this.list().refresh(); }
  openCopy(event: EventSummary) { this.lastFocused = document.activeElement as HTMLElement | null; this.copySource.set(event); }
  closeCopy() { this.copySource.set(null); this.lastFocused?.focus(); }
  copied(result: EventDetail) { this.copySource.set(null); void this.router.navigate(['/events', result.id]); }
}
