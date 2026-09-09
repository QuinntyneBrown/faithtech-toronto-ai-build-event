import { provideHttpClient } from "@angular/common/http";
import { bootstrapApplication } from "@angular/platform-browser";
import { provideRouter } from "@angular/router";
import { provideCsTheme } from "@quinntyne/cornerstone";
import { ADMINISTRATOR_SESSION_SERVICE, ENTRY_SERVICE, EVENT_SERVICE, PROFILE_SERVICE, RAFFLE_SERVICE, AdministratorSessionService, EntryService, EventService, ProfileService, RaffleService } from "@faithtech/api";
import { AppComponent } from "./app/app.component";
import { routes } from "./app/app.routes";

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
    provideCsTheme("light"),
    { provide: EVENT_SERVICE, useClass: EventService },
    { provide: ENTRY_SERVICE, useClass: EntryService },
    { provide: PROFILE_SERVICE, useClass: ProfileService },
    { provide: RAFFLE_SERVICE, useClass: RaffleService },
    { provide: ADMINISTRATOR_SESSION_SERVICE, useClass: AdministratorSessionService }
  ]
}).catch(console.error);
