import { InjectionToken } from "@angular/core";
import { ITeamService } from "./team-service.contract";

export const TEAM_SERVICE = new InjectionToken<ITeamService>("TEAM_SERVICE");
