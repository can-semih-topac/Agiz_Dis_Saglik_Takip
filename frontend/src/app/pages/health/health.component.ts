import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';
import { GoalService } from '../../core/services/goal.service';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { StatusNoteService } from '../../core/services/status-note.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { GoalDto, PeriodUnit, Importance, TrackingType, CreateGoalDto } from '../../core/models/goal.models';
import { GoalStatusDto, CreateGoalStatusDto } from '../../core/models/goal-status.models';
import { StatusNoteDto } from '../../core/models/status-note.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { StatusDetailModalComponent } from '../../shared/status-detail-modal/status-detail-modal.component';
import { formatTurkishDateTime } from '../../shared/turkish-date';
import { Title } from '@angular/platform-browser';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-health',
  imports: [ReactiveFormsModule, FormsModule, RouterLink, NavbarComponent, StatusDetailModalComponent, TranslocoPipe],
  templateUrl: './health.component.html',
  styleUrl: './health.component.css'
})
export class HealthComponent implements OnInit {
  private fb = inject(FormBuilder);
  private goalService = inject(GoalService);
  private goalStatusService = inject(GoalStatusService);
  private statusNoteService = inject(StatusNoteService);
  private suggestionService = inject(SuggestionService);

  constructor(title: Title) {
    title.setTitle('Alışkanlık Yönetimi | ADS');
  }

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  // Not: apiBaseUrl'nin SONUNDAKİ "/api"yi kesiyoruz — düz .replace('/api','') canlıda
  // "api.cansemihtopac.com" gibi "api" ile başlayan alt alan adlarında baştaki "/api"yi
  // yanlışlıkla siliyor ve adresi bozuyordu (regex'teki $ ifadesi sonu sabitliyor).
  apiOrigin = environment.apiBaseUrl.replace(/\/api$/, '');

  // Template'te enum karşılaştırması yapabilmek için.
  readonly TrackingType = TrackingType;
  readonly formatTurkishDateTime = formatTurkishDateTime;

  activeTab: 'durum' | 'hedef' = 'durum';

  goals: GoalDto[] = [];
  last7DaysStatus: GoalStatusDto[] = [];
  last7DaysNotes: StatusNoteDto[] = [];
  notesByStatusId = new Map<number, StatusNoteDto>();
  selectedStatus: GoalStatusDto | null = null;
  suggestionText = '';
  selectedImage: File | null = null;

  goalError = '';
  goalSuccess = '';
  statusError = '';
  statusSuccess = '';

  // Tarayıcının çirkin confirm() penceresi yerine kendi modalımızı gösteriyoruz.
  pendingDeleteGoal: GoalDto | null = null;
  confirmMessage = '';

  pendingPauseGoal: GoalDto | null = null;
  pauseReasonInput = '';
  pauseError = '';
  pauseSubmitting = false;
  resumingGoalId: number | null = null;

  // label burada çeviri anahtarı olarak tutuluyor, gerçek metin şablonda transloco pipe'ıyla çözülüyor.
  periodUnits = [
    { value: PeriodUnit.Gun, label: 'common.day' },
    { value: PeriodUnit.Hafta, label: 'common.week' },
    { value: PeriodUnit.Ay, label: 'common.month' }
  ];

  importanceLevels = [
    { value: Importance.Dusuk, label: 'health.importanceLow' },
    { value: Importance.Orta, label: 'health.importanceMedium' },
    { value: Importance.Yuksek, label: 'health.importanceHigh' }
  ];

  trackingTypes = [
    { value: TrackingType.Sureli, label: 'health.trackingSureliOption' },
    { value: TrackingType.Yapildi, label: 'health.trackingYapildiOption' }
  ];

  goalForm = this.fb.group({
    title: ['', Validators.required],
    description: ['', Validators.required],
    periodUnit: [PeriodUnit.Gun, Validators.required],
    periodFrequency: [1, [Validators.required, Validators.min(1)]],
    importance: [Importance.Orta, Validators.required],
    trackingType: [TrackingType.Sureli, Validators.required]
  });

  // Durum kaydı + not tek formda birleşti: goal/tarih/saat zorunlu, süre ise
  // seçilen hedefin takip türüne göre zorunlu ya da hiç gösterilmiyor —
  // açıklama+görsel opsiyonel, doluysa not olarak da ayrıca kaydedilecek.
  statusForm = this.fb.group({
    goalId: [null as number | null, Validators.required],
    activityDate: [this.currentDateStr(), Validators.required],
    activityTime: [this.currentTimeStr(), Validators.required],
    durationMinutes: [0 as number | null, [Validators.min(0)]],
    noteDescription: ['']
  });

  get selectedGoalForStatus(): GoalDto | undefined {
    return this.goals.find(g => g.id === this.statusForm.value.goalId);
  }

  // Hedef seçimi değişince süre alanının zorunlu olup olmayacağını (ve görünürlüğünü) günceller.
  onStatusGoalChange(): void {
    const durationControl = this.statusForm.get('durationMinutes')!;

    if (this.selectedGoalForStatus?.trackingType === TrackingType.Sureli) {
      durationControl.setValidators([Validators.required, Validators.min(0)]);
    } else {
      durationControl.clearValidators();
      durationControl.setValue(null);
    }
    durationControl.updateValueAndValidity();
  }

  ngOnInit(): void {
    this.loadGoals();
    this.loadLast7Days();
    this.loadSuggestion();
  }

  setTab(tab: 'durum' | 'hedef'): void {
    this.activeTab = tab;
  }

  loadGoals(): void {
    this.goalService.getGoals().subscribe({
      next: (result) => { if (result.success) this.goals = result.data; }
    });
  }

  loadLast7Days(): void {
    this.goalStatusService.getLast7Days().subscribe({
      next: (result) => { if (result.success) this.last7DaysStatus = result.data; }
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

  noteFor(status: GoalStatusDto): StatusNoteDto | null {
    return this.notesByStatusId.get(status.id) ?? null;
  }

  openDetail(status: GoalStatusDto): void {
    this.selectedStatus = status;
  }

  closeDetail(): void {
    this.selectedStatus = null;
  }

  onDetailSaved(): void {
    this.selectedStatus = null;
    this.loadLast7Days();
  }

  loadSuggestion(): void {
    this.suggestionService.getRandom().subscribe({
      next: (result) => { if (result.success) this.suggestionText = result.data.text; }
    });
  }

  createGoal(): void {
    if (this.goalForm.invalid) {
      this.goalForm.markAllAsTouched();
      return;
    }
    this.goalError = '';
    this.goalSuccess = '';

    const dto: CreateGoalDto = {
      title: this.goalForm.value.title!,
      description: this.goalForm.value.description!,
      periodUnit: this.goalForm.value.periodUnit!,
      periodFrequency: this.goalForm.value.periodFrequency!,
      importance: this.goalForm.value.importance!,
      trackingType: this.goalForm.value.trackingType!
    };

    this.goalService.createGoal(dto).subscribe({
      next: (result) => {
        if (result.success) {
          this.goalSuccess = result.message ?? 'Hedef oluşturuldu.';
          this.goalForm.reset({ periodUnit: PeriodUnit.Gun, periodFrequency: 1, importance: Importance.Orta, trackingType: TrackingType.Sureli });
          this.loadGoals();
        } else {
          this.goalError = result.message ?? 'Hedef oluşturulamadı.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.goalError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  // Backend, durum kaydı olan bir hedefi ilk denemede silmiyor, onay istiyor (data:true döner).
  deleteGoal(goal: GoalDto): void {
    this.goalService.deleteGoal(goal.id, false).subscribe({
      next: (result) => {
        if (result.data === true) {
          this.confirmMessage = result.message ?? 'Bu hedefe ait durum kayıtları var. Silmek istediğinize emin misiniz?';
          this.pendingDeleteGoal = goal;
        } else {
          this.loadGoals();
        }
      },
      error: (err: HttpErrorResponse) => {
        this.goalError = err.error?.message ?? 'Silme başarısız.';
      }
    });
  }

  confirmDelete(): void {
    if (!this.pendingDeleteGoal) return;
    this.goalService.deleteGoal(this.pendingDeleteGoal.id, true).subscribe({
      next: () => {
        this.pendingDeleteGoal = null;
        this.loadGoals();
      },
      error: (err: HttpErrorResponse) => {
        this.goalError = err.error?.message ?? 'Silme başarısız.';
        this.pendingDeleteGoal = null;
      }
    });
  }

  cancelDelete(): void {
    this.pendingDeleteGoal = null;
  }

  requestPauseGoal(goal: GoalDto): void {
    this.pendingPauseGoal = goal;
    this.pauseReasonInput = '';
    this.pauseError = '';
  }

  cancelPauseGoal(): void {
    this.pendingPauseGoal = null;
  }

  confirmPauseGoal(): void {
    if (!this.pendingPauseGoal) return;

    const reason = this.pauseReasonInput.trim();
    if (!reason) {
      this.pauseError = 'Duraklatma sebebi yazılmalı.';
      return;
    }

    this.pauseSubmitting = true;
    this.pauseError = '';

    this.goalService.pauseGoal(this.pendingPauseGoal.id, { reason }).subscribe({
      next: (result) => {
        this.pauseSubmitting = false;
        if (result.success) {
          this.pendingPauseGoal = null;
          this.loadGoals();
        } else {
          this.pauseError = result.message ?? 'Hedef duraklatılamadı.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.pauseSubmitting = false;
        this.pauseError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  resumeGoal(goal: GoalDto): void {
    this.resumingGoalId = goal.id;
    this.goalError = '';

    this.goalService.resumeGoal(goal.id).subscribe({
      next: (result) => {
        this.resumingGoalId = null;
        if (result.success) {
          this.loadGoals();
        } else {
          this.goalError = result.message ?? 'Hedef tekrar aktif edilemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.resumingGoalId = null;
        this.goalError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImage = input.files?.[0] ?? null;
  }

  // Durum kaydı zorunlu alanları + (doluysa) açıklama/görsel notunu tek butonla,
  // arka arkaya iki ayrı backend çağrısıyla kaydeder (GoalStatus ve StatusNote,
  // şemada birbirinden bağımsız iki tablo — ama kullanıcıya tek işlem gibi görünür).
  submitStatus(): void {
    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      return;
    }

    // Görsel tek başına anlamsız kalıyor (haftalar sonra bağlamı kayboluyor) — açıklamasız
    // gönderilirse StatusNote hiç oluşturulmuyor ve seçilen görsel sessizce kayboluyordu.
    if (this.selectedImage && !this.statusForm.value.noteDescription?.trim()) {
      this.statusError = 'Görsel eklediğinizde açıklama yazmanız gerekiyor.';
      return;
    }

    this.statusError = '';
    this.statusSuccess = '';

    // <input type="time"> saniyesiz "HH:mm" gönderiyor ama backend'deki TimeOnly
    // JSON çözümleyicisi saniye istiyor ("HH:mm:ss") — eksikse burada tamamlıyoruz.
    const rawTime = this.statusForm.value.activityTime!;
    const activityTime = rawTime.length === 5 ? `${rawTime}:00` : rawTime;

    const isSureli = this.selectedGoalForStatus?.trackingType === TrackingType.Sureli;

    const dto: CreateGoalStatusDto = {
      goalId: this.statusForm.value.goalId!,
      activityDate: this.statusForm.value.activityDate!,
      activityTime,
      durationMinutes: isSureli ? this.statusForm.value.durationMinutes! : null
    };

    this.goalStatusService.createGoalStatus(dto).subscribe({
      next: (result) => {
        if (!result.success || result.data == null) {
          this.statusError = result.message ?? 'Kayıt başarısız.';
          return;
        }
        this.saveNoteIfPresent(result.data);
      },
      error: (err: HttpErrorResponse) => {
        this.statusError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  private saveNoteIfPresent(goalStatusId: number): void {
    const description = this.statusForm.value.noteDescription?.trim();

    if (!description) {
      this.statusSuccess = 'Durum kaydedildi.';
      this.resetAfterSave();
      return;
    }

    this.statusNoteService.createStatusNote(description, this.selectedImage, goalStatusId).subscribe({
      next: (result) => {
        this.statusSuccess = result.success
          ? 'Durum ve not kaydedildi.'
          : `Durum kaydedildi, ama not kaydedilemedi: ${result.message}`;
        this.resetAfterSave();
      },
      error: (err: HttpErrorResponse) => {
        this.statusSuccess = 'Durum kaydedildi, ama not kaydedilemedi.';
        console.error(err);
        this.resetAfterSave();
      }
    });
  }

  private resetAfterSave(): void {
    // Ardı ardına kayıt eklenebilsin diye tarih/saat her kayıttan sonra o ana yenileniyor.
    this.statusForm.patchValue({
      noteDescription: '',
      activityDate: this.currentDateStr(),
      activityTime: this.currentTimeStr()
    });
    this.selectedImage = null;
    this.loadLast7Days();
  }

  private currentDateStr(): string {
    const d = new Date();
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private currentTimeStr(): string {
    const d = new Date();
    const h = String(d.getHours()).padStart(2, '0');
    const m = String(d.getMinutes()).padStart(2, '0');
    return `${h}:${m}`;
  }
}
