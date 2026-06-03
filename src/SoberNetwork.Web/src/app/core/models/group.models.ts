// Group models — mirror GroupsController request/response DTOs

export type MembershipStatus = 'Active' | 'Probationary' | 'Suspended' | 'Banned';
export type MemberRole = 'Member' | 'GroupAdmin';

export type MeetingFormat = 'Discussion' | 'Speaker' | 'StepStudy' | 'BigBook' | 'Beginners';

export const MEETING_FORMATS: MeetingFormat[] = ['Discussion', 'Speaker', 'StepStudy', 'BigBook', 'Beginners'];
export const DAYS_OF_WEEK = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

export interface GroupResponse {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  meetingSchedule: string | null;
  meetingDay: number | null;        // 0=Sun..6=Sat
  meetingTime: string | null;       // "HH:mm"
  durationMinutes: number;
  isOpen: boolean;
  language: string | null;
  meetingFormats: string | null;    // comma-separated MeetingFormat values
  zoomLink: string | null;
  zoomMeetingId: string | null;
  zoomPasscode: string | null;
  timeZone: string | null;
  isActive: boolean;
  isPublic: boolean;
  requiresApproval: boolean;
  memberCount: number;
  userRole: string;
  createdAt: string;
}

export interface GroupSummaryResponse {
  name: string;
  slug: string;
  description: string | null;
  meetingSchedule: string | null;
  meetingDay: number | null;
  meetingTime: string | null;
  durationMinutes: number;
  isOpen: boolean;
  language: string | null;
  meetingFormats: string | null;
  timeZone: string | null;
  isActive: boolean;
  isPublic: boolean;
  requiresApproval: boolean;
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
  meetingSchedule?: string;
  meetingDay?: number | null;
  meetingTime?: string | null;
  durationMinutes?: number;
  isOpen?: boolean;
  language?: string;
  meetingFormats?: string;
  zoomLink?: string;
  zoomMeetingId?: string;
  zoomPasscode?: string;
  timeZone?: string;
  isPublic: boolean;
  requiresApproval: boolean;
}

export interface UpdateGroupRequest {
  name?: string;
  description?: string;
  meetingSchedule?: string;
  meetingDay?: number | null;
  meetingTime?: string | null;
  durationMinutes?: number;
  isOpen?: boolean;
  language?: string;
  meetingFormats?: string;
  zoomLink?: string;
  zoomMeetingId?: string;
  zoomPasscode?: string;
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
