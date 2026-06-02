// Auth models — mirror AuthController request/response DTOs

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string; // ISO 8601
  userId: string;
  displayName: string;
  email: string;
  isSuperAdmin: boolean;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface ConfirmEmailRequest {
  userId: string;
  token: string;
}

export interface RevokeTokenRequest {
  refreshToken: string;
}

/** Decoded JWT payload — stored in AuthService as the current user. */
export interface CurrentUser {
  userId: string;
  displayName: string;
  email: string;
  isSuperAdmin: boolean;
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
}
