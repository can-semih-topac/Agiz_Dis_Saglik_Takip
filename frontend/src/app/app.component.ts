import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'frontend';

  // Uygulama açılır açılmaz oluşsun: kayıtlı tercihi uygular ve 'auto' seçiliyken
  // cihaz teması değişimini dinlemeye başlar (hangi sayfada olursak olalım).
  private themeService = inject(ThemeService);
}
