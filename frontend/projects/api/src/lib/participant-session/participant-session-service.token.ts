import { InjectionToken } from '@angular/core';
import { IParticipantSessionService } from './participant-session-service.contract';

export const PARTICIPANT_SESSION_SERVICE = new InjectionToken<IParticipantSessionService>('PARTICIPANT_SESSION_SERVICE');
