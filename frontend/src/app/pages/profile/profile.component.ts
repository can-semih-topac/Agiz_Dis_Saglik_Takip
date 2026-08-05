import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { UserService } from '../../core/services/user.service';
import { UpdateProfileDto } from '../../core/models/user.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, RouterLink, NavbarComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);

  constructor(title: Title) {
    title.setTitle('Profil | ADS');
  }

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  maxDate = new Date().toISOString().split('T')[0]; // takvimde gelecek tarih seçilemesin

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    fullName: ['', Validators.required],
    birthDate: ['', Validators.required],
    // Boş bırakılabilir (parola değişmesin diye) — Angular'ın minLength/pattern validator'ları
    // boş değeri zaten geçerli sayıyor, sadece dolu girilirse kurala bakıyor.
    newPassword: ['', [Validators.minLength(8), Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/)]],
    newPasswordConfirm: ['']
  });

  ngOnInit(): void {
    this.userService.getProfile().subscribe({
      next: (result) => {
        if (result.success) {
          this.form.patchValue({
            email: result.data.email,
            fullName: result.data.fullName,
            birthDate: result.data.birthDate
          });
        }
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto: UpdateProfileDto = {
      email: this.form.value.email!,
      fullName: this.form.value.fullName!,
      birthDate: this.form.value.birthDate!,
      newPassword: this.form.value.newPassword || null,
      newPasswordConfirm: this.form.value.newPasswordConfirm || null
    };

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.userService.updateProfile(dto).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.successMessage = result.message ?? 'Profil güncellendi.';
          this.form.patchValue({ newPassword: '', newPasswordConfirm: '' });
        } else {
          this.errorMessage = result.message ?? 'Güncelleme başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
