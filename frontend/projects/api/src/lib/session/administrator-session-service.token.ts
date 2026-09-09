import { InjectionToken } from "@angular/core";
import { IAdministratorSessionService } from "./administrator-session-service.contract";

export const ADMINISTRATOR_SESSION_SERVICE = new InjectionToken<IAdministratorSessionService>("ADMINISTRATOR_SESSION_SERVICE");
