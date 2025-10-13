/**
 * Authentication related types
 */

export interface User {
  id: string;
  email: string;
  name?: string;
  roles?: string[];
}

export interface AuthResponse {
  token: string;
  refreshToken?: string;
  user: User;
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface RegisterCredentials {
  lastName: string;
  firstName: string;
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export type AuthStatus = 'loading' | 'authenticated' | 'unauthenticated';
