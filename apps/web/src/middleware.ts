import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

/**
 * Next.js Middleware for route protection
 *
 * This middleware runs BEFORE page rendering to:
 * - Block access to protected routes if not authenticated
 * - Redirect to /login when necessary
 */

// Public routes accessible without authentication
const PUBLIC_ROUTES = [
  '/login',
  '/register',
  '/unauthorized',
  '/forbidden',
  '/', // Public homepage
];

// Protected routes that require authentication
const PROTECTED_ROUTES = ['/dashboard', '/auth-test'];

/**
 * Check if a route is public
 */
function isPublicRoute(pathname: string): boolean {
  return PUBLIC_ROUTES.some(
    (route) => pathname === route || pathname.startsWith(`${route}/`)
  );
}

/**
 * Check if a route is protected
 */
function isProtectedRoute(pathname: string): boolean {
  return PROTECTED_ROUTES.some((route) => pathname.startsWith(route));
}

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

  // Get authentication token from cookie (set during login)
  const authToken = request.cookies.get('fluxora_auth_token')?.value;

  // If route is public, allow access
  if (isPublicRoute(pathname)) {
    return NextResponse.next();
  }

  // If route is protected and no token, redirect to /login
  if (isProtectedRoute(pathname) && !authToken) {
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('redirect', pathname); // Redirect after login
    return NextResponse.redirect(loginUrl);
  }

  // Allow access for all other routes
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
