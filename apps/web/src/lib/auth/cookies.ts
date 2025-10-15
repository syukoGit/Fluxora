/**
 * Cookie management utilities for authentication
 * Used to synchronize auth state between client-side and middleware
 */

const COOKIE_NAME = 'fluxora_auth_token';

/**
 * Set authentication cookie
 * This cookie is used by the middleware to protect routes
 */
export function setAuthCookie(token: string, expiresIn: number): void {
  if (typeof window === 'undefined') return;

  const maxAge = expiresIn; // in seconds
  const expires = new Date(Date.now() + expiresIn * 1000).toUTCString();

  // Create cookie with security settings
  const isProduction = typeof process !== 'undefined' && process.env && process.env.NODE_ENV === 'production';
  const secureFlag = isProduction ? '; Secure' : '';
  document.cookie = `${COOKIE_NAME}=${token}; path=/; max-age=${maxAge}; expires=${expires}; SameSite=Lax${secureFlag}`;
}

/**
 * Remove authentication cookie
 * Called during logout to clear middleware access
 */
export function removeAuthCookie(): void {
  if (typeof window === 'undefined') return;

  // Set cookie with expired date to remove it
  document.cookie = `${COOKIE_NAME}=; path=/; max-age=0; expires=Thu, 01 Jan 1970 00:00:00 GMT`;
}

/**
 * Get authentication cookie value
 * Mainly for debugging purposes (middleware reads it server-side)
 */
export function getAuthCookie(): string | null {
  if (typeof window === 'undefined') return null;

  const cookies = document.cookie.split(';');
  const authCookie = cookies.find((c) =>
    c.trim().startsWith(`${COOKIE_NAME}=`)
  );

  if (!authCookie) return null;

  return authCookie.split('=')[1] || null;
}
