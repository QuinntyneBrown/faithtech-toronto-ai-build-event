import { NativeControlStateDirective } from "@mock/components";
import {
  Component,
  ChangeDetectionStrategy,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  signal,
} from "@angular/core";
import { NavigationEnd, Router, RouterOutlet } from "@angular/router";
import { FormsModule } from "@angular/forms";
import {
  AlertComponent,
  BadgeComponent,
  CsButtonDirective,
  CsInputDirective,
  FieldComponent,
} from "@quinntyne/cornerstone";
import { ReviewDialogComponent } from "@mock/components";
import { EVENT_SERVICE } from "./data/event-service.token";
import { Screen, SCREENS, SCREEN_LABELS } from "./models/screen";
@Component({
  selector: "mock-app",
  imports: [
    NativeControlStateDirective,
    RouterOutlet,
    FormsModule,
    AlertComponent,
    BadgeComponent,
    CsButtonDirective,
    CsInputDirective,
    FieldComponent,
    ReviewDialogComponent,
  ],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  readonly event = inject(EVENT_SERVICE);
  private readonly router = inject(Router);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  readonly screens = SCREENS;
  readonly labels = SCREEN_LABELS;
  readonly view = signal<Screen>("countdown");
  readonly loginOpen = signal(false);
  readonly passcode = signal("");
  readonly controls = signal(false);
  readonly confirm = signal<"advance" | "populated" | "empty" | null>(null);
  readonly disabled = computed(() => this.event.busy() || this.event.offline());
  readonly next = computed(
    () => SCREENS[SCREENS.indexOf(this.event.state().stage) + 1],
  );
  constructor() {
    const subscription = this.router.events.subscribe((e) => {
      if (e instanceof NavigationEnd) {
        const path = e.urlAfterRedirects.split(/[/?#]/)[1] as Screen;
        if (SCREENS.includes(path)) this.view.set(path);
        requestAnimationFrame(() => {
          const heading =
            this.host.nativeElement.querySelector<HTMLElement>("main h1");
          if (heading) {
            heading.tabIndex = -1;
            heading.focus({ preventScroll: true });
          }
        });
      }
    });
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
    effect(() => {
      if (!this.event.ready()) return;
      const stage = this.event.state().stage;
      if (
        !this.event.admin() ||
        SCREENS.indexOf(this.view()) > SCREENS.indexOf(stage)
      )
        this.view.set(stage);
      if (this.router.url !== "/" + this.view())
        void this.router.navigateByUrl("/" + this.view(), {
          replaceUrl: !this.event.admin(),
        });
    });
  }
  canVisit(screen: Screen): boolean {
    return (
      this.event.admin() &&
      SCREENS.indexOf(screen) <= SCREENS.indexOf(this.event.state().stage)
    );
  }
  login(): void {
    if (this.event.login(this.passcode())) {
      this.loginOpen.set(false);
      this.passcode.set("");
    }
  }
  async confirmAction(): Promise<void> {
    const action = this.confirm();
    if (!action) return;
    if (
      await this.event.dispatch(
        action === "advance"
          ? { type: "advance" }
          : { type: "reset", populated: action === "populated" },
      )
    ) {
      this.confirm.set(null);
      this.view.set(this.event.state().stage);
    }
  }
}
