import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';
import { GoalService } from '../../core/services/goal.service';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { StatusNoteService } from '../../core/services/status-note.service';
import { SuggestionService } from '../../core/services/suggestion.service';
import { GoalDto, PeriodUnit, Importance, CreateGoalDto } from '../../core/models/goal.models';
import { GoalStatusDto, CreateGoalStatusDto } from '../../core/models/goal-status.models';
import { StatusNoteDto } from '../../core/models/status-note.models';

@Component({
  selector: 'app-health',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './health.component.html',
  styleUrl: './health.component.css'
})
export class HealthComponent implements OnInit {
  private fb = inject(FormBuilder);
  private goalService = inject(GoalService);
  private goalStatusService = inject(GoalStatusService);
  private statusNoteService = inject(StatusNoteService);
  private suggestionService = inject(SuggestionService);

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  apiOrigin = environment.apiBaseUrl.replace('/api', '');

  activeTab: 'durum' | 'hedef' = 'durum';

  goals: GoalDto[] = [];
  last7DaysStatus: GoalStatusDto[] = [];
  last7DaysNotes: StatusNoteDto[] = [];
  suggestionText = '';
  selectedImage: File | null = null;

  goalError = '';
  goalSuccess = '';
  statusError = '';
  statusSuccess = '';
  noteError = '';
  noteSuccess = '';

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

  goalForm = this.fb.group({
    title: ['', Validators.required],
    description: ['', Validators.required],
    periodUnit: [PeriodUnit.Gun, Validators.required],
    periodFrequency: [1, [Validators.required, Validators.min(1)]],
    importance: [Importance.Orta, Validators.required]
  });

  statusForm = this.fb.group({
    goalId: [null as number | null, Validators.required],
    activityDate: ['', Validators.required],
    activityTime: ['', Validators.required],
    durationMinutes: [0, [Validators.required, Validators.min(0)]],
    isApplied: [true]
  });

  noteForm = this.fb.group({
    description: ['', Validators.required]
  });

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
      next: (result) => { if (result.success) this.last7DaysNotes = result.data; }
    });
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
      importance: this.goalForm.value.importance!
    };

    this.goalService.createGoal(dto).subscribe({
      next: (result) => {
        if (result.success) {
          this.goalSuccess = result.message ?? 'Hedef oluşturuldu.';
          this.goalForm.reset({ periodUnit: PeriodUnit.Gun, periodFrequency: 1, importance: Importance.Orta });
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
          const confirmed = confirm(result.message ?? 'Bu hedefe ait durum kayıtları var. Silmek istediğinize emin misiniz?');
          if (confirmed) {
            this.goalService.deleteGoal(goal.id, true).subscribe({ next: () => this.loadGoals() });
          }
        } else {
          this.loadGoals();
        }
      },
      error: (err: HttpErrorResponse) => {
        this.goalError = err.error?.message ?? 'Silme başarısız.';
      }
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImage = input.files?.[0] ?? null;
  }

  createStatus(): void {
    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      return;
    }
    this.statusError = '';
    this.statusSuccess = '';

    const dto: CreateGoalStatusDto = {
      goalId: this.statusForm.value.goalId!,
      activityDate: this.statusForm.value.activityDate!,
      activityTime: this.statusForm.value.activityTime!,
      durationMinutes: this.statusForm.value.durationMinutes!,
      isApplied: this.statusForm.value.isApplied!
    };

    this.goalStatusService.createGoalStatus(dto).subscribe({
      next: (result) => {
        if (result.success) {
          this.statusSuccess = result.message ?? 'Durum kaydedildi.';
          this.loadLast7Days();
        } else {
          this.statusError = result.message ?? 'Kayıt başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.statusError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  createNote(): void {
    if (this.noteForm.invalid) {
      this.noteForm.markAllAsTouched();
      return;
    }
    this.noteError = '';
    this.noteSuccess = '';

    this.statusNoteService.createStatusNote(this.noteForm.value.description!, this.selectedImage).subscribe({
      next: (result) => {
        if (result.success) {
          this.noteSuccess = result.message ?? 'Not kaydedildi.';
          this.noteForm.reset();
          this.selectedImage = null;
          this.loadLast7Days();
        } else {
          this.noteError = result.message ?? 'Kayıt başarısız.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.noteError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
