import { Component, EventEmitter, Input, Output } from '@angular/core';
import { environment } from '../../../environments/environment';
import { GoalStatusDto } from '../../core/models/goal-status.models';
import { StatusNoteDto } from '../../core/models/status-note.models';
import { TrackingType } from '../../core/models/goal.models';
import { formatTurkishDateTime } from '../turkish-date';

@Component({
  selector: 'app-status-detail-modal',
  imports: [],
  templateUrl: './status-detail-modal.component.html',
  styleUrl: './status-detail-modal.component.css'
})
export class StatusDetailModalComponent {
  @Input() status!: GoalStatusDto;
  @Input() note: StatusNoteDto | null = null;
  @Output() closed = new EventEmitter<void>();

  readonly TrackingType = TrackingType;

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  apiOrigin = environment.apiBaseUrl.replace('/api', '');

  get formattedDate(): string {
    return formatTurkishDateTime(this.status.activityDate, this.status.activityTime);
  }

  close(): void {
    this.closed.emit();
  }
}
