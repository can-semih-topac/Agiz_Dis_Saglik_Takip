// Backend'deki RegisterDto'nun karşılığı.
// Not: property adları camelCase — ASP.NET Core, C#'taki PascalCase (Email, FullName)
// alanları JSON'a çevirirken otomatik camelCase yapıyor (email, fullName), o yüzden birebir eşleşiyor.
export interface RegisterDto {
  email: string;
  password: string;
  passwordConfirm: string;
  fullName: string;
  birthDate: string; // "YYYY-MM-DD" formatında ISO tarih metni
}
