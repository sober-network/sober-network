// Group models — mirror GroupsController request/response DTOs

export type MembershipStatus = 'Active' | 'Probationary' | 'Suspended' | 'Banned';
export type MemberRole = 'Member' | 'GroupAdmin';

export interface GroupResponse {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  isPublic: boolean;
  requiresApproval: boolean;
  createdAt: string;
  memberCount: number;
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
  isPublic: boolean;
  requiresApproval: boolean;
}

export interface UpdateGroupRequest {
  name?: string;
  description?: string;
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
