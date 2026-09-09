import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CsButtonDirective, CsInputDirective, CsTextareaDirective, FieldComponent } from "@quinntyne/cornerstone";
import { EVENT_SERVICE, PROFILE_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-profile-form",
  imports: [FormsModule, CsButtonDirective, CsInputDirective, CsTextareaDirective, FieldComponent],
  templateUrl: "./profile-form.component.html",
  styleUrl: "./profile-form.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileFormComponent {
  readonly profileService = inject(PROFILE_SERVICE);
  readonly event = inject(EVENT_SERVICE);
  readonly eventVersion = input.required<string>();
  readonly name = signal("");
  readonly whatYouMake = signal("");
  readonly onYourHeart = signal("");
  readonly skipped = signal(false);

  constructor() {
    this.profileService.load();
    effect(() => {
      const profile = this.profileService.profile();
      if (profile !== null) {
        this.name.set(profile.name ?? "");
        this.whatYouMake.set(profile.whatYouMake ?? "");
        this.onYourHeart.set(profile.onYourHeart ?? "");
      }
    });
  }

  save(): void {
    this.profileService.save({
      name: this.name() || null,
      whatYouMake: this.whatYouMake() || null,
      onYourHeart: this.onYourHeart() || null
    }, this.eventVersion());
  }

  skip(): void {
    this.skipped.set(true);
  }

  resume(): void {
    this.skipped.set(false);
  }
}
