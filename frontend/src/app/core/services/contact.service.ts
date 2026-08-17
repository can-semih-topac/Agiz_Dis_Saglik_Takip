import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ContactMessageDto, SendContactMessageDto } from '../models/contact.models';
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

  // Sadece admin çağırabilir — backend 403/401 ile reddeder, burada ekstra bir kontrol gerekmiyor.
  getAllMessages(): Observable<ServiceResult<ContactMessageDto[]>> {
    return this.http.get<ServiceResult<ContactMessageDto[]>>(this.baseUrl);
  }
}
