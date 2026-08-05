import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterDto } from '../models/auth.models';
import { ServiceResult } from '../models/service-result';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/auth`;

  register(dto: RegisterDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/register`, dto);
  }
}
