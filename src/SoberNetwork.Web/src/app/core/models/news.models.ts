// News & Announcements models — mirror NewsController request/response DTOs

export interface PostResponse {
  id: string;
  groupId: string;
  groupName: string;
  groupSlug: string;
  authorId: string;
  authorDisplayName: string;
  subject: string;
  body: string;
  imageUrl?: string | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
  isApproved: boolean;
  needsApproval: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePostRequest {
  groupSlug: string;
  subject: string;
  body: string;
  imageUrl?: string | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
}

export interface UpdatePostRequest {
  subject?: string | null;
  body?: string | null;
  imageUrl?: string | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
}

export interface PostsFeedResponse {
  items: PostResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}
