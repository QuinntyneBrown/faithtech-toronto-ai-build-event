import { ChangeDetectionStrategy, Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CsButtonDirective, CsInputDirective, FieldComponent } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-administrator-login",
  imports: [FormsModule, CsButtonDirective, CsInputDirective, FieldComponent],
  templateUrl: "./administrator-login.component.html",
  styleUrl: "./administrator-login.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdministratorLoginComponent {
  readonly session = inject(ADMINISTRATOR_SESSION_SERVICE);
  readonly open = signal(false);
  readonly passcode = signal("");

  signIn(): void {
    this.session.signIn(this.passcode());
  }
}
