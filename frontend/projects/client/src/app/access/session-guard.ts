import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PARTICIPANT_SESSION_SERVICE } from '@faithtech/api';

/** Conceals private routes until the server confirms the session, on every entry: initial navigation,
 * browser history, and a restored tab all re-check rather than trusting cached client state. */
export const sessionGuard: CanActivateFn = async route => {
  const service = inject(PARTICIPANT_SESSION_SERVICE);
  const router = inject(Router);
  const eventId = route.paramMap.get('eventId');
  if (!eventId) return false;
  const state = await service.read(eventId);
  return state ? true : router.createUrlTree(['/events', eventId, 'access']);
};
