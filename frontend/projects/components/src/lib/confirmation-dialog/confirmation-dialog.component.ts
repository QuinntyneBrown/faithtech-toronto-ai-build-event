import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import {
  ConfirmDialogData,
  ConfirmResult,
  CsButtonDirective,
  DialogActionsDirective,
  DialogDescriptionDirective,
  DialogShellComponent,
  DialogTitleDirective
} from "@quinntyne/cornerstone";

@Component({
  selector: "cs-confirm-dialog",
  imports: [CsButtonDirective, DialogActionsDirective, DialogDescriptionDirective, DialogShellComponent, DialogTitleDirective],
  templateUrl: "./confirmation-dialog.component.html",
  styleUrl: "./confirmation-dialog.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmationDialogComponent {
  protected readonly data = inject<ConfirmDialogData>(DIALOG_DATA);
  private readonly dialog = inject<DialogRef<ConfirmResult>>(DialogRef);

  protected close(result: ConfirmResult): void {
    this.dialog.close(result);
  }
}
