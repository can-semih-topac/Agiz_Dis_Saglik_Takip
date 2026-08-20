import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

type BannerState = 'hidden' | 'offline' | 'online';

@Component({
  selector: 'app-network-status-banner',
  imports: [TranslocoPipe],
  templateUrl: './network-status-banner.component.html',
  styleUrl: './network-status-banner.component.css'
})
export class NetworkStatusBannerComponent implements OnInit, OnDestroy {
  state = signal<BannerState>('hidden');

  private hideTimeout?: ReturnType<typeof setTimeout>;
  private readonly onlineHandler = () => this.handleOnline();
  private readonly offlineHandler = () => this.handleOffline();

  ngOnInit(): void {
    // Sayfa zaten çevrimdışıyken açıldıysa (event beklemeden) hemen göster.
    if (!navigator.onLine) {
      this.state.set('offline');
    }
    window.addEventListener('online', this.onlineHandler);
    window.addEventListener('offline', this.offlineHandler);
  }

  ngOnDestroy(): void {
    window.removeEventListener('online', this.onlineHandler);
    window.removeEventListener('offline', this.offlineHandler);
    if (this.hideTimeout) clearTimeout(this.hideTimeout);
  }

  private handleOffline(): void {
    if (this.hideTimeout) clearTimeout(this.hideTimeout);
    this.state.set('offline');
  }

  private handleOnline(): void {
    if (this.hideTimeout) clearTimeout(this.hideTimeout);
    this.state.set('online');
    this.hideTimeout = setTimeout(() => this.state.set('hidden'), 3500);
  }
}
