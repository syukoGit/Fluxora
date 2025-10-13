import { tokenService } from '../auth/token';
import type { KeycloakTokenResponse } from '../auth/types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
if (!API_BASE_URL) {
  throw new Error(
    'NEXT_PUBLIC_API_BASE_URL environment variable must be set. Refusing to use a default localhost URL in production.'
  );
}
// Flag to prevent multiple simultaneous refresh attempts
let isRefreshing = false;
let refreshPromise: Promise<void> | null = null;

export const apiClient = {
  /**
   * Refresh the access token
   */
  async refreshAccessToken(): Promise<void> {
    const refreshToken = tokenService.getRefreshToken();

    if (!refreshToken || tokenService.isRefreshTokenExpired()) {
      // Refresh token is missing or expired, clear all tokens
      tokenService.removeToken();
      if (typeof window !== 'undefined') {
        window.dispatchEvent(new Event('auth:unauthorized'));
      }
      throw new Error('Refresh token is invalid or expired');
    }

    try {
      const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) {
        throw new Error('Failed to refresh token');
      }

      const data: KeycloakTokenResponse = await response.json();

      // Save new tokens with expiry information
      if (data.access_token) {
        tokenService.setToken(data.access_token, data.expires_in);
      }
      if (data.refresh_token) {
        tokenService.setRefreshToken(
          data.refresh_token,
          data.refresh_expires_in
        );
      }
    } catch (error) {
      // If refresh fails, clear all tokens
      tokenService.removeToken();
      if (typeof window !== 'undefined') {
        window.dispatchEvent(new Event('auth:unauthorized'));
      }
      throw error;
    }
  },

  /**
   * Fetch wrapper that adds Authorization header if token exists
   * Automatically refreshes token if expired
   */
  async fetch(endpoint: string, options?: RequestInit) {
    // Skip token refresh for auth endpoints to avoid circular dependencies
    const isAuthEndpoint =
      endpoint.includes('/auth/login') ||
      endpoint.includes('/auth/register') ||
      endpoint.includes('/auth/refresh');

    // Check if token needs refresh before making the request
    if (!isAuthEndpoint && tokenService.isAccessTokenExpired()) {
      if (!tokenService.isRefreshTokenExpired()) {
        // Token is expired but refresh token is still valid, refresh it
        if (isRefreshing) {
          // Wait for ongoing refresh to complete
          await refreshPromise;
        } else {
          isRefreshing = true;
          refreshPromise = this.refreshAccessToken().finally(() => {
            isRefreshing = false;
            refreshPromise = null;
          });
          await refreshPromise;
        }
      } else {
        // Both tokens are expired, clear and notify
        tokenService.removeToken();
        if (typeof window !== 'undefined') {
          window.dispatchEvent(new Event('auth:unauthorized'));
        }
        throw new Error('Session expired');
      }
    }

    const token = tokenService.getToken();

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    // Add custom headers if provided
    if (options?.headers) {
      Object.entries(options.headers).forEach(([key, value]) => {
        if (typeof value === 'string') {
          headers[key] = value;
        }
      });
    }

    // Add Bearer token if available
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    let response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    // If 401 Unauthorized, try to refresh the token and retry once
    if (response.status === 401 && !isAuthEndpoint) {
      if (!tokenService.isRefreshTokenExpired()) {
        try {
          // Attempt to refresh the token
          if (isRefreshing) {
            await refreshPromise;
          } else {
            isRefreshing = true;
            refreshPromise = this.refreshAccessToken().finally(() => {
              isRefreshing = false;
              refreshPromise = null;
            });
            await refreshPromise;
          }

          // Retry the original request with the new token
          const newToken = tokenService.getToken();
          if (newToken) {
            headers['Authorization'] = `Bearer ${newToken}`;
          }

          response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers,
          });
        } catch (error) {
          // Refresh failed, clear tokens and notify
          tokenService.removeToken();
          if (typeof window !== 'undefined') {
            window.dispatchEvent(new Event('auth:unauthorized'));
          }
          throw new Error('Unauthorized');
        }
      } else {
        // Refresh token is expired, clear tokens and notify
        tokenService.removeToken();
        if (typeof window !== 'undefined') {
          window.dispatchEvent(new Event('auth:unauthorized'));
        }
        throw new Error('Unauthorized');
      }
    }

    if (!response.ok) {
      const errorData = await response.json().catch((parseError) => {
        // Log the parsing error for debugging
        if (typeof console !== 'undefined' && console.error) {
          console.error('Failed to parse error response JSON:', parseError);
        }
        return {
          error: 'Failed to parse response JSON',
          parseError: parseError instanceof Error ? parseError.message : String(parseError),
        };
      });
      throw new Error(
        errorData.message ||
        errorData.error ||
        `API request failed with status ${response.status}`
      );
    }

    return response.json();
  },
};
