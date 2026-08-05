import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SuggestionDto } from '../models/suggestion.models';
import { ServiceResult } from '../models/service-result';

@Injectable({ providedIn: 'root' })
export class SuggestionService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/suggestion`;

  getRandom(): Observable<ServiceResult<SuggestionDto>> {
    return this.http.get<ServiceResult<SuggestionDto>>(`${this.baseUrl}/random`);
  }
}
