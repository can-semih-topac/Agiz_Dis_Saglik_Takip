import { Component } from '@angular/core';
import { ContactModalComponent } from '../contact-modal/contact-modal.component';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-auth-navbar',
  imports: [ContactModalComponent, ThemeToggleComponent],
  templateUrl: './auth-navbar.component.html',
  styleUrl: './auth-navbar.component.css'
})
export class AuthNavbarComponent {
  showContactModal = false;
}
