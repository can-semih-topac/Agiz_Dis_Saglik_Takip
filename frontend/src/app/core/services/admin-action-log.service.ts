import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminActionLogDto } from '../models/admin-action-log.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class AdminActionLogService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/adminactionlog`;

  getRecent(): Observable<ServiceResult<AdminActionLogDto[]>> {
    return this.http.get<ServiceResult<AdminActionLogDto[]>>(this.baseUrl);
  }
}
