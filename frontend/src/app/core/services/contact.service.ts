import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SendContactMessageDto } from '../models/contact.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class ContactService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/contact`;

  sendMessage(dto: SendContactMessageDto, image: File | null): Observable<ServiceResult> {
    const formData = new FormData();
    formData.append('fullName', dto.fullName);
    formData.append('email', dto.email);
    formData.append('message', dto.message);
    if (image) {
      formData.append('image', image);
    }
    return this.http.post<ServiceResult>(this.baseUrl, formData);
  }
}
