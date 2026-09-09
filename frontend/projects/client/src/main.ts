import { provideHttpClient } from "@angular/common/http";
import { bootstrapApplication } from "@angular/platform-browser";
import { provideRouter } from "@angular/router";
import { provideCsTheme } from "@quinntyne/cornerstone";
import { EVENT_SERVICE, EventService } from "@faithtech/api";
import { AppComponent } from "./app/app.component";
import { routes } from "./app/app.routes";

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
    provideCsTheme("light"),
    { provide: EVENT_SERVICE, useClass: EventService }
  ]
}).catch(console.error);
