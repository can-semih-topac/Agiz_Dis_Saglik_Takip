import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UpdateProfileDto, UserProfileDto } from '../models/user.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/user`;

  getProfile(): Observable<ServiceResult<UserProfileDto>> {
    return this.http.get<ServiceResult<UserProfileDto>>(`${this.baseUrl}/profile`);
  }

  updateProfile(dto: UpdateProfileDto): Observable<ServiceResult> {
    return this.http.put<ServiceResult>(`${this.baseUrl}/profile`, dto);
  }
}
