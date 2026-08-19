import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ContactService } from '../../core/services/contact.service';

@Component({
  selector: 'app-contact-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './contact-modal.component.html',
  styleUrl: './contact-modal.component.css'
})
export class ContactModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private contactService = inject(ContactService);

  // Oturum açıkken ad/e-posta hazır gelsin diye dışarıdan veriliyor.
  @Input() prefillFullName = '';
  @Input() prefillEmail = '';
  @Output() closed = new EventEmitter<void>();

  readonly supportEmail = 'can.semih.topac18@gmail.com';

  isSubmitting = false;
  submitted = false;
  errorMessage = '';
  successMessage = '';
  showSuccessDialog = false;
  selectedImage: File | null = null;
  imageWarning = '';
  emailCopied = false;
  private emailCopiedTimeout: ReturnType<typeof setTimeout> | null = null;

  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    message: ['', Validators.required]
  });

  ngOnInit(): void {
    this.form.patchValue({
      fullName: this.prefillFullName,
      email: this.prefillEmail
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.imageWarning = '';

    if (file && !/\.(jpe?g|png)$/i.test(file.name)) {
      this.imageWarning = 'Sadece .jpg, .jpeg veya .png yükleyebilirsiniz.';
      input.value = '';
      this.selectedImage = null;
      return;
    }

    this.selectedImage = file;
  }

  close(): void {
    this.closed.emit();
  }

  // Gönderim sonrası formu değil, ortada küçük bir onay penceresi gösteriyoruz —
  // "Tamam"a basınca hem bu pencere hem "Bize Ulaşın" formu kapanıp kalınan sayfaya dönülüyor.
  closeSuccessDialog(): void {
    this.showSuccessDialog = false;
    this.closed.emit();
  }

  copyEmail(): void {
    navigator.clipboard.writeText(this.supportEmail).then(() => {
      this.emailCopied = true;
      if (this.emailCopiedTimeout) clearTimeout(this.emailCopiedTimeout);
      this.emailCopiedTimeout = setTimeout(() => this.emailCopied = false, 2000);
    });
  }

  submit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.form.invalid) {
      return;
    }

    this.isSubmitting = true;

    this.contactService.sendMessage({
      fullName: this.form.value.fullName!,
      email: this.form.value.email!,
      message: this.form.value.message!
    }, this.selectedImage).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        if (result.success) {
          this.successMessage = result.message ?? 'Mesajınız gönderildi, teşekkürler!';
          this.showSuccessDialog = true;
          this.form.patchValue({ message: '' });
          this.selectedImage = null;
          this.submitted = false;
        } else {
          this.errorMessage = result.message ?? 'Mesaj gönderilemedi.';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message ?? 'Sunucuya ulaşılamadı.';
      }
    });
  }
}
