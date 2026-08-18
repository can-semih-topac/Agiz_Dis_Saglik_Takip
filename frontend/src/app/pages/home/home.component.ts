import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { GoalService } from '../../core/services/goal.service';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { StatusNoteService } from '../../core/services/status-note.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { UserService } from '../../core/services/user.service';
import { WillpowerService } from '../../core/services/willpower.service';
import { GoalStatusDto, LongestStreakDto } from '../../core/models/goal-status.models';
import { WillpowerTierDto } from '../../core/models/willpower.models';
import { StatusNoteDto } from '../../core/models/status-note.models';
import { GoalDto, PeriodUnit, TrackingType } from '../../core/models/goal.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { StatusDetailModalComponent } from '../../shared/status-detail-modal/status-detail-modal.component';
import { CalendarViewComponent } from '../../shared/calendar-view/calendar-view.component';
import { WillpowerHistoryModalComponent } from '../../shared/willpower-history-modal/willpower-history-modal.component';
import { formatTurkishDate, formatTurkishDateTime } from '../../shared/turkish-date';

@Component({
  selector: 'app-home',
  imports: [FormsModule, NavbarComponent, StatusDetailModalComponent, CalendarViewComponent, WillpowerHistoryModalComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private goalService = inject(GoalService);
  private goalStatusService = inject(GoalStatusService);
  private statusNoteService = inject(StatusNoteService);
  private suggestionService = inject(SuggestionService);
  private userService = inject(UserService);
  private willpowerService = inject(WillpowerService);
  private router = inject(Router);

  // Template'te enum karşılaştırması yapabilmek için.
  readonly PeriodUnit = PeriodUnit;
  readonly TrackingType = TrackingType;
  readonly formatTurkishDateTime = formatTurkishDateTime;

  today = '';

  goals: GoalDto[] = [];
  allStatus: GoalStatusDto[] = [];
  notesByStatusId = new Map<number, StatusNoteDto>();
  longestStreaks: LongestStreakDto[] = [];
  willpowerScore = 0;
  willpowerLabel = '';
  willpowerTiers: WillpowerTierDto[] = [];
  suggestionText = '';

  selectedCalendarDate: string | null = null;
  selectedStatus: GoalStatusDto | null = null;
  showWillpowerHistory = false;

  // Hızlı ekleme (Bugün bölümü) — süreli hedeflerde tıklayınca küçük bir süre kutusu açılır.
  expandedQuickGoalId: number | null = null;
  quickDuration: number | null = null;
  quickAddError = '';

  // Google ile oluşturulup henüz şifre belirlememiş hesaplar için bildirim.
  showPasswordBanner = false;
  // Admin panelinden geçici şifreyle oluşturulan hesaplar için — şifre değiştirilene kadar gösterilir.
  showTempPasswordBanner = false;

  // "Bugün Yapılanlar" -> "Yapılması Gerekenler" sürükle-bırak (yanlışlıkla işaretlenen kaydı geri alma).
  draggingStatusId: number | null = null;
  isTodoDropTarget = false;

  constructor(title: Title) {
    title.setTitle('Ana Sayfa | ADS');
  }

  ngOnInit(): void {
    this.today = this.computeTodayStr();
    this.selectedCalendarDate = this.today;

    this.loadGoals();
    this.loadStatusData();
    this.loadLongestStreaks();
    this.loadWillpowerScore();
    this.loadSuggestion();
    this.checkPasswordStatus();
  }

  // ---- Bugün bölümü ----

  get todaysStatus(): GoalStatusDto[] {
    return this.allStatus
      .filter(s => s.activityDate === this.today)
      .sort((a, b) => b.activityTime.localeCompare(a.activityTime));
  }

  get todoTodayList(): { goal: GoalDto; doneCount: number }[] {
    return this.goals
      .filter(g => g.periodUnit === PeriodUnit.Gun)
      .map(goal => ({
        goal,
        doneCount: this.todaysStatus.filter(s => s.goalId === goal.id).length
      }))
      .filter(x => x.doneCount < x.goal.periodFrequency);
  }

  quickAddYapildi(goal: GoalDto): void {
    this.submitQuickAdd(goal, null);
  }

  startQuickAddSureli(goal: GoalDto): void {
    this.expandedQuickGoalId = goal.id;
    this.quickDuration = null;
    this.quickAddError = '';
  }

  cancelQuickAdd(): void {
    this.expandedQuickGoalId = null;
    this.quickAddError = '';
  }

  confirmQuickAddSureli(goal: GoalDto): void {
    if (this.quickDuration == null || this.quickDuration < 0) {
      this.quickAddError = 'Geçerli bir süre gir.';
      return;
    }
    this.submitQuickAdd(goal, this.quickDuration);
  }

  private submitQuickAdd(goal: GoalDto, durationMinutes: number | null): void {
    const now = new Date();
    const activityTime = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:00`;

    this.goalStatusService.createGoalStatus({
      goalId: goal.id,
      activityDate: this.today,
      activityTime,
      durationMinutes
    }).subscribe({
      next: (result) => {
        if (result.success) {
          this.expandedQuickGoalId = null;
          this.quickAddError = '';
          this.loadStatusData();
          this.loadLongestStreaks();
          this.loadWillpowerScore();
        } else {
          this.quickAddError = result.message ?? 'Kayıt eklenemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.quickAddError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  // ---- Sürükle-bırak: yanlışlıkla işaretlenen kaydı geri alma ----

  onStatusDragStart(event: DragEvent, status: GoalStatusDto): void {
    this.draggingStatusId = status.id;
    event.dataTransfer?.setData('text/plain', String(status.id));
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move';
  }

  onStatusDragEnd(): void {
    this.draggingStatusId = null;
    this.isTodoDropTarget = false;
  }

  onTodoDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isTodoDropTarget = true;
  }

  onTodoDragLeave(): void {
    this.isTodoDropTarget = false;
  }

  onTodoDrop(event: DragEvent): void {
    event.preventDefault();
    this.isTodoDropTarget = false;

    const statusId = this.draggingStatusId;
    this.draggingStatusId = null;
    if (statusId == null) return;

    this.goalStatusService.delete(statusId).subscribe({
      next: (result) => {
        if (result.success) {
          this.loadStatusData();
          this.loadLongestStreaks();
          this.loadWillpowerScore();
        }
      }
    });
  }

  // ---- Takvim ----

  recordDates: Set<string> = new Set();

  get selectedDateRecords(): GoalStatusDto[] {
    if (!this.selectedCalendarDate) return [];
    return this.allStatus
      .filter(s => s.activityDate === this.selectedCalendarDate)
      .sort((a, b) => b.activityTime.localeCompare(a.activityTime));
  }

  onCalendarDateSelected(date: string): void {
    this.selectedCalendarDate = date;
  }

  get selectedDateLabel(): string {
    return this.selectedCalendarDate ? formatTurkishDate(this.selectedCalendarDate) : '';
  }

  // ---- Kayıt detayı (hem Bugün hem Takvim listelerinde ortak) ----

  noteFor(status: GoalStatusDto): StatusNoteDto | null {
    return this.notesByStatusId.get(status.id) ?? null;
  }

  openDetail(status: GoalStatusDto): void {
    this.selectedStatus = status;
  }

  closeDetail(): void {
    this.selectedStatus = null;
  }

  // Detay penceresinden bir kayıt düzenlenince tarih/süre/seri/puan hepsi değişmiş olabilir.
  onDetailSaved(): void {
    this.selectedStatus = null;
    this.loadStatusData();
    this.loadLongestStreaks();
    this.loadWillpowerScore();
  }

  openWillpowerHistory(): void {
    this.showWillpowerHistory = true;
  }

  closeWillpowerHistory(): void {
    this.showWillpowerHistory = false;
  }

  // ---- Veri yükleme ----

  loadGoals(): void {
    this.goalService.getGoals().subscribe({
      next: (result) => { if (result.success) this.goals = result.data; }
    });
  }

  loadStatusData(): void {
    this.goalStatusService.getAll().subscribe({
      next: (result) => {
        if (result.success) {
          this.allStatus = result.data;
          this.recordDates = new Set(this.allStatus.map(s => s.activityDate));
        }
      },
      error: (err) => console.error('Durum verileri alınamadı', err)
    });
    this.statusNoteService.getAll().subscribe({
      next: (result) => {
        if (result.success) {
          this.notesByStatusId.clear();
          for (const note of result.data) {
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
      next: (result) => { if (result.success) this.longestStreaks = result.data; }
    });
  }

  loadWillpowerScore(): void {
    this.willpowerService.getScore().subscribe({
      next: (result) => {
        if (result.success) {
          this.willpowerScore = result.data.score;
          this.willpowerLabel = result.data.label;
          this.willpowerTiers = result.data.tiers;
        }
      }
    });
  }

  loadSuggestion(): void {
    this.suggestionService.getRandom().subscribe({
      next: (result) => { if (result.success) this.suggestionText = result.data.text; }
    });
  }

  checkPasswordStatus(): void {
    this.userService.getProfile().subscribe({
      next: (result) => {
        if (result.success) {
          this.showPasswordBanner = !result.data.hasPassword;
          this.showTempPasswordBanner = result.data.hasPassword && result.data.mustChangePassword;
        }
      }
    });
  }

  // Şifre belirleme formu artık burada değil, Profil sayfasında — kullanıcı oradayken
  // diğer bilgilerini de görüp güncelleyebilsin diye.
  goToSetPassword(): void {
    this.router.navigate(['/profile'], { queryParams: { openPassword: '1' } });
  }

  private computeTodayStr(): string {
    const d = new Date();
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
