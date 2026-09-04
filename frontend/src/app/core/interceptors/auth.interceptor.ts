import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

// Access token kısa ömürlü (15 dk) — süresi dolunca sunucu 401 döner. Bunu burada yakalayıp
// refresh token ile arka planda sessizce yeni bir access token alıyor ve başarısız olan
// isteği otomatik tekrar deniyoruz; kullanıcı login sayfasına atılmadan işine devam eder.
// Refresh token da geçersizse (7 gün doldu, token çalıntı şüphesiyle sunucu tarafında
// tüm oturumlar kapatıldı vb.) oturum tamamen temizlenip giriş sayfasına yönlendiriliyor.

// Modül seviyesinde (component/servis örneğinden bağımsız) tutuluyor ki aynı anda başarısız
// olan birden fazla istek TEK bir refresh çağrısını paylaşsın — her biri ayrı ayrı refresh
// denerse hem gereksiz yük olur hem de rotasyon nedeniyle biri diğerinin token'ını geçersiz kılar.
let isRefreshing = false;
const refreshedTokenSubject = new BehaviorSubject<string | null>(null);

const isAuthEndpoint = (url: string): boolean =>
  url.includes('/auth/login') ||
  url.includes('/auth/register') ||
  url.includes('/auth/google') ||
  url.includes('/auth/refresh') ||
  url.includes('/auth/demo') ||
  url.includes('/auth/forgot-password');

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      // Login/register/refresh gibi zaten token'sız ya da token yenilemenin kendisi olan
      // isteklerde 401'i normal bir hata olarak geçiriyoruz — yoksa sonsuz döngüye girer.
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthEndpoint(req.url)) {
        return throwError(() => error);
      }

      const refreshToken = authService.getRefreshToken();
      if (!refreshToken) {
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      if (!isRefreshing) {
        isRefreshing = true;
        refreshedTokenSubject.next(null);

        return authService.refreshToken(refreshToken).pipe(
          switchMap(result => {
            isRefreshing = false;

            if (!result.success || !result.data) {
              authService.logout();
              router.navigate(['/login']);
              return throwError(() => error);
            }

            authService.saveSession(result.data);
            refreshedTokenSubject.next(result.data.token);

            const retriedReq = req.clone({
              setHeaders: { Authorization: `Bearer ${result.data.token}` }
            });
            return next(retriedReq);
          }),
          catchError(refreshError => {
            isRefreshing = false;
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }

      // Aynı anda birden fazla istek 401 aldıysa (ör. sayfa açılışında paralel birkaç çağrı),
      // hepsi tek bir refresh isteğinin bitmesini bekleyip aynı yeni token'la devam eder.
      return refreshedTokenSubject.pipe(
        filter((newToken): newToken is string => newToken !== null),
        take(1),
        switchMap(newToken => {
          const retriedReq = req.clone({
            setHeaders: { Authorization: `Bearer ${newToken}` }
          });
          return next(retriedReq);
        })
      );
    })
  );
};
