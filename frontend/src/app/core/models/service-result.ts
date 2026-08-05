// Backend'deki ServiceResult<T>'nin birebir karşılığı — her API cevabı bu şekilde geliyor.
export interface ServiceResult<T = void> {
  success: boolean;
  message: string | null;
  data: T;
}
