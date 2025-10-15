import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import {
  isPublicRoute,
  findRouteConfig,
  hasRequiredRoles,
} from './lib/auth/route-config';
import { extractRoles, isTokenExpired } from './lib/auth/jwt-edge';

/**
 * Next.js Middleware for route protection
 *
 * This middleware runs BEFORE page rendering to:
 * - Block access to protected routes if not authenticated
 * - Check role requirements for protected routes
 * - Redirect to /login or /forbidden when necessary
 */

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Ignore static files and assets
  if (
    pathname.startsWith('/_next') ||
    pathname.startsWith('/api') ||
    pathname.includes('.') // files with extension (css, js, images, etc.)
  ) {
    return NextResponse.next();
  }

  // If route is public, allow access
  if (isPublicRoute(pathname)) {
    return NextResponse.next();
  }

  // Get authentication token from cookie (set during login)
  const authToken = request.cookies.get('fluxora_auth_token')?.value;

  // Find route configuration
  const routeConfig = findRouteConfig(pathname);

  // If route is protected but no config found, it's a catch-all authenticated route
  if (!routeConfig && !isPublicRoute(pathname)) {
    // Require authentication for undefined routes (default behavior)
    if (!authToken) {
      const loginUrl = new URL('/login', request.url);
      loginUrl.searchParams.set('redirect', pathname);
      return NextResponse.redirect(loginUrl);
    }
    return NextResponse.next();
  }

  // If route has config, it's protected
  if (routeConfig) {
    // Check authentication
    if (!authToken) {
      const loginUrl = new URL('/login', request.url);
      loginUrl.searchParams.set('redirect', pathname);
      return NextResponse.redirect(loginUrl);
    }

    // Check if token is expired
    if (isTokenExpired(authToken)) {
      const loginUrl = new URL('/login', request.url);
      loginUrl.searchParams.set('redirect', pathname);
      loginUrl.searchParams.set('reason', 'expired');
      return NextResponse.redirect(loginUrl);
    }

    // Extract user roles from token
    const userRoles = extractRoles(authToken);

    // Check role requirements
    if (!hasRequiredRoles(userRoles, routeConfig)) {
      // User is authenticated but doesn't have required roles
      const forbiddenUrl = new URL('/forbidden', request.url);
      forbiddenUrl.searchParams.set('route', pathname);
      return NextResponse.redirect(forbiddenUrl);
    }
  }

  // Allow access
  return NextResponse.next();
}

/**
 * Matcher configuration
 * Defines which routes the middleware should run on
 */
export const config = {
  matcher: [
    /*
     * Match all routes except:
     * - /api (API routes)
     * - /_next/static (static files)
     * - /_next/image (image optimization)
     * - /favicon.ico, /sitemap.xml, etc.
     */
    '/((?!api|_next/static|_next/image|favicon.ico|.*\\..*$).*)',
  ],
};
