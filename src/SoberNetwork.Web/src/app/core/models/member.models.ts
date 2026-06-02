// Member models — mirror MembersController request/response DTOs

export interface MemberProfileResponse {
  userId: string;
  displayName: string;
  email: string;
  timeZone: string | null;
  isSobrietyDatePublic: boolean;
  isDaysSoberPublic: boolean;
  hasPhone: boolean;
  createdAt: string;
}

export interface UpdateProfileRequest {
  displayName?: string;
  timeZone?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ChangeEmailRequest {
  newEmail: string;
  password: string;
}

export interface SetSobrietyDateRequest {
  sobrietyDate: string | null; // ISO date string YYYY-MM-DD, null to clear
}

export interface SobrietyVisibilityRequest {
  isSobrietyDatePublic: boolean;
  isDaysSoberPublic: boolean;
}

export interface SobrietyResponse {
  sobrietyDate: string | null;    // ISO date, null if hidden or not set
  daysSober: number | null;       // null if hidden or no date
  isSobrietyDatePublic: boolean;
  isDaysSoberPublic: boolean;
}

export interface SetPhoneRequest {
  phoneNumber: string | null; // null to remove
}

export interface PhoneListEntryResponse {
  userId: string;
  displayName: string;
  phoneNumber: string;
}

export interface MemberDetailResponse {
  userId: string;
  displayName: string;
  timeZone: string | null;
  sobriety: SobrietyResponse | null;
  phoneNumber: string | null;    // only present if caller is in same group and phone is shared
  groupsInCommon: string[];      // group slugs
}

export interface DeleteAccountRequest {
  password: string;
  confirmation: string;          // must equal "DELETE MY ACCOUNT"
}

export interface AdminMemberResponse {
  userId: string;
  displayName: string;
  email: string;
  emailConfirmed: boolean;
  isSuperAdmin: boolean;
  lockoutEnd: string | null;
  createdAt: string;
  groupCount: number;
}
