import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from "@angular/core";
import { CardComponent, CountdownComponent as CsCountdownComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_FLOW_SERVICE, EVENT_SERVICE } from "@faithtech/api";
import { EventHeaderComponent } from "@faithtech/components";
import { EntryFormComponent } from "../entry/entry-form.component";
import { AdministratorLoginComponent } from "../access/administrator-login.component";
import { ParticipantRosterComponent } from "../roster/participant-roster.component";

@Component({
  selector: "event-countdown",
  imports: [CardComponent, CsCountdownComponent, EventHeaderComponent, EntryFormComponent, AdministratorLoginComponent, ParticipantRosterComponent],
  templateUrl: "./countdown.component.html",
  styleUrl: "./countdown.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CountdownComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly flow = inject(EVENT_FLOW_SERVICE);
  readonly now = signal(this.event.serverNow());
  readonly Date = Date;

  constructor() {
    this.event.load();
    const timer = window.setInterval(() => this.now.set(this.event.serverNow()), 250);
    inject(DestroyRef).onDestroy(() => window.clearInterval(timer));
  }

  advance(): void { const state = this.event.state(); if (state) this.flow.advance("countdown", "projects", state.version); }
}
