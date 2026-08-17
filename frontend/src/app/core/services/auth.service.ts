import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthSession, GoogleLoginDto, LoginDto, LoginResultDto, RegisterDto, ResetPasswordDto, VerifyEmailDto, VerifyResetCodeDto } from '../models/auth.models';
import { ServiceResult } from '../models/service-result';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/auth`;
  private readonly storageKey = 'auth_session';

  register(dto: RegisterDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/register`, dto);
  }

  login(dto: LoginDto): Observable<ServiceResult<LoginResultDto>> {
    return this.http.post<ServiceResult<LoginResultDto>>(`${this.baseUrl}/login`, dto);
  }

  googleLogin(dto: GoogleLoginDto): Observable<ServiceResult<LoginResultDto>> {
    return this.http.post<ServiceResult<LoginResultDto>>(`${this.baseUrl}/google`, dto);
  }

  requestResetCode(dto: VerifyEmailDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/forgot-password/request-code`, dto);
  }

  verifyResetCode(dto: VerifyResetCodeDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/forgot-password/verify-code`, dto);
  }

  resetPassword(dto: ResetPasswordDto): Observable<ServiceResult> {
    return this.http.post<ServiceResult>(`${this.baseUrl}/forgot-password/reset`, dto);
  }

  // Giriş başarılı olunca component bunu çağırıp token'ı kalıcı hale getirecek.
  saveSession(result: LoginResultDto): void {
    const session: AuthSession = {
      token: result.token,
      email: result.email,
      fullName: result.fullName,
      isAdmin: result.isAdmin
    };
    localStorage.setItem(this.storageKey, JSON.stringify(session));
  }

  getSession(): AuthSession | null {
    const raw = localStorage.getItem(this.storageKey);
    return raw ? JSON.parse(raw) : null;
  }

  getToken(): string | null {
    return this.getSession()?.token ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.getSession()?.isAdmin ?? false;
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
  }
}
