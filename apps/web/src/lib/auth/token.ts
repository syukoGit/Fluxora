/**
 * Service de gestion des tokens JWT
 * Gère la persistance et la validation des tokens
 */

import { User } from './types';

const TOKEN_KEY = 'fluxora_auth_token';
const REFRESH_TOKEN_KEY = 'fluxora_refresh_token';

const BUFFER_TIME = 5 * 60 * 1000;

export interface DecodedToken {
  sub: string; // User ID
  email: string;
  name?: string;
  exp: number; // Expiration timestamp
  iat: number; // Issued at timestamp
  roles?: string[];
}

export const tokenService = {
  /**
   * Save the JWT token to localStorage
   */
  setToken(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(TOKEN_KEY, token);
    }
  },

  /**
   * Retrieve the token from localStorage
   */
  getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(TOKEN_KEY);
    }
    return null;
  },

  /**
   * Remove the token from localStorage
   */
  removeToken(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    }
  },

  /**
   * Save the refresh token
   */
  setRefreshToken(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(REFRESH_TOKEN_KEY, token);
    }
  },

  /**
   * Retrieve the refresh token
   */
  getRefreshToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(REFRESH_TOKEN_KEY);
    }
    return null;
  },

  /**
   * Decode a JWT token without verifying the signature
   * (The token's integrity should be verified by the server)
   */
  decodeToken(token: string): DecodedToken | null {
    try {
      const base64Url = token.split('.')[1];
      if (!base64Url) return null;

      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  },

  /**
   * Check if the token is expired (with a buffer time)
   */
  isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded) return true;

    // Check if the token expires in less than 5 minutes
    const expirationTime = decoded.exp * 1000;
    const currentTime = Date.now();

    return expirationTime - currentTime < BUFFER_TIME;
  },

  /**
   * Check if the token is valid (exists and is not expired)
   */
  isTokenValid(): boolean {
    const token = this.getToken();
    return !token ? false : !this.isTokenExpired(token);
  },

  /**
   * Retrieve the user information from the token
   */
  getUserFromToken(): User | null {
    const token = this.getToken();
    if (!token) return null;

    if (this.isTokenExpired(token)) {
      this.removeToken();
      console.log('Token expired');
      return null;
    }

    const decodedToken = this.decodeToken(token);
    if (!decodedToken) {
      console.log('Unable to decode the token');
      return null;
    }

    return {
      id: decodedToken.sub,
      email: decodedToken.email,
      ...(decodedToken.name && { name: decodedToken.name }),
      ...(decodedToken.roles && { roles: decodedToken.roles }),
    };
  },
};
