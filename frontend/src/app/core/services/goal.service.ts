import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateGoalDto, GoalDto, StartGoalPauseDto, UpdateGoalDto } from '../models/goal.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class GoalService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/goal`;

  getGoals(): Observable<ServiceResult<GoalDto[]>> {
    return this.http.get<ServiceResult<GoalDto[]>>(this.baseUrl);
  }

  createGoal(dto: CreateGoalDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(this.baseUrl, dto);
  }

  updateGoal(id: number, dto: UpdateGoalDto): Observable<ServiceResult> {
    return this.http.put<ServiceResult>(`${this.baseUrl}/${id}`, dto);
  }

  deleteGoal(id: number, confirmed: boolean): Observable<ServiceResult<boolean>> {
    return this.http.delete<ServiceResult<boolean>>(`${this.baseUrl}/${id}?confirmed=${confirmed}`);
  }

  pauseGoal(id: number, dto: StartGoalPauseDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/${id}/pause`, dto);
  }

  resumeGoal(id: number): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/${id}/resume`, {});
  }
}
