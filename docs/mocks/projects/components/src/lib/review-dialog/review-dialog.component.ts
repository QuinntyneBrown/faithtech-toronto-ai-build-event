import { NativeControlStateDirective } from "../native-control-state.directive";
import {
  Component,
  ChangeDetectionStrategy,
  ElementRef,
  effect,
  input,
  output,
  viewChild,
} from "@angular/core";
import {
  CsButtonDirective,
  DialogShellComponent,
  DialogTitleDirective,
} from "@quinntyne/cornerstone";
@Component({
  selector: "mock-review-dialog",
  imports: [
    NativeControlStateDirective,
    CsButtonDirective,
    DialogShellComponent,
    DialogTitleDirective,
  ],
  templateUrl: "./review-dialog.component.html",
  styleUrl: "./review-dialog.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewDialogComponent {
  readonly headingId = "dialog-" + crypto.randomUUID();
  readonly title = input.required<string>();
  readonly open = input(false);
  readonly closed = output<void>();
  private readonly dialog = viewChild<ElementRef<HTMLDialogElement>>("dialog");
  constructor() {
    effect(() => {
      const el = this.dialog()?.nativeElement;
      if (!el) return;
      if (this.open() && !el.open) el.showModal();
      if (!this.open() && el.open) el.close();
    });
  }
}
