import { InjectionToken } from "@angular/core";
import { IRosterService } from "./roster-service.contract";

export const ROSTER_SERVICE = new InjectionToken<IRosterService>("ROSTER_SERVICE");
