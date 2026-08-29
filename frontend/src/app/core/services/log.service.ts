import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LogDto } from '../models/log.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class LogService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/log`;

  getRecent(): Observable<ServiceResult<LogDto[]>> {
    return this.http.get<ServiceResult<LogDto[]>>(this.baseUrl);
  }

  search(keyword: string): Observable<ServiceResult<LogDto[]>> {
    return this.http.get<ServiceResult<LogDto[]>>(`${this.baseUrl}/search`, { params: { q: keyword } });
  }

  reindex(): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/reindex`, {});
  }
}
