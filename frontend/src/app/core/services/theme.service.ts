import { Injectable, signal } from '@angular/core';

// Kullanıcının seçtiği tercih — 'auto' cihazın/tarayıcının temasını takip eder.
export type ThemePreference = 'light' | 'dark' | 'auto';
// Ekrana gerçekten uygulanan tema ('auto' burada çözümlenmiş olur).
export type ResolvedTheme = 'light' | 'dark';

const STORAGE_KEY = 'theme_preference';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

  readonly preference = signal<ThemePreference>(this.readStoredPreference());
  readonly resolved = signal<ResolvedTheme>('light');

  constructor() {
    this.apply();

    // 'auto' seçiliyken cihaz teması değişirse (ör. gece moduna geçilirse) anında yansısın.
    this.mediaQuery.addEventListener('change', () => {
      if (this.preference() === 'auto') {
        this.apply();
      }
    });
  }

  setPreference(preference: ThemePreference): void {
    this.preference.set(preference);
    localStorage.setItem(STORAGE_KEY, preference);
    this.apply();
  }

  // Switch için: o an neyi görüyorsa onun tersine geçer (ve artık 'auto' olmaktan çıkar).
  toggle(): void {
    this.setPreference(this.resolved() === 'dark' ? 'light' : 'dark');
  }

  private readStoredPreference(): ThemePreference {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'light' || stored === 'dark' || stored === 'auto' ? stored : 'auto';
  }

  private apply(): void {
    const preference = this.preference();
    const resolved: ResolvedTheme =
      preference === 'auto' ? (this.mediaQuery.matches ? 'dark' : 'light') : preference;

    this.resolved.set(resolved);
    document.documentElement.setAttribute('data-theme', resolved);
  }
}
