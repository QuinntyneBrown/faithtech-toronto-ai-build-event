import { InjectionToken } from "@angular/core";
import { IEventService } from "./event-service.contract";
export const EVENT_SERVICE = new InjectionToken<IEventService>("Event mock");
