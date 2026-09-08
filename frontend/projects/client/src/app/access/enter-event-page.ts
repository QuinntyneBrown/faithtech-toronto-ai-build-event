import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ENTRY_SERVICE, EntryHeader, EntryResult } from '@faithtech/api';
import { ParticipantAccessForm } from '@faithtech/domain';

@Component({ selector: 'ft-enter-event-page', imports: [ParticipantAccessForm], templateUrl: './enter-event-page.html', styleUrl: './enter-event-page.css' })
export class EnterEventPage implements OnInit {
  private readonly service = inject(ENTRY_SERVICE);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly eventId = this.route.snapshot.paramMap.get('eventId')!;
  readonly returnTo = this.route.snapshot.queryParamMap.get('returnTo') ?? undefined;
  /** undefined = still loading, null = unknown/draft (identical generic response), otherwise the published event's header. */
  readonly header = signal<EntryHeader | null | undefined>(undefined);
  readonly loadError = signal(false);

  ngOnInit() { void this.load(); }

  async load() {
    this.loadError.set(false);
    this.header.set(undefined);
    try { this.header.set(await this.service.header(this.eventId)); }
    catch { this.loadError.set(true); }
  }

  authenticated(result: EntryResult) {
    // The server already validated authorizedInitialRoute (a requested deep link only if it's a recognized,
    // same-event route; otherwise its own computed default), so it's safe to navigate to directly.
    void this.router.navigateByUrl(result.authorizedInitialRoute, { replaceUrl: true });
  }
}
