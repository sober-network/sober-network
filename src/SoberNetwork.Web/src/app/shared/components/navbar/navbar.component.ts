import { Component, inject, AfterViewInit, OnDestroy, NgZone, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { NavigationEnd } from '@angular/router';
import { LoginModalComponent } from '../login-modal/login-modal.component';
import { catchError, combineLatest, debounceTime, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { NotificationService } from '@app/core/services/notification.service';
import { CurrentUser, GroupResponse } from '@app/core/models';
import { NotificationResponse } from '@app/core/models/notification.models';
import { CreateGroupModalComponent } from '@app/features/groups/create-group-modal/create-group-modal.component';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements AfterViewInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  readonly notificationService = inject(NotificationService);
  readonly router = inject(Router);
  private readonly zone = inject(NgZone);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);

  user: CurrentUser | null = null;
  myGroups: GroupResponse[] = [];
  activeAnchor: string | null = null;

  userMenuOpen = false;
  groupsMenuOpen = false;
  notificationsMenuOpen = false;

  notifications: NotificationResponse[] = [];
  notificationsLoading = false;

  private sectionObserver: IntersectionObserver | null = null;
  // Guest landing sections that have a matching nav pill (scroll-spy targets).
  // 'how-it-works' still exists as a section but has no pill, so it is intentionally
  // not observed — 'features' stays highlighted contiguously until 'traditions'.
  private readonly sectionIds = ['about', 'features', 'traditions'];

  constructor() {
    // Identity-stable: only re-assign `user` when the signed-in identity actually changes,
    // not on every token-refresh re-emission. This keeps the logged-in template (and its
    // dropdown triggers) from being torn down when the same user is re-emitted.
    this.auth.currentUser$
      .pipe(distinctUntilChanged((a, b) => a?.userId === b?.userId), takeUntilDestroyed())
      .subscribe(u => {
        this.user = u;
        if (!u) this.closeMenus();
      });

    const nav$ = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      startWith(null),
    );

    combineLatest([this.auth.currentUser$.pipe(distinctUntilChanged((a, b) => a?.userId === b?.userId)), nav$]).pipe(
      map(([user]) => user),
      debounceTime(100), // Coalesce rapid emissions (e.g. auth restore + NavigationEnd) to avoid cancelling in-flight requests
      switchMap(user => user ? this.groupService.getMyGroups().pipe(catchError(() => of([]))) : of([])),
      takeUntilDestroyed(),
    ).subscribe(groups => (this.myGroups = groups));

    // Re-init scroll-spy on each navigation (sections appear after router-outlet renders)
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntilDestroyed(),
    ).subscribe(() => {
      this.closeMenus();
      this.teardownScrollSpy();
      if (this.router.url === '/') {
        setTimeout(() => this.setupScrollSpy(), 150);
      } else {
        this.activeAnchor = null;
      }
    });
  }

  ngAfterViewInit(): void {
    if (this.router.url === '/') {
      setTimeout(() => this.setupScrollSpy(), 150);
    }
  }

  ngOnDestroy(): void {
    this.teardownScrollSpy();
  }

  navigateToGroup(slug: string): void {
    this.closeMenus();
    this.router.navigate(['/groups', slug]);
  }

  openCreateGroupDialog(): void {
    this.closeMenus();
    this.dialog.open(CreateGroupModalComponent, {
      panelClass: ['sn-modal-panel', 'sn-create-group-panel'],
      maxWidth: '100vw',
      autoFocus: 'first-tabbable',
    }).afterClosed().subscribe({
      next: created => {
        if (created) {
          this.groupService.getMyGroups().pipe(catchError(() => of([]))).subscribe(groups => {
            this.myGroups = groups;
          });
        }
      },
    });
  }

  logout(): void {
    this.closeMenus();
    this.auth.logout();
  }

  /** Toggle the account dropdown. stopPropagation keeps the document:click handler from closing it instantly. */
  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.groupsMenuOpen = false;
    this.userMenuOpen = !this.userMenuOpen;
  }

  toggleGroupsMenu(event: Event): void {
    event.stopPropagation();
    this.userMenuOpen = false;
    this.groupsMenuOpen = !this.groupsMenuOpen;
  }

  closeMenus(): void {
    this.userMenuOpen = false;
    this.groupsMenuOpen = false;
    this.notificationsMenuOpen = false;
  }

  toggleNotificationsMenu(event: Event): void {
    event.stopPropagation();
    const opening = !this.notificationsMenuOpen;
    this.userMenuOpen = false;
    this.groupsMenuOpen = false;
    this.notificationsMenuOpen = opening;

    if (opening) {
      this.notificationsLoading = true;
      this.notificationService.getNotifications().subscribe({
        next: items => {
          this.notifications = items.slice(0, 10);
          this.notificationsLoading = false;
        },
        error: () => { this.notificationsLoading = false; },
      });
    }
  }

  navigateToNotifications(): void {
    this.closeMenus();
    this.router.navigate(['/news'], { queryParams: { filter: 'notifications' } });
    this.notificationService.markAllRead().subscribe({
      next: () => this.notificationService.fetchUnreadCount(),
      error: () => {},
    });
  }

  openNotificationPost(n: NotificationResponse): void {
    if (!n.postId) {
      this.navigateToNotifications();
      return;
    }
    this.closeMenus();
    this.notificationService.markAllRead().subscribe({
      next: () => this.notificationService.fetchUnreadCount(),
      error: () => {},
    });
    import('@app/features/news/image-viewer-modal/image-viewer-modal.component')
      .then(({ ImageViewerModalComponent }) => {
        this.notificationService.getNotificationPosts().subscribe({
          next: posts => {
            const post = posts.find(p => p.id === n.postId);
            if (!post) { this.router.navigate(['/news']); return; }
            const mediaUrl = post.mediaUrl
              ? (environment.apiUrl + post.mediaUrl)
              : null;
            this.dialog.open(ImageViewerModalComponent, {
              data: {
                imageUrl: mediaUrl,
                post,
                currentUserId: this.user?.userId ?? '',
                isGroupAdmin: false,
              },
              width: post.mediaUrl ? '90vw' : '600px',
              maxWidth: post.mediaUrl ? '1100px' : '96vw',
              height: '85vh',
              maxHeight: '90vh',
              panelClass: 'sn-image-viewer-panel',
            });
          },
          error: () => this.router.navigate(['/news']),
        });
      });
  }

  getNotificationLabel(n: NotificationResponse): string {
    if (n.type === 'CommentOnMyPost') return `${n.triggerUserDisplayName} commented on your post`;
    if (n.type === 'LikedMyPost') return `${n.triggerUserDisplayName} liked your post`;
    return `${n.triggerUserDisplayName} replied to your comment`;
  }

  isGroupRouteActive(): boolean {
    return this.router.url.startsWith('/groups');
  }

  trackBySlug(_: number, group: GroupResponse): string {
    return group.slug;
  }

  getGroupIconColor(index: number): string {
    const colors = ['violet', 'rose', 'sky', 'amber', 'green', 'teal', 'indigo', 'coral'];
    return colors[index % colors.length];
  }

  getUserAvatarBackground(displayName: string): string {
    const colors = [
      'var(--sn-tint-violet, hsla(260,70%,65%,0.12))',
      'var(--sn-tint-rose, hsla(345,75%,65%,0.12))',
      'var(--sn-tint-sky, hsla(205,85%,60%,0.12))',
      'var(--sn-tint-amber, hsla(38,90%,58%,0.12))',
      'var(--sn-tint-green, hsla(148,58%,52%,0.12))',
      'var(--sn-tint-teal, hsla(183,62%,52%,0.12))',
      'var(--sn-tint-indigo, hsla(230,70%,62%,0.12))',
      'var(--sn-tint-coral, hsla(18,82%,62%,0.12))',
    ];
    const seed = displayName.trim().charCodeAt(0) || 0;
    return colors[seed % colors.length];
  }

  groupMeta(group: GroupResponse): string {
    return `${group.memberCount} member${group.memberCount === 1 ? '' : 's'}`;
  }

  /** Any click that bubbles to the document (i.e. outside an open dropdown) closes the menus. */
  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.userMenuOpen || this.groupsMenuOpen) this.closeMenus();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeMenus();
  }

  openSignIn(): void {
    this.dialog.open(LoginModalComponent, {
      panelClass: ['sn-modal-panel', 'sn-login-panel'],
      maxWidth:   '100vw',
      autoFocus:  'first-tabbable',
    });
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private setupScrollSpy(): void {
    if (typeof window === 'undefined') return;

    this.sectionObserver = new IntersectionObserver(
      (entries) => {
        this.zone.run(() => {
          for (const entry of entries) {
            if (entry.isIntersecting) {
              this.activeAnchor = entry.target.id;
              this.cdr.markForCheck();
            }
          }
        });
      },
      { rootMargin: '-10% 0px -55% 0px', threshold: 0 },
    );

    for (const id of this.sectionIds) {
      const el = document.getElementById(id);
      if (el) this.sectionObserver.observe(el);
    }
  }

  private teardownScrollSpy(): void {
    this.sectionObserver?.disconnect();
    this.sectionObserver = null;
  }
}
