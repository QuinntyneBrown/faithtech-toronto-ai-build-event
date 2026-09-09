import { Participant } from '../models/participant';
import { Project } from '../models/project';
export type EventCommand =
  | { type: 'enter'; email: string; id: string }
  | { type: 'profile'; id: string; name: string; makes: string; heart: string }
  | { type: 'participantSave'; participant: Participant }
  | { type: 'participantDelete'; id: string }
  | { type: 'projectSave'; project: Project }
  | { type: 'projectDelete'; id: string }
  | { type: 'advance' }
  | { type: 'move'; participantId: string; teamId: string | null }
  | { type: 'assign'; teamId: string; projectId: string | null }
  | { type: 'draw' }
  | { type: 'reset'; populated: boolean }
  | { type: 'restartCountdown' };
