import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({ selector: 'ft-current-event-page', templateUrl: './current-event-page.html', styleUrl: './current-event-page.css' })
export class CurrentEventPage {
  private readonly route = inject(ActivatedRoute);
  readonly eventId = this.route.snapshot.paramMap.get('eventId') ?? this.route.parent!.snapshot.paramMap.get('eventId')!;
}
