// Group models — mirror GroupsController request/response DTOs

export type MembershipStatus = 'Active' | 'Probationary' | 'Suspended' | 'Banned';
export type MemberRole = 'Member' | 'GroupAdmin';

export type MeetingFormat = 'Discussion' | 'Speaker' | 'StepStudy' | 'BigBook' | 'Beginners';

export const MEETING_FORMATS: MeetingFormat[] = ['Discussion', 'Speaker', 'StepStudy', 'BigBook', 'Beginners'];
export const DAYS_OF_WEEK = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

// ── Meeting DTOs ──────────────────────────────────────────────────────────────

/** Publicly-safe meeting detail (no Zoom, no Notes). Used on public group pages (T11/T12). */
export interface PublicMeetingResponse {
  id: string;
  name: string;
  description: string | null;
  isRecurring: boolean;
  dayOfWeek: number | null;       // 0=Sun..6=Sat
  time: string;                   // "HH:mm"
  durationMinutes: number;
  occursOn: string | null;        // ISO date for one-off meetings
  isOpen: boolean;
  formats: string | null;         // comma-separated
  language: string | null;
  location: string | null;
  isActive: boolean;
}

/** Member-facing meeting detail. Includes Zoom credentials. No admin Notes. */
export interface MeetingResponse extends PublicMeetingResponse {
  zoomLink: string | null;
  zoomMeetingId: string | null;
  zoomPasscode: string | null;
  createdAt: string;
}

/** Admin-only meeting detail. Includes Notes and all Zoom fields. */
export interface AdminMeetingResponse extends MeetingResponse {
  notes: string | null;
  updatedAt: string;
}

export interface CreateMeetingRequest {
  name: string;
  description?: string | null;
  notes?: string | null;
  isRecurring?: boolean;
  dayOfWeek?: number | null;
  time: string;
  durationMinutes?: number;
  occursOn?: string | null;
  isOpen?: boolean;
  formats?: string | null;
  language?: string | null;
  location?: string | null;
  zoomLink?: string | null;
  zoomMeetingId?: string | null;
  zoomPasscode?: string | null;
}

export interface UpdateMeetingRequest {
  name?: string;
  description?: string | null;
  notes?: string | null;
  isRecurring?: boolean;
  dayOfWeek?: number | null;
  time?: string;
  durationMinutes?: number;
  occursOn?: string | null;
  isOpen?: boolean;
  formats?: string | null;
  language?: string | null;
  location?: string | null;
  zoomLink?: string | null;
  zoomMeetingId?: string | null;
  zoomPasscode?: string | null;
  isActive?: boolean;
}

// ── Group DTOs ────────────────────────────────────────────────────────────────

export interface GroupResponse {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  timeZone: string | null;
  isActive: boolean;
  isPublic: boolean;
  requiresApproval: boolean;
  memberCount: number;
  userRole: string;
  createdAt: string;
  meetings: MeetingResponse[];
}

export interface GroupSummaryResponse {
  name: string;
  slug: string;
  description: string | null;
  timeZone: string | null;
  isActive: boolean;
  isPublic: boolean;
  requiresApproval: boolean;
  meetings: PublicMeetingResponse[];
}

export interface GroupMemberResponse {
  userId: string;
  displayName: string;
  role: MemberRole;
  status: MembershipStatus;
  joinedAt: string;
  isPhoneShared: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CreateGroupRequest {
  name: string;
  slug: string;
  description?: string;
  timeZone?: string;
  isPublic: boolean;
  requiresApproval: boolean;
}

export interface UpdateGroupRequest {
  name?: string;
  description?: string;
  timeZone?: string;
  isPublic?: boolean;
  requiresApproval?: boolean;
}

export interface JoinGroupRequest {
  message?: string;
}

export interface JoinRequestResponse {
  userId: string;
  displayName: string;
  email: string;
  requestedAt: string;
  message: string | null;
}

export interface UpdateMemberRoleRequest {
  role: MemberRole;
}

export interface UpdateMemberStatusRequest {
  status: MembershipStatus;
  reason?: string;
}

export interface ApproveJoinRequest {
  approved: boolean;
  reason?: string;
}

export interface PhoneVisibilityRequest {
  isShared: boolean;
}
