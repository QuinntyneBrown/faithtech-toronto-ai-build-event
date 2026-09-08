import { SessionState } from './session-state';

export interface ISessionService {
  read(): Promise<SessionState | null>;
  signIn(username: string, password: string): Promise<SessionState>;
  signOut(): Promise<void>;
  interact(): Promise<void>;
}
