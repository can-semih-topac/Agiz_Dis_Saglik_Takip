export interface UserProfileDto {
  email: string;
  fullName: string;
  birthDate: string;
}

export interface UpdateProfileDto {
  email: string;
  fullName: string;
  birthDate: string;
  newPassword?: string | null;
  newPasswordConfirm?: string | null;
}
