import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { GoalStatusDto } from '../../core/models/goal-status.models';
import { StatusNoteDto } from '../../core/models/status-note.models';
import { TrackingType } from '../../core/models/goal.models';
import { GoalStatusService } from '../../core/services/goal-status.service';
import { StatusNoteService } from '../../core/services/status-note.service';
import { formatTurkishDateTime } from '../turkish-date';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-status-detail-modal',
  imports: [ReactiveFormsModule, TranslocoPipe],
  templateUrl: './status-detail-modal.component.html',
  styleUrl: './status-detail-modal.component.css'
})
export class StatusDetailModalComponent implements OnChanges {
  private fb = inject(FormBuilder);
  private goalStatusService = inject(GoalStatusService);
  private statusNoteService = inject(StatusNoteService);

  @Input() status!: GoalStatusDto;
  @Input() note: StatusNoteDto | null = null;
  @Output() closed = new EventEmitter<void>();
  // Düzenleme kaydedilince tetiklenir — üst bileşen listeyi yeniden yüklesin diye.
  @Output() saved = new EventEmitter<void>();

  readonly TrackingType = TrackingType;

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  // Not: apiBaseUrl'nin SONUNDAKİ "/api"yi kesiyoruz — düz .replace('/api','') canlıda
  // "api.cansemihtopac.com" gibi "api" ile başlayan alt alan adlarında baştaki "/api"yi
  // yanlışlıkla siliyor ve adresi bozuyordu (regex'teki $ ifadesi sonu sabitliyor).
  apiOrigin = environment.apiBaseUrl.replace(/\/api$/, '');

  isEditing = false;
  isSaving = false;
  saveError = '';
  selectedImage: File | null = null;
  removeImage = false;
  // Düzenlemeye başlarken bir not zaten varsa açıklama zorunlu kalır (not silme özelliği yok);
  // hiç yoksa boş bırakılabilir, o zaman not hiç oluşturulmaz.
  descriptionRequired = false;

  editForm = this.fb.group({
    activityDate: ['', Validators.required],
    activityTime: ['', Validators.required],
    durationMinutes: [0 as number | null],
    description: ['']
  });

  get formattedDate(): string {
    return formatTurkishDateTime(this.status.activityDate, this.status.activityTime);
  }

  ngOnChanges(): void {
    // Farklı bir kayda tıklanınca (modal kapanmadan) düzenleme modu ve hata mesajı sıfırlansın.
    this.isEditing = false;
    this.saveError = '';
  }

  close(): void {
    this.closed.emit();
  }

  startEdit(): void {
    this.descriptionRequired = this.note != null;
    this.editForm.get('description')!.setValidators(this.descriptionRequired ? [Validators.required] : []);
    this.editForm.get('description')!.updateValueAndValidity();

    const isSureli = this.status.trackingType === TrackingType.Sureli;
    this.editForm.get('durationMinutes')!.setValidators(isSureli ? [Validators.required, Validators.min(0)] : []);
    this.editForm.get('durationMinutes')!.updateValueAndValidity();

    this.editForm.setValue({
      activityDate: this.status.activityDate,
      activityTime: this.status.activityTime.slice(0, 5),
      durationMinutes: this.status.durationMinutes,
      description: this.note?.description ?? ''
    });
    this.selectedImage = null;
    this.removeImage = false;
    this.saveError = '';
    this.isEditing = true;
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.saveError = '';
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImage = input.files?.[0] ?? null;
    if (this.selectedImage) {
      this.removeImage = false;
    }
  }

  toggleRemoveImage(): void {
    this.removeImage = !this.removeImage;
    if (this.removeImage) {
      this.selectedImage = null;
    }
  }

  submitEdit(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.saveError = '';

    const rawTime = this.editForm.value.activityTime!;
    const activityTime = rawTime.length === 5 ? `${rawTime}:00` : rawTime;
    const isSureli = this.status.trackingType === TrackingType.Sureli;

    this.goalStatusService.updateGoalStatus(this.status.id, {
      activityDate: this.editForm.value.activityDate!,
      activityTime,
      durationMinutes: isSureli ? this.editForm.value.durationMinutes! : null
    }).subscribe({
      next: (result) => {
        if (!result.success) {
          this.isSaving = false;
          this.saveError = result.message ?? 'Durum kaydı güncellenemedi.';
          return;
        }
        this.saveNoteIfNeeded();
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving = false;
        this.saveError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  private saveNoteIfNeeded(): void {
    const description = this.editForm.value.description?.trim() ?? '';

    if (!description) {
      // Zaten var olan bir not zorunlu olduğu için buraya sadece "hiç not yoktu, hâlâ yok" durumunda düşülür.
      this.finishSave();
      return;
    }

    const request = this.note
      ? this.statusNoteService.updateStatusNote(this.note.id, description, this.selectedImage, this.removeImage)
      : this.statusNoteService.createStatusNote(description, this.selectedImage, this.status.id);

    request.subscribe({
      next: (result) => {
        this.isSaving = false;
        if (!result.success) {
          this.saveError = result.message ?? 'Not güncellenemedi.';
          return;
        }
        this.finishSave();
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving = false;
        this.saveError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  private finishSave(): void {
    this.isSaving = false;
    this.isEditing = false;
    this.saved.emit();
  }
}
