/**
 * RoleGate Component
 * Conditionally renders children based on user roles
 *
 * Usage:
 * <RoleGate allowedRoles={['admin']}>
 *   <AdminOnlyContent />
 * </RoleGate>
 */

'use client';

import { ReactNode } from 'react';
import { usePermissions } from '@/hooks/usePermissions';
import type { Role } from '@/lib/auth/route-config';

interface RoleGateProps {
  children: ReactNode;
  allowedRoles: (Role | string)[];
  requireAll?: boolean; // If true, user must have ALL roles. Default: false (user needs at least ONE)
  fallback?: ReactNode; // Content to show when user doesn't have permission
}

export function RoleGate({
  children,
  allowedRoles,
  requireAll = false,
  fallback = null,
}: RoleGateProps) {
  const { hasAnyRole, hasAllRoles } = usePermissions();

  const hasPermission = requireAll
    ? hasAllRoles(allowedRoles)
    : hasAnyRole(allowedRoles);

  if (!hasPermission) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
