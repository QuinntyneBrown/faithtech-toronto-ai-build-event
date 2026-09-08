import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { IScheduleService } from './schedule-service.contract';
import { ScheduleInput } from './schedule-input';
import { ScheduleDetail } from './schedule-detail';
import { ScheduleFailure } from './schedule-failure';

@Injectable()
export class ScheduleService implements IScheduleService {
  private readonly http = inject(HttpClient);
  async get(id: string): Promise<ScheduleDetail> {
    try { return await firstValueFrom(this.http.get<ScheduleDetail>(`/api/admin/events/${encodeURIComponent(id)}/schedule`)); }
    catch (error) { throw new ScheduleFailure(error instanceof HttpErrorResponse ? error.status : 0); }
  }
  async save(id: string, input: ScheduleInput, version: string, operationId: string): Promise<ScheduleDetail> {
    try {
      const token = await firstValueFrom(this.http.get<{ requestToken: string }>('/api/admin/antiforgery'));
      return await firstValueFrom(this.http.put<ScheduleDetail>(`/api/admin/events/${encodeURIComponent(id)}/schedule`, input, {
        headers: { 'X-CSRF-TOKEN': token.requestToken, 'Idempotency-Key': operationId, 'If-Match': `"${version}"` },
      }));
    } catch (error) {
      if (error instanceof HttpErrorResponse) throw new ScheduleFailure(error.status, error.error?.code, error.error?.errors, error.error?.current);
      throw new ScheduleFailure(0);
    }
  }
}
