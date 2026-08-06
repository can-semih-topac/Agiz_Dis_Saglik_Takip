import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  constructor(title: Title) {
    title.setTitle('Parolamı Unuttum | ADS');
  }

  // Form gereği: önce sadece email doğrulanır, kayıtlıysa parola alanları açılır.
  step: 'email' | 'reset' = 'email';
  verifiedEmail = '';

  isSubmitting = false;
  emailSubmitted = false;
  resetSubmitted = false;
  errorMessage = '';

  emailForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
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

    this.authService.verifyEmail({ email }).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.verifiedEmail = email;
          this.step = 'reset';
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

  submitReset(): void {
    this.resetSubmitted = true;

    if (this.resetForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.resetPassword({
      email: this.verifiedEmail,
      newPassword: this.resetForm.value.newPassword!,
      newPasswordConfirm: this.resetForm.value.newPasswordConfirm!
    }).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = result.message ?? 'Parola güncellenemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
