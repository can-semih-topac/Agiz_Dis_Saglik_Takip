import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateGoalStatusDto, GoalStatusDto, LongestStreakDto } from '../models/goal-status.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class GoalStatusService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/goalstatus`;

  // Dönen data, oluşturulan kaydın Id'si — notu aynı kayda bağlayabilmek için.
  createGoalStatus(dto: CreateGoalStatusDto): Observable<ServiceResult<number>> {
    return this.http.post<ServiceResult<number>>(this.baseUrl, dto);
  }

  getLast7Days(): Observable<ServiceResult<GoalStatusDto[]>> {
    return this.http.get<ServiceResult<GoalStatusDto[]>>(`${this.baseUrl}/last7days`);
  }

  // Takvim görünümü için — herhangi bir ayı gezebilmek amacıyla tüm geçmiş kayıtlar.
  getAll(): Observable<ServiceResult<GoalStatusDto[]>> {
    return this.http.get<ServiceResult<GoalStatusDto[]>>(`${this.baseUrl}/all`);
  }

  getLongestStreaks(): Observable<ServiceResult<LongestStreakDto[]>> {
    return this.http.get<ServiceResult<LongestStreakDto[]>>(`${this.baseUrl}/longest-streaks`);
  }
}
