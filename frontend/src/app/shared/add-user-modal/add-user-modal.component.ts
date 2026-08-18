import { Component, EventEmitter, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { UserService } from '../../core/services/user.service';
import { Role } from '../../core/models/user.models';

@Component({
  selector: 'app-add-user-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './add-user-modal.component.html',
  styleUrl: './add-user-modal.component.css'
})
export class AddUserModalComponent {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);

  @Output() closed = new EventEmitter<void>();
  // Ekleme başarılı olunca admin sayfasındaki listeyi yeniden yüklemesi için.
  @Output() created = new EventEmitter<void>();

  readonly Role = Role;

  isSubmitting = false;
  submitted = false;
  errorMessage = '';
  successMessage = '';

  // AuthBusinessRules.IsValidPassword ile aynı — kullanıcı sunucuya sormadan anında uyarı görsün diye (register formundakiyle aynı desen).
  private static readonly passwordComplexityPattern = /(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/;

  form = this.fb.group({
    role: [Role.User, Validators.required],
    email: ['', [Validators.required, Validators.email]],
    // Rol Admin iken zorunlu, User iken opsiyonel — girilirse yine de geçerli bir şifre olmalı
    // (bkz. onRoleChange). Boş bırakılırsa hesap şifresiz oluşturulup davet e-postası gönderilir.
    temporaryPassword: ['', [Validators.minLength(8), Validators.pattern(AddUserModalComponent.passwordComplexityPattern)]]
  });

  get isAdminRole(): boolean {
    return this.form.value.role === Role.Admin;
  }

  onRoleChange(): void {
    const control = this.form.controls.temporaryPassword;
    const baseValidators = [Validators.minLength(8), Validators.pattern(AddUserModalComponent.passwordComplexityPattern)];
    control.setValidators(this.isAdminRole ? [Validators.required, ...baseValidators] : baseValidators);
    control.updateValueAndValidity();
  }

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.form.invalid) {
      return;
    }

    this.isSubmitting = true;

    this.userService.createUser({
      email: this.form.value.email!,
      role: this.form.value.role!,
      temporaryPassword: this.form.value.temporaryPassword || null
    }).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.successMessage = result.message ?? 'Kullanıcı oluşturuldu.';
          this.created.emit();
        } else {
          this.errorMessage = result.message ?? 'Kullanıcı oluşturulamadı.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
