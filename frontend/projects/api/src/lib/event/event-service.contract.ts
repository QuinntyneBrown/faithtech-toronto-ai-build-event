import { Signal } from "@angular/core";
import { PublicEventState } from "./public-event-state";

export interface IEventService {
  readonly state: Signal<PublicEventState | null>;
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  readonly connected: Signal<boolean>;
  load(): void;
  serverNow(): number;
}
