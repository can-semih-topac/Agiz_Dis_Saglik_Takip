import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { RegisterDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent { // kayıt formu ve submit işlemleri için component
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  maxDate = new Date().toISOString().split('T')[0]; // takvimde gelecek tarih seçilemesin

  // Parola kuralı (min 8 karakter + büyük/küçük harf + rakam) backend'deki
  // AuthBusinessRules.IsValidPassword ile aynı — kullanıcı sunucuya sormadan anında uyarı görsün diye.
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/)]],
    passwordConfirm: ['', Validators.required],
    fullName: ['', Validators.required],
    birthDate: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto: RegisterDto = {
      email: this.form.value.email!,
      password: this.form.value.password!,
      passwordConfirm: this.form.value.passwordConfirm!,
      fullName: this.form.value.fullName!,
      birthDate: this.form.value.birthDate!
    };

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register(dto).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.successMessage = result.message ?? 'Kayıt başarılı.';
          this.form.reset();
        } else {
          this.errorMessage = result.message ?? 'Kayıt başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        // Backend, iş kuralı ihlallerinde (400 Bad Request) ServiceResult gövdesiyle cevap veriyor.
        // HttpClient bunu "next" değil "error" sayıyor ama gövdede backend'in gerçek mesajı var —
        // status 0 ise gerçekten sunucuya hiç ulaşılamamış demektir, o zaman gövde de yok.
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
