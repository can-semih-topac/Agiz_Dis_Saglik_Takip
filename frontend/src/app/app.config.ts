import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

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
    })
  ]
};
