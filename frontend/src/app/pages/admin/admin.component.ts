import { Component, OnInit, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpErrorResponse } from '@angular/common/http';
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

type AdminTab = 'messages' | 'logs' | 'adminActions' | 'users';

@Component({
  selector: 'app-admin',
  imports: [NavbarComponent, AddUserModalComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  private contactService = inject(ContactService);
  private logService = inject(LogService);
  private userService = inject(UserService);
  private adminActionLogService = inject(AdminActionLogService);

  // Görsellerin yolu backend'den "/uploads/..." olarak geliyor, başına backend adresini eklememiz lazım.
  apiOrigin = environment.apiBaseUrl.replace('/api', '');

  readonly Role = Role;
  readonly ContactMessageStatus = ContactMessageStatus;

  activeTab: AdminTab = 'messages';

  messages: ContactMessageDto[] = [];
  messagesLoading = true;
  messagesError = '';
  reviewingMessageId: number | null = null;
  reviewErrorMessage = '';

  logs: LogDto[] = [];
  logsLoading = true;
  logsError = '';

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

  markAsReviewed(message: ContactMessageDto): void {
    this.reviewingMessageId = message.id;
    this.reviewErrorMessage = '';

    this.contactService.markAsReviewed(message.id).subscribe({
      next: (result) => {
        this.reviewingMessageId = null;
        if (result.success) {
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
