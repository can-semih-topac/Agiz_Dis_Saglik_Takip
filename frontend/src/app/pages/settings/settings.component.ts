import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ThemePreference, ThemeService } from '../../core/services/theme.service';
import { NavbarComponent } from '../../shared/navbar/navbar.component';

@Component({
  selector: 'app-settings',
  imports: [RouterLink, NavbarComponent],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent {
  private themeService = inject(ThemeService);

  constructor(title: Title) {
    title.setTitle('Ayarlar | ADS');
  }

  readonly preference = this.themeService.preference;
  readonly resolved = this.themeService.resolved;

  readonly themeOptions: { value: ThemePreference; label: string; icon: string; description: string }[] = [
    { value: 'light', label: 'Açık Tema', icon: '☀️', description: 'Her zaman açık görünüm.' },
    { value: 'auto', label: 'Otomatik', icon: '🖥️', description: 'Cihazınızın temasını takip eder.' },
    { value: 'dark', label: 'Koyu Tema', icon: '🌙', description: 'Her zaman koyu görünüm.' }
  ];

  selectTheme(preference: ThemePreference): void {
    this.themeService.setPreference(preference);
  }
}
