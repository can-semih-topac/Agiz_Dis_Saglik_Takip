import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { AuthNavbarComponent } from '../../shared/auth-navbar/auth-navbar.component';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, AuthNavbarComponent],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  constructor(title: Title) {
    title.setTitle('Şifremi Unuttum | ADS');
  }

  // Bu sayfaya profildeki "Şifremi unuttum" linkiyle, oturum açıkken de gelinebiliyor.
  // O durumda oturumu bozmadan "Ana Sayfaya Dön" gösteriyoruz, "Giriş sayfasına dön" değil.
  isLoggedIn = this.authService.isLoggedIn();

  // 3 adım: email doğrula (kod gönder) -> kodu doğrula -> yeni şifre.
  step: 'email' | 'code' | 'reset' = 'email';
  verifiedEmail = '';
  verifiedCode = '';

  isSubmitting = false;
  emailSubmitted = false;
  codeSubmitted = false;
  resetSubmitted = false;
  errorMessage = '';
  showNewPassword = false;
  showNewPasswordConfirm = false;

  emailForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  codeForm = this.fb.group({
    code: ['', [Validators.required, Validators.pattern(/^[0-9]{6}$/)]]
  });

  resetForm = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/)]],
    newPasswordConfirm: ['', Validators.required]
  });

  submitEmail(): void {
    this.emailSubmitted = true;

    if (this.emailForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    const email = this.emailForm.value.email!;

    this.authService.requestResetCode({ email }).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.verifiedEmail = email;
          this.step = 'code';
        } else {
          this.errorMessage = result.message ?? 'Kullanıcı bulunamadı.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  submitCode(): void {
    this.codeSubmitted = true;

    if (this.codeForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    const code = this.codeForm.value.code!;

    this.authService.verifyResetCode({ email: this.verifiedEmail, code }).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.verifiedCode = code;
          this.step = 'reset';
        } else {
          this.errorMessage = result.message ?? 'Kod hatalı ya da süresi dolmuş.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  submitReset(): void {
    this.resetSubmitted = true;

    if (this.resetForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.resetPassword({
      email: this.verifiedEmail,
      code: this.verifiedCode,
      newPassword: this.resetForm.value.newPassword!,
      newPasswordConfirm: this.resetForm.value.newPasswordConfirm!
    }).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          // Oturum zaten açıksa (profil sayfasından gelindiyse) tekrar login'e değil, ana sayfaya dön.
          this.router.navigate([this.isLoggedIn ? '/home' : '/login']);
        } else {
          this.errorMessage = result.message ?? 'Şifre güncellenemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
