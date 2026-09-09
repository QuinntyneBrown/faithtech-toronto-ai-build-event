import { Signal } from "@angular/core";
import { EventState } from "../models/event-state";
import { Participant } from "../models/participant";
import { EventCommand } from "./event-command";
export interface IEventService {
  readonly state: Signal<EventState>;
  readonly ready: Signal<boolean>;
  readonly admin: Signal<boolean>;
  readonly participant: Signal<Participant | undefined>;
  readonly offline: Signal<boolean>;
  readonly busy: Signal<boolean>;
  readonly error: Signal<string>;
  readonly notice: Signal<string>;
  readonly failNext: Signal<boolean>;
  dispatch(command: EventCommand): Promise<boolean>;
  login(passcode: string): boolean;
  logout(): void;
  clearEntry(): void;
  setOffline(value: boolean): void;
  simulateFailure(): void;
  dismiss(): void;
}
