import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { StatusNoteService } from '../../core/services/status-note.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { UserService } from '../../core/services/user.service';
import { GoalStatusDto, LongestStreakDto } from '../../core/models/goal-status.models';
import { StatusNoteDto } from '../../core/models/status-note.models';
import { TrackingType } from '../../core/models/goal.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { StatusDetailModalComponent } from '../../shared/status-detail-modal/status-detail-modal.component';
import { formatTurkishDateTime } from '../../shared/turkish-date';

@Component({
  selector: 'app-home',
  imports: [NavbarComponent, StatusDetailModalComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private goalStatusService = inject(GoalStatusService);
  private statusNoteService = inject(StatusNoteService);
  private suggestionService = inject(SuggestionService);
  private userService = inject(UserService);
  private router = inject(Router);

  // Template'te enum karşılaştırması yapabilmek için.
  readonly TrackingType = TrackingType;
  readonly formatTurkishDateTime = formatTurkishDateTime;

  last7Days: GoalStatusDto[] = [];
  last7DaysNotes: StatusNoteDto[] = [];
  notesByStatusId = new Map<number, StatusNoteDto>();
  longestStreaks: LongestStreakDto[] = [];
  suggestionText = '';

  selectedStatus: GoalStatusDto | null = null;

  // Google ile oluşturulup henüz şifre belirlememiş hesaplar için bildirim.
  showPasswordBanner = false;

  constructor(title: Title) {
    title.setTitle('Ana Sayfa | ADS');
  }

  ngOnInit(): void {
    this.loadLast7Days();
    this.loadLongestStreaks();
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
    this.statusNoteService.getLast7Days().subscribe({
      next: (result) => {
        if (result.success) {
          this.last7DaysNotes = result.data;
          this.notesByStatusId.clear();
          for (const note of this.last7DaysNotes) {
            if (note.goalStatusId != null) {
              this.notesByStatusId.set(note.goalStatusId, note);
            }
          }
        }
      }
    });
  }

  loadLongestStreaks(): void {
    this.goalStatusService.getLongestStreaks().subscribe({
      next: (result) => {
        if (result.success) {
          this.longestStreaks = result.data;
        }
      }
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

  noteFor(status: GoalStatusDto): StatusNoteDto | null {
    return this.notesByStatusId.get(status.id) ?? null;
  }

  openDetail(status: GoalStatusDto): void {
    this.selectedStatus = status;
  }

  closeDetail(): void {
    this.selectedStatus = null;
  }

  // Şifre belirleme formu artık burada değil, Profil sayfasında — kullanıcı oradayken
  // diğer bilgilerini de görüp güncelleyebilsin diye.
  goToSetPassword(): void {
    this.router.navigate(['/profile'], { queryParams: { openPassword: '1' } });
  }
}
