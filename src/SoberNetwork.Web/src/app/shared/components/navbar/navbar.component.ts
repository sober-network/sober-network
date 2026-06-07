import { Component, inject, AfterViewInit, OnDestroy, NgZone, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { NavigationEnd } from '@angular/router';
import { LoginModalComponent } from '../login-modal/login-modal.component';
import { catchError, combineLatest, debounceTime, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { CurrentUser, GroupResponse } from '@app/core/models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements AfterViewInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly groupService = inject(GroupService);
  private readonly router = inject(Router);
  private readonly zone = inject(NgZone);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);

  user: CurrentUser | null = null;
  myGroups: GroupResponse[] = [];
  activeAnchor: string | null = null;

  // Plain in-DOM dropdowns (no Angular Material mat-menu / CDK overlay). The overlay
  // portal repeatedly detached from its trigger and flashed at the viewport origin
  // before closing; an in-template dropdown is fully under our control and immune to it.
  userMenuOpen = false;
  groupsMenuOpen = false;

  private sectionObserver: IntersectionObserver | null = null;
  private readonly sectionIds = ['about', 'features', 'how-it-works', 'principles', 'traditions'];

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
