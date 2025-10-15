/**
 * JWT utilities for Edge Runtime (middleware)
 * These functions work in the Edge runtime without Node.js dependencies
 */

/**
 * Keycloak JWT token structure
 */
export interface KeycloakJWT {
  sub: string; // User ID
  email?: string;
  name?: string;
  preferred_username?: string;
  exp: number; // Expiration timestamp
  iat: number; // Issued at timestamp
  realm_access?: {
    roles: string[];
  };
}

/**
 * Decode JWT token without verifying signature
 * Used in middleware to extract user roles
 *
 * Note: Signature verification is done by the backend API
 * Middleware only needs to read claims for routing decisions
 */
export function decodeJWT(token: string): KeycloakJWT | null {
  try {
    // JWT format: header.payload.signature
    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }

    // Decode base64url payload
    const payload = parts[1];
    if (!payload) {
      return null;
    }

    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');

    // Decode base64 to string
    const jsonPayload = atob(base64);

    // Parse JSON
    return JSON.parse(jsonPayload) as KeycloakJWT;
  } catch (error) {
    console.error('Failed to decode JWT:', error);
    return null;
  }
}

/**
 * Extract user roles from Keycloak JWT token
 */
export function extractRoles(token: string): string[] {
  const decoded = decodeJWT(token);
  if (!decoded) {
    return [];
  }

  const roles: string[] = [];

  // Extract realm roles
  if (decoded.realm_access?.roles) {
    roles.push(...decoded.realm_access.roles);
  }

  return roles;
}

/**
 * Check if token is expired
 */
export function isTokenExpired(token: string): boolean {
  const decoded = decodeJWT(token);
  if (!decoded || !decoded.exp) {
    return true;
  }

  const currentTime = Math.floor(Date.now() / 1000);
  return currentTime >= decoded.exp;
}

/**
 * Get user ID from token
 */
export function getUserId(token: string): string | null {
  const decoded = decodeJWT(token);
  return decoded?.sub || null;
}
