import { Component, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-language-toggle',
  imports: [],
  templateUrl: './language-toggle.component.html',
  styleUrl: './language-toggle.component.css'
})
export class LanguageToggleComponent {
  private translocoService = inject(TranslocoService);

  // ThemeService'teki resolved Signal'ıyla aynı desen — Transloco kendi Signal'ını hazır sunuyor.
  readonly activeLang = this.translocoService.activeLang;

  toggle(): void {
    const next = this.activeLang() === 'tr' ? 'en' : 'tr';
    this.translocoService.setActiveLang(next);
  }
}
