import { InjectionToken } from "@angular/core";
import { IProjectService } from "./project-service.contract";

export const PROJECT_SERVICE = new InjectionToken<IProjectService>("PROJECT_SERVICE");
