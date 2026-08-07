import { Component } from '@angular/core';
import { ContactModalComponent } from '../contact-modal/contact-modal.component';

@Component({
  selector: 'app-auth-navbar',
  imports: [ContactModalComponent],
  templateUrl: './auth-navbar.component.html',
  styleUrl: './auth-navbar.component.css'
})
export class AuthNavbarComponent {
  showContactModal = false;
}
