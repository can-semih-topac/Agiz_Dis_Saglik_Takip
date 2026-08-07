import { AfterViewInit, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { LoginDto } from '../../core/models/auth.models';
import { environment } from '../../../environments/environment';
import { AuthNavbarComponent } from '../../shared/auth-navbar/auth-navbar.component';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, AuthNavbarComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements AfterViewInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  constructor(title: Title) {
    title.setTitle('Giriş Yap | ADS');
  }

  isSubmitting = false;
  submitted = false;
  errorMessage = '';
  successMessage = '';

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  submit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const dto: LoginDto = {
      email: this.form.value.email!,
      password: this.form.value.password!
    };

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.login(dto).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success && result.data) {
          this.authService.saveSession(result.data);
          this.router.navigate(['/home']);
        } else {
          this.errorMessage = result.message ?? 'Giriş başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  ngAfterViewInit(): void {
    this.renderGoogleButton();
  }

  // https://accounts.google.com/gsi/client script'i "async" yüklendiği için sayfa
  // hazır olduğunda henüz gelmemiş olabilir — gelene kadar kısa aralıklarla tekrar deniyoruz.
  private renderGoogleButton(): void {
    const google = (window as any).google;
    if (!google?.accounts?.id) {
      setTimeout(() => this.renderGoogleButton(), 100);
      return;
    }

    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (response: { credential: string }) => this.handleGoogleCredential(response.credential)
    });

    google.accounts.id.renderButton(
      document.getElementById('google-signin-button'),
      { theme: 'outline', size: 'large', text: 'continue_with', width: 320 }
    );
  }

  private handleGoogleCredential(idToken: string): void {
    this.errorMessage = '';

    this.authService.googleLogin({ idToken }).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.authService.saveSession(result.data);
          this.router.navigate(['/home']);
        } else {
          this.errorMessage = result.message ?? 'Google ile giriş başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
