import type { ScheduleDetail, ScheduleInput } from '../../frontend/projects/api/src/public-api';
import { EventFixture } from './event-fixture';

export class ScheduleFixture {
  readonly schedules = new Map<string, ScheduleDetail>();
  readonly receipts = new Map<string, { hash: string; result: ScheduleDetail }>();
  unavailable = false;
  loseNextResponse = false;
  validationErrors: Record<string, string[]> | null = null;
  saves = 0;
  references = 0;
  referenceInput: ScheduleInput = {
    timezone: 'America/Toronto', start: { local: '2026-09-09T17:00', offsetMinutes: -240 }, end: { local: '2026-09-09T21:00', offsetMinutes: -240 },
    stages: [{ id: '00000000-0000-0000-0000-000000000021', name: 'Reference arrival', phase: 'Gather', screenType: 'welcome', content: 'Welcome from the reference service', resourceUrl: null,
      start: { local: '2026-09-09T17:00', offsetMinutes: -240 }, end: { local: '2026-09-09T17:20', offsetMinutes: -240 } }],
    selection: { start: { local: '2026-09-09T18:05', offsetMinutes: -240 }, end: { local: '2026-09-09T18:15', offsetMinutes: -240 } },
    presentation: { start: { local: '2026-09-09T20:30', offsetMinutes: -240 }, end: { local: '2026-09-09T20:50', offsetMinutes: -240 } },
  };
  constructor(private readonly events: EventFixture) {}
  handle(operation: string, args: { id: string; input?: ScheduleInput; version?: string; operationId?: string }) {
    if (this.unavailable) return { status: 503 };
    const event = this.events.events.get(args.id);
    if (!event) return { status: 404 };
    const current: ScheduleDetail = { stages: [], selection: null, presentation: null, selectionClosed: false, completed: false,
      ...this.schedules.get(args.id), id: event.id, version: event.version, timezone: event.timezone, start: event.start, end: event.end };
    if (operation === 'get') return { result: current };
    const hash = JSON.stringify({ operation, args }), receipt = this.receipts.get(args.operationId!);
    if (receipt) return receipt.hash === hash ? { result: receipt.result } : { status: 409, code: 'operation-key-reused' };
    if (args.version !== current.version) return { status: 409, code: 'stale-version', current };
    if (this.validationErrors) return { status: 422, errors: this.validationErrors };
    const input = operation === 'reference' ? structuredClone(this.referenceInput) : args.input!;
    const stages = [...input.stages].sort((a, b) => a.start!.local.localeCompare(b.start!.local));
    if (stages.some((stage, index) => index > 0 && stage.start!.local < stages[index - 1].end!.local))
      return { status: 422, errors: { stages: ['Stages overlap. Adjust their times before saving.'] } };
    const result = { ...current, ...input, stages, version: String(Number(current.version) + 1) };
    this.schedules.set(args.id, result);
    this.events.events.set(args.id, { ...event, timezone: input.timezone, start: input.start, end: input.end, version: result.version });
    if (operation === 'reference') { this.references++; this.events.events.set(args.id, { ...this.events.events.get(args.id)!, venueName: 'Stone Church', useLiturgy: false }); }
    this.receipts.set(args.operationId!, { hash, result }); this.saves++;
    if (this.loseNextResponse) { this.loseNextResponse = false; return { failed: true }; }
    return { result };
  }
}
