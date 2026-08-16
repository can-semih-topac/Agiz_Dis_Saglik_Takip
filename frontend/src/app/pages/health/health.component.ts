import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
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

@Component({
  selector: 'app-health',
  imports: [ReactiveFormsModule, RouterLink, NavbarComponent, StatusDetailModalComponent],
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
    title.setTitle('Ağız ve Diş Sağlığı | ADS');
  }

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  apiOrigin = environment.apiBaseUrl.replace('/api', '');

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

  periodUnits = [
    { value: PeriodUnit.Gun, label: 'Gün' },
    { value: PeriodUnit.Hafta, label: 'Hafta' },
    { value: PeriodUnit.Ay, label: 'Ay' }
  ];

  importanceLevels = [
    { value: Importance.Dusuk, label: 'Düşük' },
    { value: Importance.Orta, label: 'Orta' },
    { value: Importance.Yuksek, label: 'Yüksek' }
  ];

  trackingTypes = [
    { value: TrackingType.Sureli, label: 'Süreli Kaydet' },
    { value: TrackingType.Yapildi, label: 'Sadece Yapıldı Olarak İşaretle' }
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
    activityDate: ['', Validators.required],
    activityTime: ['', Validators.required],
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
    this.statusForm.patchValue({ noteDescription: '' });
    this.selectedImage = null;
    this.loadLast7Days();
  }
}
