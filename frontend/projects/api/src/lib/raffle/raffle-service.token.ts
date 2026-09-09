import { InjectionToken } from "@angular/core";
import { IRaffleService } from "./raffle-service.contract";

export const RAFFLE_SERVICE = new InjectionToken<IRaffleService>("RAFFLE_SERVICE");
