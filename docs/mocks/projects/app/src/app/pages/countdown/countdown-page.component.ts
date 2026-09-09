import { NativeControlStateDirective } from "@mock/components";
import {
  Component,
  ChangeDetectionStrategy,
  DestroyRef,
  computed,
  inject,
  signal,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  CardComponent,
  CsButtonDirective,
  FieldComponent,
  CsInputDirective,
  CsTextareaDirective,
  BadgeComponent,
} from "@quinntyne/cornerstone";
import { CountdownComponent } from "@mock/components";
import { EVENT_SERVICE } from "../../data/event-service.token";
import { ParticipantManagerComponent } from "./participant-manager.component";
@Component({
  selector: "mock-countdown-page",
  imports: [
    NativeControlStateDirective,
    FormsModule,
    CardComponent,
    CsButtonDirective,
    FieldComponent,
    CsInputDirective,
    CsTextareaDirective,
    BadgeComponent,
    CountdownComponent,
    ParticipantManagerComponent,
  ],
  templateUrl: "./countdown-page.component.html",
  styleUrl: "./countdown-page.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CountdownPageComponent {
  readonly event = inject(EVENT_SERVICE);
  readonly now = signal(Date.now());
  readonly email = signal("");
  readonly name = signal("");
  readonly makes = signal("");
  readonly heart = signal("");
  readonly editing = signal(false);
  readonly skip = signal(false);
  private readonly entryId = crypto.randomUUID();
  readonly closed = computed(() => this.event.state().stage !== "countdown");
  constructor() {
    const timer = setInterval(() => this.now.set(Date.now()), 250);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }
  async enter(): Promise<void> {
    if (
      await this.event.dispatch({
        type: "enter",
        email: this.email(),
        id: this.entryId,
      })
    ) {
      this.editing.set(true);
      this.skip.set(false);
    }
  }
  edit(): void {
    const p = this.event.participant();
    if (!p) return;
    this.name.set(p.name);
    this.makes.set(p.makes);
    this.heart.set(p.heart);
    this.editing.set(true);
    this.skip.set(false);
  }
  async save(): Promise<void> {
    const p = this.event.participant();
    if (
      p &&
      (await this.event.dispatch({
        type: "profile",
        id: p.id,
        name: this.name(),
        makes: this.makes(),
        heart: this.heart(),
      }))
    )
      this.editing.set(false);
  }
}
