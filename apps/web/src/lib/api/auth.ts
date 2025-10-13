import { apiClient } from './client';
import { tokenService } from '../auth/token';
import type {
  AuthResponse,
  LoginCredentials,
  RegisterCredentials,
  KeycloakTokenResponse,
} from '../auth/types';

export const authApi = {
  /**
   * Login user with username and password
   */
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response: KeycloakTokenResponse = await apiClient.fetch(
      '/auth/login',
      {
        method: 'POST',
        body: JSON.stringify(credentials),
      }
    );

    // Save tokens with expiry information
    tokenService.setToken(response.access_token, response.expires_in);
    tokenService.setRefreshToken(
      response.refresh_token,
      response.refresh_expires_in
    );

    return returnAuthResponse();
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
    const response: KeycloakTokenResponse = await apiClient.fetch(
      '/auth/register',
      {
        method: 'POST',
        body: JSON.stringify(credentials),
      }
    );

    // Save tokens with expiry information
    tokenService.setToken(response.access_token, response.expires_in);
    tokenService.setRefreshToken(
      response.refresh_token,
      response.refresh_expires_in
    );

    return returnAuthResponse();
  },

  /**
   * Refresh the access token using the refresh token
   */
  async refreshToken(): Promise<AuthResponse> {
    const refreshToken = tokenService.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response: KeycloakTokenResponse = await apiClient.fetch(
      '/auth/refresh',
      {
        method: 'POST',
        body: JSON.stringify({ refreshToken }),
      }
    );

    // Save new tokens with expiry information
    tokenService.setToken(response.access_token, response.expires_in);
    tokenService.setRefreshToken(
      response.refresh_token,
      response.refresh_expires_in
    );

    return returnAuthResponse();
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

function returnAuthResponse(): AuthResponse {
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
}
