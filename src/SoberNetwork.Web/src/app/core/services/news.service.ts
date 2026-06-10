import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PostResponse, CreatePostRequest, UpdatePostRequest, PostsFeedResponse,
  CommentResponse, CreateCommentRequest, UpdateCommentRequest,
} from '../models/news.models';

export interface UploadMediaResponse {
  mediaId: string;
  mediaUrl: string;
  thumbnailUrl?: string;
  mediaType: 'image' | 'video';
  imageWidth?: number;
  imageHeight?: number;
  videoDurationSeconds?: number;
}

@Injectable({ providedIn: 'root' })
export class NewsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ── Posts ──────────────────────────────────────────────────────────────────

  getFeed(page = 1, pageSize = 20): Observable<PostsFeedResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PostsFeedResponse>(`${this.baseUrl}/api/news`, { params });
  }

  createPost(request: CreatePostRequest): Observable<PostResponse> {
    return this.http.post<PostResponse>(`${this.baseUrl}/api/news`, request);
  }

  updatePost(postId: string, request: UpdatePostRequest): Observable<PostResponse> {
    return this.http.put<PostResponse>(`${this.baseUrl}/api/news/${postId}`, request);
  }

  deletePost(postId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/news/${postId}`);
  }

  approvePost(postId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/api/news/${postId}/approve`, {});
  }

  toggleLike(postId: string): Observable<{ likeCount: number; isLiked: boolean }> {
    return this.http.post<{ likeCount: number; isLiked: boolean }>(
      `${this.baseUrl}/api/news/${postId}/like`, {});
  }

  uploadMedia(formData: FormData, groupSlug?: string): Observable<UploadMediaResponse> {
    let params = new HttpParams();
    if (groupSlug) {
      params = params.set('groupSlug', groupSlug);
    }
    return this.http.post<UploadMediaResponse>(`${this.baseUrl}/api/news/upload`, formData, { params });
  }

  // ── Comments ───────────────────────────────────────────────────────────────

  getComments(postId: string): Observable<CommentResponse[]> {
    return this.http.get<CommentResponse[]>(`${this.baseUrl}/api/news/${postId}/comments`);
  }

  createComment(postId: string, request: CreateCommentRequest): Observable<CommentResponse> {
    return this.http.post<CommentResponse>(`${this.baseUrl}/api/news/${postId}/comments`, request);
  }

  updateComment(postId: string, commentId: string, request: UpdateCommentRequest): Observable<CommentResponse> {
    return this.http.put<CommentResponse>(`${this.baseUrl}/api/news/${postId}/comments/${commentId}`, request);
  }

  deleteComment(postId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/news/${postId}/comments/${commentId}`);
  }
}
