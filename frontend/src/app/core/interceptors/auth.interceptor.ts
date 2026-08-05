import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

// Giden HER HTTP isteğinden önce çalışır. Token varsa isteğe Authorization
// header'ını ekler; register/login gibi zaten token'sız çalışması gereken
// isteklerde token yoktur, bu yüzden hiçbir şey eklenmeden istek olduğu gibi devam eder.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req);
};
