import type { RegistrationInput, RosterEntry, RosterIssuance } from '../../frontend/projects/api/src/public-api';
import { EventFixture } from './event-fixture';

export class RosterFixture {
  readonly entries = new Map<string, RosterEntry[]>();
  readonly receipts = new Map<string, { hash: string; result: RosterIssuance }>();
  unavailable = false;
  loseNextResponse = false;
  additions = 0;
  constructor(private readonly events: EventFixture) {}
  handle(operation: string, args: { eventId: string; input?: RegistrationInput; operationId?: string; registrationId?: string; displayName?: string; version?: string; }) {
    if (this.unavailable) return { status: 503 };
    if (!this.events.events.has(args.eventId)) return { status: 404 };
    const entries = this.entries.get(args.eventId) ?? [];
    if (operation === 'list') return { result: entries };
    if (operation === 'rename') {
      const entry = entries.find(x => x.id === args.registrationId);
      if (!entry) return { status: 404 };
      const name = (args.displayName ?? '').trim().replace(/\r\n?/g, '\n');
      if (!name || Array.from(name).length > 200) return { status: 422, errors: { displayName: ['Enter a participant name of 1–200 characters.'] } };
      if (entry.version !== args.version) return { status: 409, code: 'stale-version' };
      entry.displayName = name; entry.version = (Number(entry.version) + 1).toString();
      return { result: entry };
    }
    if (operation === 'deactivate') {
      const entry = entries.find(x => x.id === args.registrationId);
      if (!entry) return { status: 404 };
      if (entry.version !== args.version) return { status: 409, code: 'stale-version' };
      entry.active = false; entry.version = (Number(entry.version) + 1).toString();
      return { result: entry };
    }
    if (operation === 'replaceCode') {
      const entry = entries.find(x => x.id === args.registrationId);
      if (!entry) return { status: 404 };
      if (entry.version !== args.version) return { status: 409, code: 'stale-version' };
      entry.version = (Number(entry.version) + 1).toString();
      const result: RosterIssuance = { entry, code: (++this.additions).toString(16).padStart(32, '0'), previouslyCompleted: false, credentialVersion: crypto.randomUUID() };
      return { result };
    }
    if (operation === 'reactivate') {
      const entry = entries.find(x => x.id === args.registrationId);
      if (!entry) return { status: 404 };
      if (entry.version !== args.version) return { status: 409, code: 'stale-version' };
      entry.active = true; entry.version = (Number(entry.version) + 1).toString();
      return { result: entry };
    }
    const name = args.input!.displayName.trim().replace(/\r\n?/g, '\n');
    if (!name || Array.from(name).length > 200) return { status: 422, errors: { displayName: ['Enter a participant name of 1–200 characters.'] } };
    const key = args.eventId + ':' + args.operationId, hash = JSON.stringify({ ...args, input: { displayName: name } }), receipt = this.receipts.get(key);
    if (receipt) return receipt.hash === hash ? { result: receipt.result } : { status: 409, code: 'operation-key-reused' };
    const entry: RosterEntry = { id: crypto.randomUUID(), displayName: name, active: true, emailBound: false, firstAccessAtUtc: null, version: '1' };
    const result: RosterIssuance = { entry, code: (++this.additions).toString(16).padStart(32, '0'), previouslyCompleted: false, credentialVersion: crypto.randomUUID() };
    this.entries.set(args.eventId, [...entries, entry]); this.receipts.set(key, { hash, result: { ...result, code: null, previouslyCompleted: true } });
    if (this.loseNextResponse) { this.loseNextResponse = false; return { failed: true }; }
    return { result };
  }
}
