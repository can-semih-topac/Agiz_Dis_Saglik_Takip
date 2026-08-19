import { Component } from '@angular/core';
import { ContactModalComponent } from '../contact-modal/contact-modal.component';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';
import { LanguageToggleComponent } from '../language-toggle/language-toggle.component';

@Component({
  selector: 'app-auth-navbar',
  imports: [ContactModalComponent, ThemeToggleComponent, LanguageToggleComponent],
  templateUrl: './auth-navbar.component.html',
  styleUrl: './auth-navbar.component.css'
})
export class AuthNavbarComponent {
  showContactModal = false;
}
