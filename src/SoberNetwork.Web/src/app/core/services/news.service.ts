import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PostResponse, CreatePostRequest, UpdatePostRequest, PostsFeedResponse } from '../models/news.models';

@Injectable({ providedIn: 'root' })
export class NewsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

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
}
