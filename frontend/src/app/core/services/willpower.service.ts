import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WillpowerGranularity, WillpowerHistoryPointDto, WillpowerScoreDto } from '../models/willpower.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class WillpowerService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/willpower`;

  getScore(): Observable<ServiceResult<WillpowerScoreDto>> {
    return this.http.get<ServiceResult<WillpowerScoreDto>>(`${this.baseUrl}/score`);
  }

  getHistory(granularity: WillpowerGranularity): Observable<ServiceResult<WillpowerHistoryPointDto[]>> {
    return this.http.get<ServiceResult<WillpowerHistoryPointDto[]>>(`${this.baseUrl}/history`, {
      params: { granularity }
    });
  }
}
