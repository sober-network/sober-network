export interface NotificationResponse {
  id: string;
  type: 'CommentOnMyPost' | 'ReplyToMyComment' | string;
  postId?: string | null;
  postSubject?: string | null;
  commentId?: string | null;
  triggerUserId: string;
  triggerUserDisplayName: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationCountResponse {
  count: number;
}
