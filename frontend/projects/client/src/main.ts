import { provideHttpClient } from "@angular/common/http";
import { bootstrapApplication } from "@angular/platform-browser";
import { provideRouter } from "@angular/router";
import { provideCsTheme } from "@quinntyne/cornerstone";
import { ENTRY_SERVICE, EVENT_SERVICE, EntryService, EventService } from "@faithtech/api";
import { AppComponent } from "./app/app.component";
import { routes } from "./app/app.routes";

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
    provideCsTheme("light"),
    { provide: EVENT_SERVICE, useClass: EventService },
    { provide: ENTRY_SERVICE, useClass: EntryService }
  ]
}).catch(console.error);
