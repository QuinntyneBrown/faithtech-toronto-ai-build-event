import { randomUUID } from 'node:crypto';
import type { EventDetail, EventInput } from '../../frontend/projects/api/src/public-api';

export class EventFixture {
  readonly events = new Map<string, EventDetail>();
  readonly receipts = new Map<string, {hash: string; result: EventDetail}>();
  loseNextSaveResponse = false;
  unavailable = false;
  saves = 0;
  logoUploads = 0;
  loseNextLogoResponse = false;
  logoUnavailable = false;
  readonly logos = new Map<string, {bytes: number[]; mediaType: string}>();
  handle(operation: string, args: {id?: string; input?: EventInput; title?: string; version?: string; operationId?: string;
    bytes?: number[]; mediaType?: string; name?: string}) {
    if (this.unavailable) return { status: 503 };
    if (operation === 'list') return { result: [...this.events.values()] };
    const current = args.id ? this.events.get(args.id) : undefined;
    if (operation === 'get') return current ? { result: current } : { status: 404 };
    if (operation === 'getLogo') return this.logoUnavailable ? { status: 503 } : this.logos.has(args.id!) ? { result: this.logos.get(args.id!) } : { status: 404 };
    const receipt = this.receipts.get(args.operationId!);
    const hash = JSON.stringify({ operation, ...args });
    if (receipt) return receipt.hash === hash ? { result: receipt.result } : { status: 409, code: 'operation-key-reused' };
    if ((operation === 'save' || operation === 'uploadLogo') && current?.version !== args.version) return { status: 409, code: 'stale-version', current };
    if (operation === 'uploadLogo') {
      if (!['image/png', 'image/jpeg', 'image/webp'].includes(args.mediaType!))
        return { status: 422, errors: { logo: ['Choose a PNG, JPEG or WebP file with a matching image type.'] } };
      const result = { ...current!, version: String(Number(args.version) + 1),
        logo: { id: randomUUID(), mediaType: args.mediaType!, width: 1, height: 1 } };
      this.logos.set(args.id!, { bytes: args.bytes!, mediaType: args.mediaType! });
      this.events.set(args.id!, result); this.receipts.set(args.operationId!, { hash, result }); this.logoUploads++;
      if (this.loseNextLogoResponse) { this.loseNextLogoResponse = false; return { status: 0, failed: true }; }
      return { result };
    }
    if (operation === 'save' && args.input?.directionsUrl && !/^https:\/\/[^@/]+(?:\/|$)/.test(args.input.directionsUrl))
      return { status: 422, errors: { directionsUrl: ['Use an absolute HTTPS URL without credentials.'] } };
    const result: EventDetail = operation === 'create' ? {
      id: randomUUID(), title: args.title?.trim() || null, published: false, useLiturgy: false, version: '1',
      venueName: null, address: null, latitude: null, longitude: null, waitingContent: null, closingContent: null, directionsUrl: null,
      timezone: null, start: null, end: null, startsAtUtc: null, endsAtUtc: null, logo: null,
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
