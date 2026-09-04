import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthSession, GoogleLoginDto, LoginDto, LoginResultDto, RefreshTokenDto, RegisterDto, ResetPasswordDto, VerifyEmailDto, VerifyResetCodeDto } from '../models/auth.models';
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

  // Demo hesabının verilerini sıfırlayıp tazeler ve o hesaba giriş token'ı döner.
  enterDemo(): Observable<ServiceResult<LoginResultDto>> {
    return this.http.post<ServiceResult<LoginResultDto>>(`${this.baseUrl}/demo`, {});
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

  // Access token (15 dk) süresi dolunca interceptor bunu çağırıp sessizce yeni bir
  // access token alır — kullanıcı hiçbir şey fark etmeden oturumu devam eder.
  refreshToken(refreshToken: string): Observable<ServiceResult<LoginResultDto>> {
    const dto: RefreshTokenDto = { refreshToken };
    return this.http.post<ServiceResult<LoginResultDto>>(`${this.baseUrl}/refresh`, dto);
  }

  // Giriş başarılı olunca component bunu çağırıp token'ı kalıcı hale getirecek.
  saveSession(result: LoginResultDto): void {
    const session: AuthSession = {
      token: result.token,
      refreshToken: result.refreshToken,
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

  getRefreshToken(): string | null {
    return this.getSession()?.refreshToken ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.getSession()?.isAdmin ?? false;
  }

  // Sunucuya refresh token'ı iptal ettirmeye çalışır (en iyi çaba — başarısız olsa bile
  // localStorage zaten temizlenecek, kullanıcı için oturum bittiği gerçeği değişmez).
  // Bu yüzden component'ler subscribe olmak zorunda kalmasın diye senkron kalıyor.
  logout(): void {
    const refreshToken = this.getRefreshToken();
    localStorage.removeItem(this.storageKey);

    if (refreshToken) {
      const dto: RefreshTokenDto = { refreshToken };
      this.http.post(`${this.baseUrl}/logout`, dto).pipe(catchError(() => of(null))).subscribe();
    }
  }
}
