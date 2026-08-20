import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ThemePreference, ThemeService } from '../../core/services/theme.service';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-settings',
  imports: [RouterLink, NavbarComponent, TranslocoPipe],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent {
  private themeService = inject(ThemeService);
  private translocoService = inject(TranslocoService);

  constructor(title: Title) {
    title.setTitle('Ayarlar | ADS');
  }

  readonly preference = this.themeService.preference;
  readonly resolved = this.themeService.resolved;

  readonly themeOptions: { value: ThemePreference; icon: string }[] = [
    { value: 'light', icon: '☀️' },
    { value: 'auto', icon: '🖥️' },
    { value: 'dark', icon: '🌙' }
  ];

  selectTheme(preference: ThemePreference): void {
    this.themeService.setPreference(preference);
  }

  // Şimdilik sadece tr/en aktif — activeLanguages'te olmayanlar aşağıda soluk ve
  // "Çok Yakında" rozetiyle gösteriliyor (henüz i18n dosyaları yok, seçilemezler).
  readonly activeLang = this.translocoService.activeLang;

  readonly activeLanguages: { code: string; nativeLabel: string }[] = [
    { code: 'tr', nativeLabel: 'Türkçe' },
    { code: 'en', nativeLabel: 'English' }
  ];

  readonly comingSoonLanguages: string[] = [
    'Français', 'Italiano', '한국어', 'Deutsch', 'Español', '日本語', 'Português', '中文'
  ];

  selectLanguage(code: string): void {
    this.translocoService.setActiveLang(code);
  }
}
