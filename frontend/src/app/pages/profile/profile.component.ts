import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { ChangePasswordDto, UpdateProfileDto } from '../../core/models/user.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';

type ProfileField = 'fullName' | 'birthDate' | 'phoneNumber' | 'email';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, RouterLink, NavbarComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  constructor(title: Title) {
    title.setTitle('Profil | ADS');
  }

  maxDate = new Date().toISOString().split('T')[0]; // takvimde gelecek tarih seçilemesin

  // Son kaydedilen değerler — bir alanı "Vazgeç" ile iptal edince buraya geri dönüyoruz.
  originalProfile: UpdateProfileDto = { fullName: '', birthDate: '', phoneNumber: '', email: '' };

  editingField: ProfileField | null = null;
  fieldSubmitting = false;
  fieldErrorMessage = '';
  phoneInvalidCharWarning = false;

  // Google ile oluşturulup henüz şifre belirlememiş hesaplarda "Mevcut Şifre" alanı gizlenir.
  hasPassword = true;
  showPasswordSection = false;
  passwordSubmitting = false;
  passwordSubmitted = false;
  passwordErrorMessage = '';
  passwordSuccessMessage = '';
  showOldPassword = false;
  showNewPassword = false;
  showNewPasswordConfirm = false;

  pendingDeleteAccount = false;
  deleteSubmitting = false;
  deleteErrorMessage = '';

  fieldForm = this.fb.group({
    fullName: ['', Validators.required],
    birthDate: ['', Validators.required],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10,11}$/)]],
    email: ['', [Validators.required, Validators.email]]
  });

  passwordForm = this.fb.group({
    oldPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/)]],
    newPasswordConfirm: ['', Validators.required]
  });

  ngOnInit(): void {
    // Ana sayfadaki "Şifre Belirle" bildirimi buraya bu query param ile yönlendiriyor —
    // kullanıcı sayfaya gelir gelmez şifre bölümü açık gelsin diye.
    if (this.route.snapshot.queryParamMap.get('openPassword')) {
      this.showPasswordSection = true;
    }

    this.userService.getProfile().subscribe({
      next: (result) => {
        if (result.success) {
          this.originalProfile = {
            fullName: result.data.fullName,
            birthDate: result.data.birthDate,
            phoneNumber: result.data.phoneNumber,
            email: result.data.email
          };
          this.fieldForm.patchValue(this.originalProfile);
          this.hasPassword = result.data.hasPassword;
          this.updateOldPasswordValidator();
        }
      }
    });
  }

  private updateOldPasswordValidator(): void {
    const control = this.passwordForm.get('oldPassword')!;
    if (this.hasPassword) {
      control.setValidators(Validators.required);
    } else {
      control.clearValidators();
    }
    control.updateValueAndValidity();
  }

  startEdit(field: ProfileField): void {
    this.fieldErrorMessage = '';
    this.editingField = field;
  }

  cancelEdit(field: ProfileField): void {
    this.fieldForm.patchValue({ [field]: this.originalProfile[field] });
    this.fieldErrorMessage = '';
    this.editingField = null;
  }

  saveField(field: ProfileField): void {
    const control = this.fieldForm.get(field)!;
    control.markAsTouched();
    if (control.invalid) {
      return;
    }

    const dto: UpdateProfileDto = {
      fullName: this.fieldForm.value.fullName!,
      birthDate: this.fieldForm.value.birthDate!,
      phoneNumber: this.fieldForm.value.phoneNumber!,
      email: this.fieldForm.value.email!
    };

    this.fieldSubmitting = true;
    this.fieldErrorMessage = '';

    this.userService.updateProfile(dto).subscribe({
      next: (result) => {
        this.fieldSubmitting = false;
        if (result.success) {
          this.originalProfile = dto;
          this.editingField = null;
        } else {
          this.fieldErrorMessage = result.message ?? 'Güncelleme başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.fieldSubmitting = false;
        this.fieldErrorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  // Kayıt formundakiyle aynı mantık: uzunluğa değil öneke (5/0/90) bakarak
  // dinamik azami hane sayısı uyguluyor ve rakam dışı karakterleri anında temizliyor.
  onPhoneInput(): void {
    const raw = this.fieldForm.value.phoneNumber ?? '';
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
      this.fieldForm.patchValue({ phoneNumber: digitsOnly });
    }
  }

  normalizePhoneNumber(): void {
    let digits = (this.fieldForm.value.phoneNumber ?? '').replace(/\D/g, '');

    if (digits.startsWith('90')) {
      digits = digits.slice(2);
    }
    if (digits.length > 0 && !digits.startsWith('0')) {
      digits = '0' + digits;
    }

    this.fieldForm.patchValue({ phoneNumber: digits });
  }

  togglePasswordSection(): void {
    this.showPasswordSection = !this.showPasswordSection;
    this.passwordErrorMessage = '';
    this.passwordSuccessMessage = '';
    this.passwordSubmitted = false;
    if (!this.showPasswordSection) {
      this.passwordForm.reset();
    }
  }

  submitPasswordChange(): void {
    this.passwordSubmitted = true;
    this.passwordErrorMessage = '';
    this.passwordSuccessMessage = '';

    if (this.passwordForm.invalid) {
      return;
    }

    const dto: ChangePasswordDto = {
      oldPassword: this.passwordForm.value.oldPassword!,
      newPassword: this.passwordForm.value.newPassword!,
      newPasswordConfirm: this.passwordForm.value.newPasswordConfirm!
    };

    this.passwordSubmitting = true;

    this.userService.changePassword(dto).subscribe({
      next: (result) => {
        this.passwordSubmitting = false;
        if (result.success) {
          this.passwordSuccessMessage = result.message ?? 'Şifre güncellendi.';
          this.passwordForm.reset();
          this.passwordSubmitted = false;
        } else {
          this.passwordErrorMessage = result.message ?? 'Şifre güncellenemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.passwordSubmitting = false;
        this.passwordErrorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  requestDeleteAccount(): void {
    this.deleteErrorMessage = '';
    this.pendingDeleteAccount = true;
  }

  cancelDeleteAccount(): void {
    this.pendingDeleteAccount = false;
  }

  confirmDeleteAccount(): void {
    this.deleteSubmitting = true;
    this.deleteErrorMessage = '';

    this.userService.deleteAccount().subscribe({
      next: (result) => {
        if (result.success) {
          this.authService.logout();
          this.router.navigate(['/login']);
        } else {
          this.deleteSubmitting = false;
          this.deleteErrorMessage = result.message ?? 'Hesap silinemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.deleteSubmitting = false;
        this.deleteErrorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
