import { Component, inject } from '@angular/core';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  imports: [],
  templateUrl: './theme-toggle.component.html',
  styleUrl: './theme-toggle.component.css'
})
export class ThemeToggleComponent {
  private themeService = inject(ThemeService);

  readonly resolved = this.themeService.resolved;

  toggle(): void {
    this.themeService.toggle();
  }
}
