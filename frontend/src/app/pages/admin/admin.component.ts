import { Component, OnInit, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { environment } from '../../../environments/environment';
import { ContactService } from '../../core/services/contact.service';
import { ContactMessageDto } from '../../core/models/contact.models';
import { LogService } from '../../core/services/log.service';
import { LogDto } from '../../core/models/log.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { formatTurkishDateTime } from '../../shared/turkish-date';

type AdminTab = 'messages' | 'logs';

@Component({
  selector: 'app-admin',
  imports: [NavbarComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  private contactService = inject(ContactService);
  private logService = inject(LogService);

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  apiOrigin = environment.apiBaseUrl.replace('/api', '');

  activeTab: AdminTab = 'messages';

  messages: ContactMessageDto[] = [];
  messagesLoading = true;
  messagesError = '';

  logs: LogDto[] = [];
  logsLoading = true;
  logsError = '';

  constructor(title: Title) {
    title.setTitle('Admin Paneli | ADS');
  }

  ngOnInit(): void {
    this.contactService.getAllMessages().subscribe({
      next: (result) => {
        this.messagesLoading = false;
        if (result.success) {
          this.messages = result.data;
        } else {
          this.messagesError = result.message ?? 'Mesajlar alınamadı.';
        }
      },
      error: () => {
        this.messagesLoading = false;
        this.messagesError = 'Sunucuya ulaşılamadı.';
      }
    });

    this.logService.getRecent().subscribe({
      next: (result) => {
        this.logsLoading = false;
        if (result.success) {
          this.logs = result.data;
        } else {
          this.logsError = result.message ?? 'Loglar alınamadı.';
        }
      },
      error: () => {
        this.logsLoading = false;
        this.logsError = 'Sunucuya ulaşılamadı.';
      }
    });
  }

  selectTab(tab: AdminTab): void {
    this.activeTab = tab;
  }

  // Error/Critical kırmızı, Warning turuncu — Log seviyesi (LogLevel enum'unun ToString() hali) burada belirliyor.
  levelClass(level: string): string {
    if (level === 'Error' || level === 'Critical') return 'is-error';
    if (level === 'Warning') return 'is-warning';
    return '';
  }

  formatDate(createdAt: string): string {
    const [date, time] = createdAt.split('T');
    return formatTurkishDateTime(date, time ?? '00:00:00');
  }
}
