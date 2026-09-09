import { Signal } from "@angular/core";

export interface EntryConfirmation {
  participantId: string;
  publicLabel: string;
}

export interface IEntryService {
  readonly confirmation: Signal<EntryConfirmation | null>;
  readonly loading: Signal<boolean>;
  readonly error: Signal<string | null>;
  enter(email: string, expectedVersion: string): void;
}
