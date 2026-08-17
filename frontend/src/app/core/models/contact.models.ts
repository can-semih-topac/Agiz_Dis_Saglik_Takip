export interface SendContactMessageDto {
  fullName: string;
  email: string;
  message: string;
}

// Backend'deki ContactMessageDto'nun karşılığı — admin panelinde listelemek için.
export interface ContactMessageDto {
  id: number;
  fullName: string;
  email: string;
  message: string;
  imagePath: string | null;
  createdAt: string;
}
