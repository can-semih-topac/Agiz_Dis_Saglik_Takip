import { bootstrapApplication } from '@angular/platform-browser';
import * as Sentry from '@sentry/angular';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { environment } from './environments/environment';

// Sadece production build'de devreye giriyor — service worker'daki
// !isDevMode() mantığıyla aynı sebep: ng serve ile geliştirirken kendi
// hatalarımızla Sentry'yi doldurmayalım, zaten konsolda görüyoruz.
if (environment.production) {
  Sentry.init({
    dsn: environment.sentryDsn,
    environment: 'production'
  });
}

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
