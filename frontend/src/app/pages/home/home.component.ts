import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { UserService } from '../../core/services/user.service';
import { GoalStatusDto } from '../../core/models/goal-status.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';

@Component({
  selector: 'app-home',
  imports: [NavbarComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private goalStatusService = inject(GoalStatusService);
  private suggestionService = inject(SuggestionService);
  private userService = inject(UserService);
  private router = inject(Router);

  last7Days: GoalStatusDto[] = [];
  suggestionText = '';

  // Google ile oluşturulup henüz şifre belirlememiş hesaplar için bildirim.
  showPasswordBanner = false;

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

  // Şifre belirleme formu artık burada değil, Profil sayfasında — kullanıcı oradayken
  // diğer bilgilerini de görüp güncelleyebilsin diye.
  goToSetPassword(): void {
    this.router.navigate(['/profile'], { queryParams: { openPassword: '1' } });
  }
}
