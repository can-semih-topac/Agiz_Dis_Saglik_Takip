// Backend'deki AdminActionLogDto'nun karşılığı.
export interface AdminActionLogDto {
  id: number;
  adminEmail: string;
  action: string;
  targetEmail: string;
  createdAt: string;
}
