import { Routes } from '@angular/router';
import { authGuard, superAdminGuard } from './core/guards/auth.guard';
import { groupMemberGuard } from './core/guards/group.guards';

export const routes: Routes = [
  // ── Public ──────────────────────────────────────────────────────────────────
  {
    path: '',
    loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'group/:slug',
    loadComponent: () => import('./features/groups/group-public/group-public.component').then(m => m.GroupPublicComponent)
  },
  {
    // Public meeting finder — no auth required (T11/T12 reviewed, meeting schedules only)
    path: 'meetings',
    loadComponent: () => import('./features/meetings/meeting-finder/meeting-finder.component').then(m => m.MeetingFinderComponent)
  },

  // ── Auth ─────────────────────────────────────────────────────────────────────
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
  },

  // ── Authenticated ─────────────────────────────────────────────────────────────
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/groups/groups-dashboard/groups-dashboard.component').then(m => m.GroupsDashboardComponent)
  },
  // Legacy sub-routes → hub (guards run on the destination groups/:slug route)
  { path: 'groups/:slug/members',       redirectTo: 'groups/:slug', pathMatch: 'full' },
  { path: 'groups/:slug/phone-list',    redirectTo: 'groups/:slug', pathMatch: 'full' },
  { path: 'groups/:slug/join-requests', redirectTo: 'groups/:slug', pathMatch: 'full' },
  { path: 'groups/:slug/meetings',      redirectTo: 'groups/:slug', pathMatch: 'full' },
  { path: 'groups/:slug/settings',      redirectTo: 'groups/:slug', pathMatch: 'full' },
  {
    path: 'groups/:slug',
    canActivate: [authGuard, groupMemberGuard],
    loadComponent: () => import('./features/groups/group-hub/group-hub.component').then(m => m.GroupHubComponent)
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/members/my-profile/my-profile.component').then(m => m.MyProfileComponent)
  },
  {
    path: 'profile/:userId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/members/member-detail/member-detail.component').then(m => m.MemberDetailComponent)
  },

  // ── SuperAdmin ────────────────────────────────────────────────────────────────
  {
    path: 'admin',
    canActivate: [authGuard, superAdminGuard],
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.adminRoutes)
  },

  // ── Fallback ──────────────────────────────────────────────────────────────────
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];
