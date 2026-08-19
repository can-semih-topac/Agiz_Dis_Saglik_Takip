// Backend'deki LogDto'nun karşılığı.
export interface LogDto {
  id: number;
  level: string;
  category: string;
  message: string;
  exception: string | null;
  createdAt: string;
}
