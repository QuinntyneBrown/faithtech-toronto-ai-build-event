import { ChangeDetectionStrategy, Component, HostListener, computed, effect, inject } from "@angular/core";
import { NavigationEnd, Router, RouterOutlet } from "@angular/router";
import { toSignal } from "@angular/core/rxjs-interop";
import { filter, map } from "rxjs";
import { CardComponent, CsButtonDirective } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_SERVICE } from "@faithtech/api";
import { AdministratorLoginComponent } from "@faithtech/domain";

@Component({
  selector: "event-companion",
  imports: [AdministratorLoginComponent, CardComponent, CsButtonDirective, RouterOutlet],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  protected readonly event = inject(EVENT_SERVICE);
  protected readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  private readonly router = inject(Router);
  protected readonly route = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(event => event.urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );
  protected readonly openedScreens = computed(() => {
    const live = this.event.state()?.currentScreen;
    if (live === undefined) return [];
    return AppComponent.screens.slice(0, AppComponent.screens.indexOf(live) + 1);
  });
  protected readonly liveRoute = computed(() => `/${this.event.state()?.currentScreen ?? "countdown"}`);
  private static readonly screens = ["countdown", "projects", "teams", "raffle"] as const;

  constructor() {
    this.event.load();
    this.administrator.load();
    effect(() => {
      const screen = this.event.state()?.currentScreen;
      if (!screen) return;
      const currentScreen = this.route().slice(1);
      const currentIndex = AppComponent.screens.indexOf(currentScreen as typeof AppComponent.screens[number]);
      const liveIndex = AppComponent.screens.indexOf(screen);
      if ((!this.administrator.active() || currentIndex < 0 || currentIndex > liveIndex) && this.route() !== `/${screen}`) {
        void this.router.navigateByUrl(`/${screen}`);
      }
    });
  }

  protected navigateTo(screen: string): void {
    void this.router.navigateByUrl(`/${screen}`);
  }

  @HostListener("document:pointerdown", ["$event"])
  protected onPointerInteraction(event: PointerEvent): void {
    if (event.isTrusted) this.administrator.recordInteraction();
  }

  @HostListener("document:keydown", ["$event"])
  protected onKeyboardInteraction(event: KeyboardEvent): void {
    if (event.isTrusted) this.administrator.recordInteraction();
  }
}
