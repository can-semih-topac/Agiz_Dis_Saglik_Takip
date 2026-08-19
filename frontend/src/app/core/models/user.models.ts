export interface UserProfileDto {
  email: string;
  fullName: string;
  birthDate: string | null;
  phoneNumber: string;
  hasPassword: boolean;
  mustChangePassword: boolean;
}

export interface UpdateProfileDto {
  email: string;
  fullName: string;
  birthDate: string | null;
  phoneNumber: string;
}

export interface ChangePasswordDto {
  oldPassword: string;
  newPassword: string;
  newPasswordConfirm: string;
}

// Backend'deki Role enum'unun karşılığı — JSON'a sayı olarak geliyor (0/1).
export enum Role {
  User = 0,
  Admin = 1
}

// Backend'deki UserAdminDto'nun karşılığı — admin panelindeki kullanıcı listesi için.
export interface UserAdminDto {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string;
  birthDate: string | null;
  role: Role;
  createdAt: string;
  willpowerScore: number;
}

// Backend'deki CreateUserByAdminDto'nun karşılığı.
export interface CreateUserByAdminDto {
  email: string;
  role: Role;
  temporaryPassword: string | null;
}
