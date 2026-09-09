import { ChangeDetectionStrategy, Component, effect, inject } from "@angular/core";
import { Router, RouterOutlet } from "@angular/router";
import { CardComponent, CsButtonDirective } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, EVENT_SERVICE } from "@faithtech/api";

@Component({
  selector: "event-companion",
  imports: [CardComponent, CsButtonDirective, RouterOutlet],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  protected readonly event = inject(EVENT_SERVICE);
  private readonly administrator = inject(ADMINISTRATOR_SESSION_SERVICE);
  private readonly router = inject(Router);

  constructor() {
    this.event.load();
    this.administrator.load();
    effect(() => {
      const screen = this.event.state()?.currentScreen;
      if (screen && !this.administrator.active() && this.router.url !== `/${screen}`) {
        void this.router.navigateByUrl(`/${screen}`);
      }
    });
  }
}
