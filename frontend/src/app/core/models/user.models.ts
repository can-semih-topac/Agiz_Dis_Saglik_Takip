export interface UserProfileDto {
  email: string;
  fullName: string;
  birthDate: string;
  phoneNumber: string;
}

export interface UpdateProfileDto {
  email: string;
  fullName: string;
  birthDate: string;
  phoneNumber: string;
}

export interface ChangePasswordDto {
  oldPassword: string;
  newPassword: string;
  newPasswordConfirm: string;
}
