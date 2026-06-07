import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { BehaviorSubject, of } from 'rxjs';

import { NavbarComponent } from './navbar.component';
import { AuthService } from '@app/core/services/auth.service';
import { GroupService } from '@app/core/services/group.service';
import { CurrentUser } from '@app/core/models';

function makeUser(userId = 'u1'): CurrentUser {
  return {
    userId,
    displayName: 'Jane Doe',
    email: 'jane@example.com',
    isSuperAdmin: false,
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAt: new Date(Date.now() + 3_600_000),
  };
}

describe('NavbarComponent — account dropdown', () => {
  let fixture: ComponentFixture<NavbarComponent>;
  let component: NavbarComponent;
  let currentUser$: BehaviorSubject<CurrentUser | null>;
  let logoutSpy: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    // jsdom lacks IntersectionObserver, which the scroll-spy uses.
    (globalThis as unknown as { IntersectionObserver: unknown }).IntersectionObserver =
      class {
        observe(): void {}
        unobserve(): void {}
        disconnect(): void {}
      };

    currentUser$ = new BehaviorSubject<CurrentUser | null>(null);
    logoutSpy = vi.fn();

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { currentUser$: currentUser$.asObservable(), logout: logoutSpy } },
        { provide: GroupService, useValue: { getMyGroups: () => of([]) } },
        { provide: MatDialog, useValue: { open: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  function el(selector: string): HTMLElement | null {
    return fixture.nativeElement.querySelector(selector);
  }

  it('renders the user pill (not the Sign In CTA) when logged in', () => {
    currentUser$.next(makeUser());
    fixture.detectChanges();

    expect(el('.user-pill')).toBeTruthy();
    expect(el('.nav-cta')).toBeNull();
  });

  it('opens the account dropdown and keeps it rendered', () => {
    currentUser$.next(makeUser());
    fixture.detectChanges();

    expect(el('.user-dropdown')).toBeNull();

    component.toggleUserMenu(new MouseEvent('click'));
    fixture.detectChanges();

    expect(component.userMenuOpen).toBe(true);
    const dropdown = el('.user-dropdown');
    expect(dropdown).toBeTruthy();
    expect(el('.dd-name')?.textContent).toContain('Jane Doe');
    expect(el('.dd-email')?.textContent).toContain('jane@example.com');
  });

  it('closes the dropdown on an outside (document) click', () => {
    currentUser$.next(makeUser());
    component.toggleUserMenu(new MouseEvent('click'));
    fixture.detectChanges();
    expect(component.userMenuOpen).toBe(true);

    component.onDocumentClick();
    fixture.detectChanges();

    expect(component.userMenuOpen).toBe(false);
    expect(el('.user-dropdown')).toBeNull();
  });

  it('keeps the user reference stable when the same identity is re-emitted (token refresh)', () => {
    const first = makeUser('u1');
    currentUser$.next(first);
    fixture.detectChanges();
    expect(component.user).toBe(first);

    // A token refresh emits a brand-new object with the SAME userId.
    const refreshed = makeUser('u1');
    currentUser$.next(refreshed);
    fixture.detectChanges();

    // distinctUntilChanged(userId) must drop it, so the logged-in view (and its
    // dropdown trigger) is never torn down — this is what caused the flicker.
    expect(component.user).toBe(first);

    // The dropdown still opens normally afterwards.
    component.toggleUserMenu(new MouseEvent('click'));
    fixture.detectChanges();
    expect(el('.user-dropdown')).toBeTruthy();
  });

  it('closes any open dropdown and logs out when Sign Out is used', () => {
    currentUser$.next(makeUser());
    component.toggleUserMenu(new MouseEvent('click'));
    fixture.detectChanges();

    component.logout();

    expect(logoutSpy).toHaveBeenCalledTimes(1);
    expect(component.userMenuOpen).toBe(false);
  });

  it('clears open menus when the user signs out / session ends', () => {
    currentUser$.next(makeUser());
    component.toggleUserMenu(new MouseEvent('click'));
    fixture.detectChanges();
    expect(component.userMenuOpen).toBe(true);

    currentUser$.next(null);
    fixture.detectChanges();

    expect(component.userMenuOpen).toBe(false);
    expect(el('.user-pill')).toBeNull();
  });
});
