import { Signal } from "@angular/core";

export interface IAdministratorSessionService {
  readonly active: Signal<boolean>;
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  load(): void;
  recordInteraction(): void;
  signIn(passcode: string): void;
  signOut(): void;
}
