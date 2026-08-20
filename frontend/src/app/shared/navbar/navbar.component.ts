import { Component, Input, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ContactModalComponent } from '../contact-modal/contact-modal.component';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, ContactModalComponent, TranslocoPipe],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Sayfa kendi başlığını verirse onu gösteririz (ör. "Profil"), vermezse
  // varsayılan olarak (Ana Sayfa'da olduğu gibi) hoş geldin mesajı gösterilir.
  @Input() pageTitle = '';

  fullName = this.authService.getSession()?.fullName ?? '';
  email = this.authService.getSession()?.email ?? '';
  isAdmin = this.authService.isAdmin();

  showContactModal = false;

  // Dar ekranda buton satırı taşıyor — hamburger menüyle aç/kapa yapılıyor.
  isMenuOpen = false;

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
