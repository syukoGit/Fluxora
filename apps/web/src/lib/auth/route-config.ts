/**
 * Route protection configuration
 * Defines role-based access control for routes
 */

/**
 * Available user roles from Keycloak
 */
export const ROLES = {
  ADMIN: 'admin',
  USER: 'user',
  MODERATOR: 'moderator',
} as const;

export type Role = (typeof ROLES)[keyof typeof ROLES];

const ALL_ROLES: Role[] = Object.values(ROLES);

/**
 * Route configuration with role requirements
 * Routes not listed here are considered public
 */
export interface RouteConfig {
  path: string;
  roles?: Role[];
  requireAll?: boolean;
}

/**
 * Public routes - accessible without authentication
 */
export const PUBLIC_ROUTES = ['/', '/login', '/forbidden'];

/**
 * Protected routes with role requirements
 * Order matters: more specific routes should come first
 */
export const PROTECTED_ROUTES: RouteConfig[] = [
  {
    path: '/dashboard',
    roles: ALL_ROLES,
    requireAll: false,
  },
  {
    path: '/my-profile',
    roles: ALL_ROLES,
    requireAll: false,
  },
];

/**
 * Check if a route is public
 */
export function isPublicRoute(pathname: string): boolean {
  return PUBLIC_ROUTES.some(
    (route) => pathname === route || pathname.startsWith(`${route}/`)
  );
}

/**
 * Find route configuration for a given pathname
 * Returns the first matching route config (most specific)
 */
export function findRouteConfig(pathname: string): RouteConfig | null {
  return (
    PROTECTED_ROUTES.find(
      (route) =>
        pathname === route.path || pathname.startsWith(`${route.path}/`)
    ) || null
  );
}

/**
 * Check if user has required role(s) for a route
 */
export function hasRequiredRoles(
  userRoles: string[],
  routeConfig: RouteConfig
): boolean {
  // If no roles specified, any authenticated user can access
  if (!routeConfig.roles || routeConfig.roles.length === 0) {
    return true;
  }

  // Check if user has required roles
  if (routeConfig.requireAll) {
    // User must have ALL required roles
    return routeConfig.roles.every((role) => userRoles.includes(role));
  } else {
    // User must have at least ONE required role
    return routeConfig.roles.some((role) => userRoles.includes(role));
  }
}
