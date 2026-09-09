import { Signal } from "@angular/core";
import { RaffleSnapshot } from "./raffle-snapshot";

export interface IRaffleService {
  readonly state: Signal<RaffleSnapshot | null>;
  readonly loading: Signal<boolean>;
  readonly drawing: Signal<boolean>;
  readonly error: Signal<string | null>;
  load(): void;
  draw(expectedVersion: string): void;
}
