import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { UserService } from '../../core/services/user.service';
import { GoalStatusDto } from '../../core/models/goal-status.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';

@Component({
  selector: 'app-home',
  imports: [NavbarComponent, ReactiveFormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private goalStatusService = inject(GoalStatusService);
  private suggestionService = inject(SuggestionService);
  private userService = inject(UserService);
  private fb = inject(FormBuilder);

  last7Days: GoalStatusDto[] = [];
  suggestionText = '';

  // Google ile oluşturulup henüz parola belirlememiş hesaplar için bildirim.
  showPasswordBanner = false;
  showPasswordForm = false;
  passwordSubmitting = false;
  passwordSubmitted = false;
  passwordErrorMessage = '';
  passwordSuccessMessage = '';

  passwordForm = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/)]],
    newPasswordConfirm: ['', Validators.required]
  });

  constructor(title: Title) {
    title.setTitle('Ana Sayfa | ADS');
  }

  ngOnInit(): void {
    this.loadLast7Days();
    this.loadSuggestion();
    this.checkPasswordStatus();
  }

  loadLast7Days(): void {
    this.goalStatusService.getLast7Days().subscribe({
      next: (result) => {
        if (result.success) {
          this.last7Days = result.data;
        }
      },
      error: (err) => console.error('Son 7 gün verisi alınamadı', err)
    });
  }

  loadSuggestion(): void {
    this.suggestionService.getRandom().subscribe({
      next: (result) => {
        if (result.success) {
          this.suggestionText = result.data.text;
        }
      }
    });
  }

  checkPasswordStatus(): void {
    this.userService.getProfile().subscribe({
      next: (result) => {
        if (result.success) {
          this.showPasswordBanner = !result.data.hasPassword;
        }
      }
    });
  }

  openPasswordForm(): void {
    this.showPasswordForm = true;
  }

  submitPassword(): void {
    this.passwordSubmitted = true;
    this.passwordErrorMessage = '';

    if (this.passwordForm.invalid) {
      return;
    }

    this.passwordSubmitting = true;

    // Henüz parolası olmayan hesap için "eski parola" diye bir şey yok — boş gönderiyoruz,
    // backend PasswordEncrypted boşsa zaten eski parola kontrolü yapmıyor.
    this.userService.changePassword({
      oldPassword: '',
      newPassword: this.passwordForm.value.newPassword!,
      newPasswordConfirm: this.passwordForm.value.newPasswordConfirm!
    }).subscribe({
      next: (result) => {
        this.passwordSubmitting = false;
        if (result.success) {
          this.passwordSuccessMessage = result.message ?? 'Parola belirlendi.';
          this.showPasswordBanner = false;
          this.showPasswordForm = false;
        } else {
          this.passwordErrorMessage = result.message ?? 'Parola belirlenemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.passwordSubmitting = false;
        this.passwordErrorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
