import { Component, OnInit, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { ContactService } from '../../core/services/contact.service';
import { ContactMessageDto, ContactMessageStatus } from '../../core/models/contact.models';
import { LogService } from '../../core/services/log.service';
import { LogDto } from '../../core/models/log.models';
import { UserService } from '../../core/services/user.service';
import { Role, UserAdminDto } from '../../core/models/user.models';
import { AdminActionLogService } from '../../core/services/admin-action-log.service';
import { AdminActionLogDto } from '../../core/models/admin-action-log.models';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AddUserModalComponent } from '../../shared/add-user-modal/add-user-modal.component';
import { formatTurkishDate, formatTurkishDateTime } from '../../shared/turkish-date';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { TranslocoPipe } from '@jsverse/transloco';

type AdminTab = 'messages' | 'logs' | 'adminActions' | 'users';

@Component({
  selector: 'app-admin',
  imports: [NavbarComponent, AddUserModalComponent, TableModule, DialogModule, TranslocoPipe, FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  private contactService = inject(ContactService);
  private logService = inject(LogService);
  private userService = inject(UserService);
  private adminActionLogService = inject(AdminActionLogService);

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  // Not: apiBaseUrl'nin SONUNDAKİ "/api"yi kesiyoruz — düz .replace('/api','') canlıda
  // "api.cansemihtopac.com" gibi "api" ile başlayan alt alan adlarında baştaki "/api"yi
  // yanlışlıkla siliyor ve adresi bozuyordu (regex'teki $ ifadesi sonu sabitliyor).
  apiOrigin = environment.apiBaseUrl.replace(/\/api$/, '');

  readonly Role = Role;
  readonly ContactMessageStatus = ContactMessageStatus;

  activeTab: AdminTab = 'messages';

  messages: ContactMessageDto[] = [];
  messagesLoading = true;
  messagesError = '';
  reviewingMessageId: number | null = null;
  reviewErrorMessage = '';
  messagesSubTab: 'pending' | 'reviewed' = 'pending';
  selectedMessage: ContactMessageDto | null = null;

  logs: LogDto[] = [];
  logsLoading = true;
  logsError = '';

  // ElasticSearch üzerinden tam metin arama — arama aktifken listede sonuçlar,
  // kutu boşaltılınca son 200 kayda geri dönülür.
  logSearchTerm = '';
  logSearchResults: LogDto[] | null = null;
  logSearchLoading = false;
  logSearchError = '';

  adminActions: AdminActionLogDto[] = [];
  adminActionsLoading = true;
  adminActionsError = '';

  users: UserAdminDto[] = [];
  usersLoading = true;
  usersError = '';

  showAddUserModal = false;

  pendingDeleteUser: UserAdminDto | null = null;
  deleteSubmitting = false;
  deleteErrorMessage = '';

  constructor(title: Title) {
    title.setTitle('Admin Paneli | ADS');
  }

  ngOnInit(): void {
    this.loadMessages();

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

    this.loadAdminActions();
    this.loadUsers();
  }

  loadMessages(): void {
    this.messagesLoading = true;
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
  }

  get pendingMessages(): ContactMessageDto[] {
    return this.messages.filter(m => m.status === ContactMessageStatus.Pending);
  }

  get reviewedMessages(): ContactMessageDto[] {
    return this.messages.filter(m => m.status === ContactMessageStatus.Reviewed);
  }

  get visibleMessages(): ContactMessageDto[] {
    return this.messagesSubTab === 'pending' ? this.pendingMessages : this.reviewedMessages;
  }

  selectMessagesSubTab(tab: 'pending' | 'reviewed'): void {
    this.messagesSubTab = tab;
  }

  messagePreview(text: string): string {
    return text.length > 90 ? text.slice(0, 90) + '…' : text;
  }

  openMessageDetail(message: ContactMessageDto): void {
    this.selectedMessage = message;
  }

  closeMessageDetail(): void {
    this.selectedMessage = null;
  }

  markAsReviewed(message: ContactMessageDto): void {
    this.reviewingMessageId = message.id;
    this.reviewErrorMessage = '';

    this.contactService.markAsReviewed(message.id).subscribe({
      next: (result) => {
        this.reviewingMessageId = null;
        if (result.success) {
          this.closeMessageDetail();
          this.loadMessages();
          this.loadAdminActions();
        } else {
          this.reviewErrorMessage = result.message ?? 'Mesaj güncellenemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.reviewingMessageId = null;
        this.reviewErrorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  get displayedLogs(): LogDto[] {
    return this.logSearchResults ?? this.logs;
  }

  searchLogs(): void {
    const term = this.logSearchTerm.trim();
    if (!term) {
      this.clearLogSearch();
      return;
    }

    this.logSearchLoading = true;
    this.logSearchError = '';

    this.logService.search(term).subscribe({
      next: (result) => {
        this.logSearchLoading = false;
        if (result.success) {
          this.logSearchResults = result.data;
        } else {
          this.logSearchError = result.message ?? 'Arama yapılamadı.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.logSearchLoading = false;
        this.logSearchError = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }

  clearLogSearch(): void {
    this.logSearchTerm = '';
    this.logSearchResults = null;
    this.logSearchError = '';
  }

  loadAdminActions(): void {
    this.adminActionsLoading = true;
    this.adminActionLogService.getRecent().subscribe({
      next: (result) => {
        this.adminActionsLoading = false;
        if (result.success) {
          this.adminActions = result.data;
        } else {
          this.adminActionsError = result.message ?? 'İşlem geçmişi alınamadı.';
        }
      },
      error: () => {
        this.adminActionsLoading = false;
        this.adminActionsError = 'Sunucuya ulaşılamadı.';
      }
    });
  }

  loadUsers(): void {
    this.usersLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (result) => {
        this.usersLoading = false;
        if (result.success) {
          this.users = result.data;
        } else {
          this.usersError = result.message ?? 'Kullanıcılar alınamadı.';
        }
      },
      error: () => {
        this.usersLoading = false;
        this.usersError = 'Sunucuya ulaşılamadı.';
      }
    });
  }

  selectTab(tab: AdminTab): void {
    this.activeTab = tab;
  }

  openAddUserModal(): void {
    this.showAddUserModal = true;
  }

  closeAddUserModal(): void {
    this.showAddUserModal = false;
  }

  onUserCreated(): void {
    this.loadUsers();
    this.loadAdminActions();
  }

  requestDeleteUser(user: UserAdminDto): void {
    this.deleteErrorMessage = '';
    this.pendingDeleteUser = user;
  }

  cancelDeleteUser(): void {
    this.pendingDeleteUser = null;
  }

  confirmDeleteUser(): void {
    if (!this.pendingDeleteUser) return;

    this.deleteSubmitting = true;
    this.deleteErrorMessage = '';

    this.userService.deleteUser(this.pendingDeleteUser.id).subscribe({
      next: (result) => {
        this.deleteSubmitting = false;
        if (result.success) {
          this.pendingDeleteUser = null;
          this.loadUsers();
          this.loadAdminActions();
        } else {
          this.deleteErrorMessage = result.message ?? 'Kullanıcı silinemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.deleteSubmitting = false;
        this.deleteErrorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
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

  formatBirthDate(birthDate: string | null): string {
    return birthDate ? formatTurkishDate(birthDate) : '—';
  }
}
