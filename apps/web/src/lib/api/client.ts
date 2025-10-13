import { tokenService } from '../auth/token';

const API_BASE_URL = 'https://localhost:7103/api';

export const apiClient = {
  /**
   * Fetch wrapper that adds Authorization header if token exists
   */
  async fetch(endpoint: string, options?: RequestInit) {
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

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    // If 401 Unauthorized, the token is invalid or expired
    if (response.status === 401) {
      tokenService.removeToken();
      // Trigger an event to notify the application
      if (typeof window !== 'undefined') {
        window.dispatchEvent(new Event('auth:unauthorized'));
      }
      throw new Error('Unauthorized');
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        errorData.message || `API request failed with status ${response.status}`
      );
    }

    return response.json();
  },
};
