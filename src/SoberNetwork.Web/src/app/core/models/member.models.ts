// Member models — mirror MembersController request/response DTOs

export interface SobrietyResponse {
  sobrietyDate: string | null;
  daysSober: number | null;
  isPublic: boolean;
}

export interface MemberProfileResponse {
  userId: string;
  displayName: string;
  firstName: string | null;
  email: string;
  phoneNumber: string | null;
  timeZone: string | null;
  sobriety: SobrietyResponse | null;
  isSuperAdmin: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface UpdateProfileRequest {
  displayName?: string;
  firstName?: string;
  timeZone?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ChangeEmailRequest {
  newEmail: string;
  currentPassword: string;
}

export interface SetSobrietyDateRequest {
  sobrietyDate: string; // ISO date string YYYY-MM-DD
}

export interface SobrietyVisibilityRequest {
  isPublic: boolean;
}

export interface SetPhoneRequest {
  phoneNumber: string | null;
}

export interface PhoneListEntryResponse {
  userId: string;
  displayName: string;
  phoneNumber: string;
}

export interface MemberDetailResponse {
  userId: string;
  displayName: string;
  role: string;
  status: string;
  isProbationary: boolean;
  sobriety: SobrietyResponse | null;
  phoneNumber: string | null;
  joinedAt: string;
}

export interface DeleteAccountRequest {
  password: string;
  confirmation: string;
}

export interface AdminMemberResponse {
  userId: string;
  displayName: string;
  firstName: string | null;
  email: string;
  phoneNumber: string | null;
  timeZone: string | null;
  sobrietyDate: string | null;
  daysSober: number | null;
  isSobrietyPublic: boolean;
  isSuperAdmin: boolean;
  isLockedOut: boolean;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  lastLoginAt: string | null;
  emailConfirmed: boolean;
  groupCount: number;
}
