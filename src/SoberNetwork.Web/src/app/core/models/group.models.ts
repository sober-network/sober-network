// Group models — mirror GroupsController request/response DTOs

export type MembershipStatus = 'PendingApproval' | 'Active' | 'Probationary' | 'Suspended' | 'Banned';
export type MemberRole = 'Member' | 'GroupAdmin';

export type MeetingFormat =
  | 'Discussion'
  | 'Speaker'
  | 'StepStudy'
  | 'TraditionStudy'
  | 'BigBook'
  | 'Literature'
  | 'Topic'
  | 'Beginners'
  | 'Candlelight'
  | 'Meditation'
  | 'BirthdayChip'
  | 'Men'
  | 'Women'
  | 'YoungPeople'
  | 'LGBTQPlus';
export enum MeetingType { InPerson = 0, Online = 1, Hybrid = 2 }
export enum TimeBlock { Morning = 0, Afternoon = 1, Evening = 2, Night = 3 }

export const MEETING_FORMATS: MeetingFormat[] = [
  'Discussion',
  'Speaker',
  'StepStudy',
  'TraditionStudy',
  'BigBook',
  'Literature',
  'Topic',
  'Beginners',
  'Candlelight',
  'Meditation',
  'BirthdayChip',
  'Men',
  'Women',
  'YoungPeople',
  'LGBTQPlus',
];
export const MEETING_FORMAT_LABELS: Record<MeetingFormat, string> = {
  Discussion: 'Discussion',
  Speaker: 'Speaker',
  StepStudy: 'Step Study',
  TraditionStudy: 'Tradition Study',
  BigBook: 'Big Book Study',
  Literature: 'Literature',
  Topic: 'Topic',
  Beginners: 'Beginner / Newcomer',
  Candlelight: 'Candlelight',
  Meditation: 'Meditation',
  BirthdayChip: 'Birthday / Chip',
  Men: "Men's Meeting",
  Women: "Women's Meeting",
  YoungPeople: "Young People's Meeting",
  LGBTQPlus: 'LGBTQ+',
};
export const formatMeetingFormat = (format: string): string => MEETING_FORMAT_LABELS[format as MeetingFormat] ?? format;
export const DAYS_OF_WEEK = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
export const DAY_ABBR = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export const LANGUAGES = [
  'English',
  'Spanish',
  'French',
  'German',
  'Italian',
  'Portuguese',
  'Russian',
  'Chinese (Mandarin)',
  'Japanese',
  'Korean',
  'Arabic',
  'Vietnamese',
];

export const US_STATES = [
  'AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA',
  'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD',
  'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ',
  'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC',
  'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY',
  'DC', 'PR', 'VI', 'GU', 'AS', 'MP',
];

export const COUNTRIES = [
  'United States',
  'Canada',
  'Mexico',
  'United Kingdom',
  'Ireland',
  'France',
  'Germany',
  'Spain',
  'Italy',
  'Netherlands',
  'Belgium',
  'Switzerland',
  'Austria',
  'Portugal',
  'Denmark',
  'Sweden',
  'Norway',
  'Finland',
  'Poland',
  'Czech Republic',
  'Hungary',
  'Romania',
  'Greece',
  'Australia',
  'New Zealand',
  'Japan',
  'South Korea',
  'China',
  'India',
  'Brazil',
  'Argentina',
  'South Africa',
  'Other',
];

// ── Meeting DTOs ──────────────────────────────────────────────────────────────

/** Publicly-safe meeting detail (no Zoom, no Notes). Used on public group pages (T11/T12). */
export interface PublicMeetingResponse {
  id: string;
  name: string;
  description: string | null;
  isRecurring: boolean;
  daysOfWeek: number[];              // Array of 0-6 for recurring; empty array for one-off
  time: string;                      // "HH:mm"
  durationMinutes: number;
  occursOn: string | null;           // ISO date for one-off meetings
  isOpen: boolean;
  formats: string[];
  language: string | null;
  meetingType: MeetingType;
  venueName: string | null;
  location: string | null;
  street: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  latitude: number | null;
  longitude: number | null;
  publicJoinUrl: string | null;
}

/** Cross-group public meeting finder result. Includes group info and optional distance. No ZoomLink/Notes. */
export interface PublicMeetingSearchResponse extends PublicMeetingResponse {
  groupName: string;
  groupSlug: string;
  distanceMiles: number | null;
}

/** Member-facing meeting detail. Includes Zoom credentials. No admin Notes. */
export interface MeetingResponse extends PublicMeetingResponse {
  zoomLink: string | null;
  zoomMeetingId: string | null;
  zoomPasscode: string | null;
  isActive: boolean;
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
  daysOfWeek?: number[] | null;       // Array of 0-6 for recurring meetings
  time: string;
  durationMinutes?: number;
  occursOn?: string | null;
  isOpen?: boolean;
  formats?: string[];
  language?: string | null;
  meetingType?: MeetingType;
  venueName?: string | null;
  location?: string | null;
  street?: string | null;
  street2?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  zoomLink?: string | null;
  zoomMeetingId?: string | null;
  zoomPasscode?: string | null;
  publicJoinUrl?: string | null;
}

export interface UpdateMeetingRequest {
  name?: string;
  description?: string | null;
  notes?: string | null;
  isRecurring?: boolean;
  daysOfWeek?: number[] | null;       // Array of 0-6 for recurring meetings
  time?: string;
  durationMinutes?: number;
  occursOn?: string | null;
  isOpen?: boolean;
  formats?: string[];
  language?: string | null;
  meetingType?: MeetingType;
  venueName?: string | null;
  location?: string | null;
  street?: string | null;
  street2?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  zoomLink?: string | null;
  zoomMeetingId?: string | null;
  zoomPasscode?: string | null;
  publicJoinUrl?: string | null;
  isActive?: boolean;
}

/** Search parameters for the public meeting finder. */
export interface MeetingSearchParams {
  days?: number[];
  timeBlock?: TimeBlock;
  formats?: string[];
  meetingType?: MeetingType;
  isOpen?: boolean;
  lat?: number;
  lon?: number;
  radiusMiles?: number;
}

/** Mailing address — opt-in, used for chip mailing and meeting finder default. Never public (T3). */
export interface MailingAddressResponse {
  mailingStreet: string | null;
  mailingCity: string | null;
  mailingState: string | null;
  mailingPostalCode: string | null;
  mailingCountry: string | null;
  mailingLatitude: number | null;
  mailingLongitude: number | null;
}

export interface UpdateMailingAddressRequest {
  mailingStreet?: string | null;
  mailingCity?: string | null;
  mailingState?: string | null;
  mailingPostalCode?: string | null;
  mailingCountry?: string | null;
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
  userMembershipStatus: string;
  createdAt: string;
  meetings: MeetingResponse[];
  nextMeeting: NextMeetingDto | null;
  userIsPhoneShared: boolean;
  userIsEmailShared: boolean;
  // District/Area information
  districtName?: string | null;
  areaName?: string | null;
  state?: string | null;
  districtWebsiteUrl?: string | null;
  areaWebsiteUrl?: string | null;
  districtLatitude?: number | null;
  districtLongitude?: number | null;
}

export interface GroupAdminContactResponse {
  userId: string;
  displayName: string;
  email: string;
  phoneNumber: string | null;
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

export type GroupMemberResponse = MemberResponse;

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
  districtName: string;
  districtWebsiteUrl?: string | null;
  areaName: string;
  areaWebsiteUrl?: string | null;
  state: string;
  districtLatitude: number;
  districtLongitude: number;
}

export interface UpdateGroupRequest {
  name?: string;
  description?: string;
  timeZone?: string;
  isPublic?: boolean;
  requiresApproval?: boolean;
  districtName?: string | null;
  districtWebsiteUrl?: string | null;
  areaName?: string | null;
  areaWebsiteUrl?: string | null;
  state?: string | null;
  districtLatitude?: number | null;
  districtLongitude?: number | null;
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

// ── Hub DTOs ─────────────────────────────────────────────────────────────────

/** Compact next-meeting summary returned as part of GroupResponse. */
export interface NextMeetingDto {
  id: string;
  name: string;
  nextOccurrence: string; // ISO 8601 UTC
  durationMinutes: number;
  meetingType: string; // 'InPerson' | 'Online' | 'Hybrid'
}

/** Member list entry returned by GET /api/groups/{slug}/members. */
export interface MemberResponse {
  userId: string;
  displayName: string;
  role: string;
  status: string;
  isProbationary: boolean;
  joinedAt: string;
  approvedAt: string | null;
  email: string | null;
  phoneNumber: string | null;
  sobrietyDate: string | null;
}

/** Group service role (AA trusted servant position). */
export interface GroupServiceRoleResponse {
  id: string;
  userId: string;
  displayName: string;
  email: string | null;
  phoneNumber: string | null;
  roleType: string;
  customTitle: string | null;
  displayOrder: number;
}

export interface AssignServiceRoleRequest {
  userId: string;
  roleType: string;
  customTitle?: string | null;
  displayOrder?: number;
}

export interface EmailVisibilityRequest {
  isShared: boolean;
}

/** Sort options for member list. */
export type MemberSortBy = 'Name' | 'JoinedAt' | 'SobrietyDate' | 'Role';

/** Sort options for meeting list. */
export type MeetingSortBy = 'Time' | 'Name' | 'Type';
