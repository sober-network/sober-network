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
  mediaId?: string | null;
  mediaUrl?: string | null;
  thumbnailUrl?: string | null;
  mediaType?: 'image' | 'video' | null;
  imageWidth?: number | null;
  imageHeight?: number | null;
  videoDurationSeconds?: number | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
  isApproved: boolean;
  needsApproval: boolean;
  createdAt: string;
  updatedAt: string;
  commentCount: number;
}

export interface CreatePostRequest {
  groupSlug: string;
  subject: string;
  body: string;
  mediaId?: string | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
}

export interface UpdatePostRequest {
  subject?: string | null;
  body?: string | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
}

export interface PostsFeedResponse {
  items: PostResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}

// ── Comments ──────────────────────────────────────────────────────────────────

export interface CommentResponse {
  id: string;
  postId: string;
  parentCommentId?: string | null;
  authorId: string;
  authorDisplayName: string;
  body: string;
  linkUrl?: string | null;
  linkTitle?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateCommentRequest {
  body: string;
  parentCommentId?: string | null;
  linkUrl?: string | null;
  linkTitle?: string | null;
}

export interface UpdateCommentRequest {
  body: string;
  linkUrl?: string | null;
  linkTitle?: string | null;
}

/** Client-side comment node for threaded rendering. */
export interface CommentNode extends CommentResponse {
  replies: CommentNode[];
}
