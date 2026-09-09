import { ChangeDetectionStrategy, Component, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BadgeComponent, CardComponent, CsButtonDirective, CsInputDirective, FieldComponent } from "@quinntyne/cornerstone";
import { ENTRY_SERVICE } from "@faithtech/api";
import { ProfileFormComponent } from "../profile/profile-form.component";

@Component({
  selector: "event-entry-form",
  imports: [FormsModule, BadgeComponent, CardComponent, CsButtonDirective, CsInputDirective, FieldComponent, ProfileFormComponent],
  templateUrl: "./entry-form.component.html",
  styleUrl: "./entry-form.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntryFormComponent {
  readonly entry = inject(ENTRY_SERVICE);
  readonly email = signal("");
  readonly eventVersion = input.required<string>();

  constructor() {
    this.entry.load();
  }

  enter(): void {
    this.entry.enter(this.email(), this.eventVersion());
  }
}
