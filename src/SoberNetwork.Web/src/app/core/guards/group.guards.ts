import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { GroupService } from '@app/core/services/group.service';

/**
 * Verifies the current user is an Active member of the group identified by
 * the :slug route parameter before allowing access to member-only pages.
 */
export const groupMemberGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const groupService = inject(GroupService);
  const router = inject(Router);
  const slug = route.paramMap.get('slug') ?? '';

  return groupService.getGroup(slug).pipe(
    map(() => true), // 200 = accessible to this member
    catchError(() => of(router.createUrlTree(['/dashboard'])))
  );
};

/**
 * Verifies the current user holds the GroupAdmin role in the target group.
 * Relies on the API returning 403 for non-admins on admin-only endpoints.
 * Redirects to /groups/:slug on failure.
 */
export const groupAdminGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const groupService = inject(GroupService);
  const router = inject(Router);
  const slug = route.paramMap.get('slug') ?? '';

  // Attempt to load join requests — only admins can do this.
  // A 403 means the user is not an admin; redirect to group hub.
  return groupService.getJoinRequests(slug).pipe(
    map(() => true),
    catchError(() => of(router.createUrlTree(['/groups', slug])))
  );
};
