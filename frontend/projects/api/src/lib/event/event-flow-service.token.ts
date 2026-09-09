import { InjectionToken } from "@angular/core";
import { IEventFlowService } from "./event-flow-service.contract";

export const EVENT_FLOW_SERVICE = new InjectionToken<IEventFlowService>("EVENT_FLOW_SERVICE");
