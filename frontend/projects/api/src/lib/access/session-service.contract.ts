import { SessionState } from './session-state';
import { Signal } from '@angular/core';

export interface ISessionService {
  readonly interactionRevision: Signal<number>;
  read(): Promise<SessionState | null>;
  signIn(username: string, password: string): Promise<SessionState>;
  signOut(): Promise<void>;
  interact(): Promise<void>;
}
