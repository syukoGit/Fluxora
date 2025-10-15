/**
 * Custom hook for role-based access control
 * Use this hook to check user permissions in React components
 */

import { useAuth } from './useAuth';
import type { Role } from '@/lib/auth/route-config';

export function usePermissions() {
  const { user } = useAuth();

  /**
   * Check if user has a specific role
   */
  const hasRole = (role: Role | string): boolean => {
    if (!user || !user.roles) return false;
    return user.roles.includes(role);
  };

  /**
   * Check if user has at least one of the specified roles
   */
  const hasAnyRole = (roles: (Role | string)[]): boolean => {
    if (!user || !user.roles) return false;
    return roles.some((role) => user.roles!.includes(role));
  };

  /**
   * Check if user has all of the specified roles
   */
  const hasAllRoles = (roles: (Role | string)[]): boolean => {
    if (!user || !user.roles) return false;
    return roles.every((role) => user.roles!.includes(role));
  };

  /**
   * Check if user is an admin
   */
  const isAdmin = (): boolean => {
    return hasRole('admin');
  };

  /**
   * Check if user is a moderator
   */
  const isModerator = (): boolean => {
    return hasAnyRole(['moderator', 'admin']);
  };

  return {
    hasRole,
    hasAnyRole,
    hasAllRoles,
    isAdmin,
    isModerator,
    userRoles: user?.roles || [],
  };
}
