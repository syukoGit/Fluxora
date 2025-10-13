import { apiClient } from './client';
import { tokenService } from '../auth/token';
import type {
  AuthResponse,
  LoginCredentials,
  RegisterCredentials,
} from '../auth/types';

export const authApi = {
  /**
   * Login user with username and password
   */
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await apiClient.fetch('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });

    // Save tokens
    if (response.access_token) {
      tokenService.setToken(response.access_token);
    }

    if (response.refresh_token) {
      tokenService.setRefreshToken(response.refresh_token);
    }

    const token = tokenService.getToken();
    const refreshToken = tokenService.getRefreshToken();
    const user = tokenService.getUserFromToken();

    if (!token || !refreshToken || !user) {
      throw new Error('Failed to retrieve authentication tokens');
    }

    return {
      token,
      refreshToken,
      user,
    };
  },

  /**
   * Logout current user
   */
  async logout(): Promise<void> {
    try {
      await apiClient.fetch('/auth/logout', {
        method: 'POST',
        body: JSON.stringify({ refreshToken: tokenService.getRefreshToken() }),
      });
    } finally {
      // Clear tokens regardless of API call success
      tokenService.removeToken();
    }
  },

  /**
   * Register a new user
   */
  async register(credentials: RegisterCredentials): Promise<AuthResponse> {
    const response = await apiClient.fetch('/auth/register', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });

    // Save tokens
    if (response.access_token) {
      tokenService.setToken(response.access_token);
    }
    if (response.refresh_token) {
      tokenService.setRefreshToken(response.refresh_token);
    }

    const token = tokenService.getToken();
    const refreshToken = tokenService.getRefreshToken();
    const user = tokenService.getUserFromToken();

    if (!token || !refreshToken || !user) {
      throw new Error('Failed to retrieve authentication tokens');
    }

    return {
      token,
      refreshToken,
      user,
    };
  },

  /**
   * Refresh the access token using the refresh token
   */
  async refreshToken(): Promise<AuthResponse> {
    const refreshToken = tokenService.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await apiClient.fetch('/auth/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    });

    if (response.token) {
      tokenService.setToken(response.token);
    }

    return response;
  },

  /**
   * Retrieve the current authenticated user
   */
  async getCurrentUser() {
    return apiClient.fetch('/auth/me', {
      method: 'GET',
    });
  },
};
