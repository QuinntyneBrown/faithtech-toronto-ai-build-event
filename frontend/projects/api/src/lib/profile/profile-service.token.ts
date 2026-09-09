import { InjectionToken } from "@angular/core";
import { IProfileService } from "./profile-service.contract";

export const PROFILE_SERVICE = new InjectionToken<IProfileService>("PROFILE_SERVICE");
