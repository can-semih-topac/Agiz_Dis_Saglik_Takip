// Backend'deki RegisterDto'nun karşılığı.
// Not: property adları camelCase — ASP.NET Core, C#'taki PascalCase (Email, FullName)
// alanları JSON'a çevirirken otomatik camelCase yapıyor (email, fullName), o yüzden birebir eşleşiyor.
export interface RegisterDto {
  email: string;
  password: string;
  passwordConfirm: string;
  fullName: string;
  birthDate: string; // "YYYY-MM-DD" formatında ISO tarih metni
  phoneNumber: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

// Backend'deki LoginResultDto'nun karşılığı.
export interface LoginResultDto {
  token: string;
  email: string;
  fullName: string;
}

// localStorage'a bu şekilde kaydedeceğiz — token + ekranlarda göstereceğimiz kullanıcı bilgisi bir arada.
export interface AuthSession {
  token: string;
  email: string;
  fullName: string;
}

export interface VerifyEmailDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  newPassword: string;
  newPasswordConfirm: string;
}
