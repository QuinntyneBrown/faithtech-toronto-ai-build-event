import { Injectable } from '@angular/core';
import { IScheduleService, ScheduleDetail, ScheduleInput, ScheduleFailure } from '@faithtech/api';

@Injectable()
export class MockScheduleService implements IScheduleService {
  get(id: string) { return this.call('get', { id }); }
  save(id: string, input: ScheduleInput, version: string, operationId: string) { return this.call('save', { id, input, version, operationId }); }
  applyReference(id: string, version: string, operationId: string) { return this.call('reference', { id, version, operationId }); }
  private async call(operation: string, args: object): Promise<ScheduleDetail> {
    const bridge = window as unknown as { __faithtechSchedule(operation: string, args: object): Promise<{
      result: ScheduleDetail; failed?: boolean; status?: number; code?: string; errors?: Record<string, string[]>; current?: ScheduleDetail;
    }> };
    const response = await bridge.__faithtechSchedule(operation, args);
    if (response.failed || response.status) throw new ScheduleFailure(response.status ?? 0, response.code, response.errors, response.current);
    return response.result;
  }
}
