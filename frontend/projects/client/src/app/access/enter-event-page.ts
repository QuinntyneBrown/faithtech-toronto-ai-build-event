import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ENTRY_SERVICE, EntryHeader, EntryResult } from '@faithtech/api';
import { ParticipantAccessForm } from '@faithtech/domain';

@Component({ selector: 'ft-enter-event-page', imports: [ParticipantAccessForm], templateUrl: './enter-event-page.html', styleUrl: './enter-event-page.css' })
export class EnterEventPage implements OnInit {
  private readonly service = inject(ENTRY_SERVICE);
  readonly eventId = inject(ActivatedRoute).snapshot.paramMap.get('eventId')!;
  /** undefined = still loading, null = unknown/draft (identical generic response), otherwise the published event's header. */
  readonly header = signal<EntryHeader | null | undefined>(undefined);
  readonly loadError = signal(false);
  readonly session = signal<EntryResult | null>(null);

  ngOnInit() { void this.load(); }

  async load() {
    this.loadError.set(false);
    this.header.set(undefined);
    try { this.header.set(await this.service.header(this.eventId)); }
    catch { this.loadError.set(true); }
  }

  authenticated(result: EntryResult) { this.session.set(result); }
}
