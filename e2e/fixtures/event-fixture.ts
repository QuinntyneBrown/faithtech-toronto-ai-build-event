import { randomUUID } from 'node:crypto';
import type { EventDetail, EventInput } from '../../frontend/projects/api/src/public-api';

export class EventFixture {
  readonly events = new Map<string, EventDetail>();
  readonly receipts = new Map<string, {hash: string; result: EventDetail}>();
  loseNextSaveResponse = false;
  unavailable = false;
  saves = 0;
  handle(operation: string, args: {id?: string; input?: EventInput; title?: string; version?: string; operationId?: string}) {
    if (this.unavailable) return { status: 503 };
    if (operation === 'list') return { result: [...this.events.values()] };
    const current = args.id ? this.events.get(args.id) : undefined;
    if (operation === 'get') return current ? { result: current } : { status: 404 };
    const receipt = this.receipts.get(args.operationId!);
    const hash = JSON.stringify({ operation, ...args });
    if (receipt) return receipt.hash === hash ? { result: receipt.result } : { status: 409, code: 'operation-key-reused' };
    if (operation === 'save' && current?.version !== args.version) return { status: 409, code: 'stale-version', current };
    if (operation === 'save' && args.input?.directionsUrl && !/^https:\/\/[^@/]+(?:\/|$)/.test(args.input.directionsUrl))
      return { status: 422, errors: { directionsUrl: ['Use an absolute HTTPS URL without credentials.'] } };
    const result: EventDetail = operation === 'create' ? {
      id: randomUUID(), title: args.title?.trim() || null, published: false, useLiturgy: false, version: '1',
      venueName: null, address: null, latitude: null, longitude: null, waitingContent: null, closingContent: null, directionsUrl: null,
    } : { ...current!, ...args.input!, version: String(Number(args.version) + 1) };
    this.events.set(result.id, result);
    this.receipts.set(args.operationId!, { hash, result });
    if (operation === 'save') {
      this.saves++;
      if (this.loseNextSaveResponse) { this.loseNextSaveResponse = false; return { status: 0, failed: true }; }
    }
    return { result };
  }
}
