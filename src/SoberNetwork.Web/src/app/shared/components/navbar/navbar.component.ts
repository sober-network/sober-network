import { Component, inject, AfterViewInit, OnDestroy, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { NavigationEnd } from '@angular/router';
import { LoginModalComponent } from '../login-modal/login-modal.component';
import { catchError, combineLatest, distinctUntilChanged, filter, map, Observable, of, startWith, switchMap } from 'rxjs';
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
    MatMenuModule,
    MatDividerModule,
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

  currentUser$: Observable<CurrentUser | null> = this.auth.currentUser$;
  myGroups: GroupResponse[] = [];
  activeAnchor: string | null = null;

  private sectionObserver: IntersectionObserver | null = null;
  private readonly sectionIds = ['about', 'features', 'how-it-works', 'principles', 'traditions'];

  constructor() {
    const nav$ = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      startWith(null),
    );

    combineLatest([this.auth.currentUser$.pipe(distinctUntilChanged((a, b) => a?.userId === b?.userId)), nav$]).pipe(
      map(([user]) => user),
      switchMap(user => user ? this.groupService.getMyGroups().pipe(catchError(() => of([]))) : of([])),
      takeUntilDestroyed(),
    ).subscribe(groups => (this.myGroups = groups));

    // Re-init scroll-spy on each navigation (sections appear after router-outlet renders)
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntilDestroyed(),
    ).subscribe(() => {
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
    this.router.navigate(['/groups', slug]);
  }

  logout(): void {
    this.auth.logout();
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
