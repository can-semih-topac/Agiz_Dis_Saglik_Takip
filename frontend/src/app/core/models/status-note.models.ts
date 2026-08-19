export interface StatusNoteDto {
  id: number;
  description: string;
  imagePath: string | null;
  goalStatusId: number | null;
  createdAt: string;
}
