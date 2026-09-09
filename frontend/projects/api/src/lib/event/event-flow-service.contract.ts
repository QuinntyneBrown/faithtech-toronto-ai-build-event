import { Signal } from "@angular/core";

export interface IEventFlowService {
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  advance(fromScreen: string, toScreen: string, expectedVersion: string): void;
}
