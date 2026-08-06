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

  showCountryInfo = false; // bayrak rozetine tıklanınca "sadece Türkiye" notu
  phoneInvalidCharWarning = false; // rakam dışı bir şey yazılmaya çalışılınca turuncu uyarı

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
  // "0532..." formatına çeviriyoruz. Uzunluğa değil, sadece öneke (prefix) bakıyoruz —
  // böylece "90532" ya da "546" gibi eksik/kısa girişlerde de doğru çalışır.
  normalizePhoneNumber(): void {
    let digits = (this.form.value.phoneNumber ?? '').replace(/\D/g, '');

    if (digits.startsWith('90')) {
      digits = digits.slice(2);
    }
    if (digits.length > 0 && !digits.startsWith('0')) {
      digits = '0' + digits;
    }

    this.form.patchValue({ phoneNumber: digits });
  }

  // Rakam dışı bir karakter (harf, nokta vb.) yazılırsa anında temizler ve uyarı gösterir.
  // Ayrıca başlangıç önekine göre izin verilen azami uzunluğu da burada uyguluyoruz:
  // "5" ile başlıyorsa 10, "0" ile başlıyorsa 11, "90" ile başlıyorsa 12 hane.
  onPhoneInput(): void {
    const raw = this.form.value.phoneNumber ?? '';
    let digitsOnly = raw.replace(/[^0-9]/g, '');

    this.phoneInvalidCharWarning = raw !== digitsOnly;

    let maxLen = 11;
    if (digitsOnly.startsWith('90')) {
      maxLen = 12;
    } else if (digitsOnly.startsWith('0')) {
      maxLen = 11;
    } else if (digitsOnly.startsWith('5')) {
      maxLen = 10;
    }

    if (digitsOnly.length > maxLen) {
      digitsOnly = digitsOnly.slice(0, maxLen);
    }

    if (raw !== digitsOnly) {
      this.form.patchValue({ phoneNumber: digitsOnly });
    }
  }

  private countryInfoTimeout?: ReturnType<typeof setTimeout>;

  // Rozete tıklanınca bildirim gibi 3 saniyeliğine görünüp kendiliğinden kapanır.
  toggleCountryInfo(): void {
    this.showCountryInfo = true;
    if (this.countryInfoTimeout) {
      clearTimeout(this.countryInfoTimeout);
    }
    this.countryInfoTimeout = setTimeout(() => {
      this.showCountryInfo = false;
    }, 3000);
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
