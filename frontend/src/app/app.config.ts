import { ApplicationConfig, provideZoneChangeDetection, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { provideTransloco } from '@jsverse/transloco';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { TranslocoHttpLoader } from './core/i18n/transloco-loader';
import { environment } from '../environments/environment';
import { provideServiceWorker } from '@angular/service-worker';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    // PrimeNG'nin dropdown/dialog gibi component'leri animasyon motorunu kullanıyor —
    // "Async" versiyonu bunu ilk ihtiyaç duyulduğunda yükler, başlangıç paketini şişirmez.
    provideAnimationsAsync(),
    // Aura: PrimeNG'nin hazır tema ön ayarlarından biri (Lara, Nora gibi alternatifleri de var).
    providePrimeNG({
      theme: {
        preset: Aura
      }
    }),
    provideTransloco({
      config: {
        availableLangs: ['tr', 'en'],
        defaultLang: 'tr',
        // Dil değişince ekrandaki metinler anında güncellensin diye.
        reRenderOnLangChange: true,
        prodMode: environment.production
      },
      loader: TranslocoHttpLoader
    }),
    // Service worker sadece production build'de devreye giriyor — "ng serve" ile
    // geliştirirken eski önbelleklenmiş bir sürümü görmeyelim diye.
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000'
    })
  ]
};
