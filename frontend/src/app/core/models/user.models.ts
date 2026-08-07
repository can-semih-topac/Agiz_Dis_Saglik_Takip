export interface UserProfileDto {
  email: string;
  fullName: string;
  birthDate: string | null;
  phoneNumber: string;
  hasPassword: boolean;
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
