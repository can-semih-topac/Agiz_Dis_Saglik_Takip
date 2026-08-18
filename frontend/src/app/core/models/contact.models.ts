export interface SendContactMessageDto {
  fullName: string;
  email: string;
  message: string;
}

// Backend'deki ContactMessageStatus enum'unun karşılığı — JSON'a sayı olarak geliyor (0/1).
export enum ContactMessageStatus {
  Pending = 0,
  Reviewed = 1
}

// Backend'deki ContactMessageDto'nun karşılığı — admin panelinde listelemek için.
export interface ContactMessageDto {
  id: number;
  fullName: string;
  email: string;
  message: string;
  imagePath: string | null;
  status: ContactMessageStatus;
  createdAt: string;
}
