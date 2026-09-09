/**
 * The part of the mocked backend that belongs to one browser rather than to
 * the event.
 *
 * The real service keeps these in cookies, so two browsers pointed at the same
 * event have independent administrator and entry sessions. Holding them here
 * rather than on the store is what lets a test sign in on one page and prove
 * the other is still a public viewer.
 */
export class BrowserSession {
  administratorAuthenticated = false;
  participantId: string | null = null;
}
