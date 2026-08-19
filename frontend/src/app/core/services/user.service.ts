import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChangePasswordDto, CreateUserByAdminDto, UpdateProfileDto, UserAdminDto, UserProfileDto } from '../models/user.models';
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

  changePassword(dto: ChangePasswordDto): Observable<ServiceResult> {
    return this.http.put<ServiceResult>(`${this.baseUrl}/change-password`, dto);
  }

  deleteAccount(): Observable<ServiceResult> {
    return this.http.delete<ServiceResult>(`${this.baseUrl}/account`);
  }

  // Sadece admin çağırabilir — backend 403/401 ile reddeder.
  getAllUsers(): Observable<ServiceResult<UserAdminDto[]>> {
    return this.http.get<ServiceResult<UserAdminDto[]>>(`${this.baseUrl}/all`);
  }

  createUser(dto: CreateUserByAdminDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(this.baseUrl, dto);
  }

  deleteUser(id: number): Observable<ServiceResult> {
    return this.http.delete<ServiceResult>(`${this.baseUrl}/${id}`);
  }
}
