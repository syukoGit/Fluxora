/**
 * JWT token management service
 * Handles the persistence and validation of tokens
 */

import { User } from './types';

const TOKEN_KEY = 'fluxora_auth_token';
const REFRESH_TOKEN_KEY = 'fluxora_refresh_token';
const TOKEN_EXPIRY_KEY = 'fluxora_token_expiry';
const REFRESH_TOKEN_EXPIRY_KEY = 'fluxora_refresh_token_expiry';

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
  setToken(token: string, expiresIn: number): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(TOKEN_KEY, token);
      const expiryTime = Date.now() + expiresIn * 1000;
      localStorage.setItem(TOKEN_EXPIRY_KEY, expiryTime.toString());
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
      localStorage.removeItem(TOKEN_EXPIRY_KEY);
      localStorage.removeItem(REFRESH_TOKEN_EXPIRY_KEY);
    }
  },

  /**
   * Save the refresh token
   */
  setRefreshToken(token: string, refreshExpiresIn: number): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(REFRESH_TOKEN_KEY, token);
      const expiryTime = Date.now() + refreshExpiresIn * 1000;
      localStorage.setItem(REFRESH_TOKEN_EXPIRY_KEY, expiryTime.toString());
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
   * Get token expiry timestamp
   */
  getTokenExpiry(): number | null {
    if (typeof window !== 'undefined') {
      const expiry = localStorage.getItem(TOKEN_EXPIRY_KEY);
      return expiry ? parseInt(expiry, 10) : null;
    }
    return null;
  },

  /**
   * Get refresh token expiry timestamp
   */
  getRefreshTokenExpiry(): number | null {
    if (typeof window !== 'undefined') {
      const expiry = localStorage.getItem(REFRESH_TOKEN_EXPIRY_KEY);
      return expiry ? parseInt(expiry, 10) : null;
    }
    return null;
  },

  /**
   * Check if access token is expired or will expire soon
   */
  isAccessTokenExpired(): boolean {
    const expiry = this.getTokenExpiry();
    if (!expiry) {
      // If no expiry is stored, fall back to JWT decoding
      const token = this.getToken();
      return token ? this.isTokenExpired(token) : true;
    }
    return Date.now() >= expiry;
  },

  /**
   * Check if refresh token is expired
   */
  isRefreshTokenExpired(): boolean {
    const expiry = this.getRefreshTokenExpiry();
    if (!expiry) return true;
    return Date.now() >= expiry;
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
   * Check if the token is expired
   */
  isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded) return true;

    const expirationTime = decoded.exp * 1000;
    const currentTime = Date.now();

    return currentTime >= expirationTime;
  },

  /**
   * Retrieve the user information from the token
   */
  getUserFromToken(): User | null {
    const token = this.getToken();
    if (!token) return null;

    if (this.isAccessTokenExpired()) {
      this.removeToken();
      return null;
    }

    const decodedToken = this.decodeToken(token);
    if (!decodedToken) {
      console.error('Unable to decode the token');
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
