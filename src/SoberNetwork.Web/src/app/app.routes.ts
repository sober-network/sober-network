import { Routes } from '@angular/router';
import { authGuard, superAdminGuard } from './core/guards/auth.guard';
import { groupMemberGuard, groupAdminGuard } from './core/guards/group.guards';

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
  {
    path: 'groups/:slug',
    canActivate: [authGuard, groupMemberGuard],
    loadComponent: () => import('./features/groups/group-hub/group-hub.component').then(m => m.GroupHubComponent)
  },
  {
    path: 'groups/:slug/members',
    canActivate: [authGuard, groupMemberGuard],
    loadComponent: () => import('./features/groups/group-members/group-members.component').then(m => m.GroupMembersComponent)
  },
  {
    path: 'groups/:slug/phone-list',
    canActivate: [authGuard, groupMemberGuard],
    loadComponent: () => import('./features/groups/group-phone-list/group-phone-list.component').then(m => m.GroupPhoneListComponent)
  },
  {
    path: 'groups/:slug/join-requests',
    canActivate: [authGuard, groupAdminGuard],
    loadComponent: () => import('./features/groups/group-join-requests/group-join-requests.component').then(m => m.GroupJoinRequestsComponent)
  },
  {
    path: 'groups/:slug/settings',
    canActivate: [authGuard, groupAdminGuard],
    loadComponent: () => import('./features/groups/group-settings/group-settings.component').then(m => m.GroupSettingsComponent)
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
