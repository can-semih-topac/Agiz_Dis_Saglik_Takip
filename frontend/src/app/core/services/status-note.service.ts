import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StatusNoteDto } from '../models/status-note.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class StatusNoteService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/statusnote`;

  createStatusNote(description: string, image: File | null): Observable<ServiceResult> {
    const formData = new FormData();
    formData.append('description', description);
    if (image) {
      formData.append('image', image);
    }
    return this.http.post<ServiceResult>(this.baseUrl, formData);
  }

  getLast7Days(): Observable<ServiceResult<StatusNoteDto[]>> {
    return this.http.get<ServiceResult<StatusNoteDto[]>>(`${this.baseUrl}/last7days`);
  }
}
