import { Component, OnInit, OnDestroy, inject, ElementRef, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

import { PostCardComponent } from '../post-card/post-card.component';
import {
  PostFormModalComponent,
  PostFormModalData,
  PostFormModalResult,
} from '../post-form-modal/post-form-modal.component';
import { NewsService } from '@app/core/services/news.service';
import { GroupService } from '@app/core/services/group.service';
import { AuthService } from '@app/core/services/auth.service';
import { PostResponse } from '@app/core/models/news.models';
import { GroupResponse } from '@app/core/models/group.models';

@Component({
  selector: 'app-news',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatProgressSpinnerModule,
    MatIconModule,
    PostCardComponent,
  ],
  templateUrl: './news.component.html',
  styleUrl: './news.component.scss',
  encapsulation: ViewEncapsulation.None,
})
export class NewsComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly newsService = inject(NewsService);
  private readonly groupService = inject(GroupService);
  private readonly authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly elRef = inject(ElementRef);
  private readonly destroy$ = new Subject<void>();

  // Feed state
  posts: PostResponse[] = [];
  loading = false;
  loadingMore = false;
  hasMore = true;
  error = '';
  page = 1;
  readonly pageSize = 20;

  // User/admin state
  currentUserId = '';
  myGroups: GroupResponse[] = [];
  /** Slugs of groups where the current user is an admin. */
  adminGroupSlugs = new Set<string>();

  private observer?: IntersectionObserver;

  ngOnInit(): void {
    this.currentUserId = this.authService.currentUser?.userId ?? '';

    forkJoin({
      groups: this.groupService.getMyGroups().pipe(catchError(() => of([]))),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ groups }) => {
        this.myGroups = groups;
        this.adminGroupSlugs = new Set(
          groups.filter(g => g.userRole === 'GroupAdmin').map(g => g.slug)
        );
        this.loadFeed();
      });
  }

  ngAfterViewInit(): void {
    this.setupIntersectionObserver();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.observer?.disconnect();
  }

  private setupIntersectionObserver(): void {
    const sentinel = this.elRef.nativeElement.querySelector('#feed-sentinel');
    if (!sentinel) return;

    this.observer = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting && !this.loading && !this.loadingMore && this.hasMore) {
        this.loadMore();
      }
    }, { rootMargin: '200px' });

    this.observer.observe(sentinel);
  }

  loadFeed(): void {
    if (this.loading) return;
    this.loading = true;
    this.error = '';
    this.page = 1;

    this.newsService.getFeed(1, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.posts = result.items;
          this.hasMore = result.items.length >= this.pageSize && result.items.length < result.totalCount;
          this.loading = false;
        },
        error: () => {
          this.error = 'Could not load posts. Please try again.';
          this.loading = false;
        },
      });
  }

  loadMore(): void {
    if (this.loadingMore || !this.hasMore) return;
    this.loadingMore = true;
    this.page++;

    this.newsService.getFeed(this.page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: result => {
          this.posts = [...this.posts, ...result.items];
          this.hasMore = this.posts.length < result.totalCount;
          this.loadingMore = false;
        },
        error: () => {
          this.loadingMore = false;
          this.page--;
        },
      });
  }

  openNewPost(): void {
    const data: PostFormModalData = {
      groups: this.myGroups.map(g => ({ name: g.name, slug: g.slug })),
    };

    this.dialog.open<PostFormModalComponent, PostFormModalData, PostFormModalResult>(
      PostFormModalComponent,
      { data, width: '560px', maxWidth: '96vw', panelClass: 'sn-dialog' }
    ).afterClosed().subscribe(result => {
      if (result?.post) {
        this.posts = [result.post, ...this.posts];
      }
    });
  }

  openEditPost(post: PostResponse): void {
    const data: PostFormModalData = {
      post,
      groups: this.myGroups.map(g => ({ name: g.name, slug: g.slug })),
    };

    this.dialog.open<PostFormModalComponent, PostFormModalData, PostFormModalResult>(
      PostFormModalComponent,
      { data, width: '560px', maxWidth: '96vw', panelClass: 'sn-dialog' }
    ).afterClosed().subscribe(result => {
      if (result?.post) {
        this.posts = this.posts.map(p => p.id === result.post.id ? result.post : p);
      }
    });
  }

  deletePost(post: PostResponse): void {
    if (!confirm(`Delete post "${post.subject}"?`)) return;

    this.newsService.deletePost(post.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.posts = this.posts.filter(p => p.id !== post.id); },
        error: () => alert('Could not delete post. Please try again.'),
      });
  }

  approvePost(post: PostResponse): void {
    this.newsService.approvePost(post.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.posts = this.posts.map(p =>
            p.id === post.id ? { ...p, isApproved: true, needsApproval: false } : p
          );
        },
        error: () => alert('Could not approve post. Please try again.'),
      });
  }

  isAdminForPost(post: PostResponse): boolean {
    return this.adminGroupSlugs.has(post.groupSlug) || this.authService.isSuperAdmin;
  }

  trackById(_: number, post: PostResponse): string {
    return post.id;
  }
}
