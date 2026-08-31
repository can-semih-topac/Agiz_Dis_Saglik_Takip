import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Translation, TranslocoLoader } from '@jsverse/transloco';

// Transloco her dil değişiminde bu sınıfın getTranslation() metodunu çağırıp
// çeviri JSON dosyasını indiriyor — biz sadece dosyanın nerede olduğunu söylüyoruz.
@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
  private http = inject(HttpClient);

  getTranslation(lang: string) {
    return this.http.get<Translation>(`/i18n/${lang}.json`);
  }
}
