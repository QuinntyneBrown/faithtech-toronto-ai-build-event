import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from "@angular/core";
import { CardComponent, CountdownComponent as CsCountdownComponent } from "@quinntyne/cornerstone";
import { EVENT_SERVICE } from "@faithtech/api";
import { EventHeaderComponent } from "@faithtech/components";
import { EntryFormComponent } from "../entry/entry-form.component";
import { AdministratorLoginComponent } from "../access/administrator-login.component";

@Component({
  selector: "event-countdown",
  imports: [CardComponent, CsCountdownComponent, EventHeaderComponent, EntryFormComponent, AdministratorLoginComponent],
  templateUrl: "./countdown.component.html",
  styleUrl: "./countdown.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CountdownComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly now = signal(Date.now());
  readonly Date = Date;

  constructor() {
    this.event.load();
    const timer = window.setInterval(() => this.now.set(Date.now()), 250);
    inject(DestroyRef).onDestroy(() => window.clearInterval(timer));
  }
}
