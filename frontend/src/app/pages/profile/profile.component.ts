import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { UserService } from '../../core/services/user.service';
import { UpdateProfileDto } from '../../core/models/user.models';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    fullName: ['', Validators.required],
    birthDate: ['', Validators.required],
    newPassword: [''],
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
