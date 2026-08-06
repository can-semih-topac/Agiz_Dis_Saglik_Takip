import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
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

  constructor(title: Title) {
    title.setTitle('Kayıt Ol | ADS');
  }

  isSubmitting = false;
  submitted = false; // hata mesajları alana dokununca değil, "Kayıt Ol"a basılınca gösterilsin diye
  errorMessage = '';
  successMessage = '';
  maxDate = new Date().toISOString().split('T')[0]; // takvimde gelecek tarih seçilemesin

  // Parola kuralı (min 8 karakter + büyük/küçük harf + rakam) backend'deki
  // AuthBusinessRules.IsValidPassword ile aynı — kullanıcı sunucuya sormadan anında uyarı görsün diye.
  form = this.fb.group({
    fullName: ['', Validators.required],
    birthDate: ['', Validators.required],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10,11}$/)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/)]],
    passwordConfirm: ['', Validators.required]
  });

  // Kullanıcı "90532...", "532..." ya da "0532..." yazsın — alandan çıkınca hepsini
  // "0532..." (11 haneli, başında 0) formatına çevirip hem gösteriyor hem de bu haliyle gönderiyoruz.
  normalizePhoneNumber(): void {
    let digits = (this.form.value.phoneNumber ?? '').replace(/\D/g, '');

    if (digits.length === 12 && digits.startsWith('90')) {
      digits = digits.slice(2);
    }
    if (digits.length === 10 && digits.startsWith('5')) {
      digits = '0' + digits;
    }

    this.form.patchValue({ phoneNumber: digits });
  }

  submit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const dto: RegisterDto = {
      email: this.form.value.email!,
      password: this.form.value.password!,
      passwordConfirm: this.form.value.passwordConfirm!,
      fullName: this.form.value.fullName!,
      birthDate: this.form.value.birthDate!,
      phoneNumber: this.form.value.phoneNumber!
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
          this.submitted = false;
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
