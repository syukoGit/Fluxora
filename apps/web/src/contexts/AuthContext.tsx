'use client';

import React, { createContext, useEffect, useState, useCallback } from 'react';
import { tokenService } from '@/lib/auth/token';
import { authApi } from '@/lib/api/auth';
import type {
  User,
  AuthStatus,
  LoginCredentials,
  RegisterCredentials,
} from '@/lib/auth/types';

interface AuthContextValue {
  user: User | null;
  status: AuthStatus;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
  register: (credentials: RegisterCredentials) => Promise<void>;
  isAuthenticated: boolean;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined
);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [status, setStatus] = useState<AuthStatus>('loading');

  /**
   * Initialize the user from the stored token
   */
  const initializeAuth = useCallback(async () => {
    try {
      setStatus('loading');

      // Check if token exists and is valid
      if (tokenService.isAccessTokenExpired()) {
        if (!tokenService.isRefreshTokenExpired()) {
          console.log('Access token expired, attempting to refresh...');
          await authApi.refreshToken();
        }

        if (tokenService.isAccessTokenExpired()) {
          console.log('Token refresh failed or access token still expired.');
          setStatus('unauthenticated');
          setUser(null);
          return;
        }
      }

      // Retrieve user information from the token
      const userData = tokenService.getUserFromToken();

      if (userData) {
        setUser(userData);
        setStatus('authenticated');
      } else {
        setStatus('unauthenticated');
        setUser(null);
      }
    } catch (error) {
      console.error('Auth initialization error:', error);
      setStatus('unauthenticated');
      setUser(null);
      tokenService.removeToken();
    }
  }, []);

  /**
   * Login
   */
  const login = useCallback(async (credentials: LoginCredentials) => {
    try {
      const response = await authApi.login(credentials);

      setUser(response.user);
      setStatus('authenticated');
    } catch (error) {
      setStatus('unauthenticated');
      setUser(null);
      throw error;
    }
  }, []);

  /**
   * Logout
   */
  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      setUser(null);
      setStatus('unauthenticated');
    }
  }, []);

  /**
   * Register
   */
  const register = useCallback(async (credentials: RegisterCredentials) => {
    try {
      const response = await authApi.register(credentials);

      setUser(response.user);
      setStatus('authenticated');
    } catch (error) {
      setStatus('unauthenticated');
      setUser(null);
      throw error;
    }
  }, []);

  /**
   * Initialize on component mount
   */
  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  /**
   * Listen for token expiration / unauthorized events
   */
  useEffect(() => {
    const handleUnauthorized = () => {
      setUser(null);
      setStatus('unauthenticated');
    };

    if (typeof window !== 'undefined') {
      window.addEventListener('auth:unauthorized', handleUnauthorized);
      return () => {
        window.removeEventListener('auth:unauthorized', handleUnauthorized);
      };
    }
  }, []);

  const value: AuthContextValue = {
    user,
    status,
    login,
    logout,
    register,
    isAuthenticated: status === 'authenticated',
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
