import { Signal } from "@angular/core";
import { ProjectInput } from "./project-input";

export interface IProjectService {
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  add(input: ProjectInput, expectedVersion: string): void;
  update(projectId: string, input: ProjectInput, expectedVersion: string): void;
  remove(projectId: string, expectedVersion: string): void;
}
