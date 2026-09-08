import { ParticipantSessionState } from './participant-session-state';

export interface IParticipantSessionService {
  read(eventId: string): Promise<ParticipantSessionState | null>;
  signOut(eventId: string): Promise<void>;
}
