'use client';

import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/button';

/**
 * Example UserProfile component to display user info and logout
 */
export function UserProfile() {
  const { user, status, logout, isAuthenticated } = useAuth();

  if (status === 'loading') {
    return <div>Chargement...</div>;
  }

  if (!isAuthenticated) {
    return <div>Non connecté</div>;
  }

  return (
    <div className="flex items-center gap-4">
      <div>
        <p className="font-semibold">{user?.name || 'Utilisateur'}</p>
        <p className="text-sm text-gray-600">{user?.email}</p>
        <p>{user?.id}</p>
      </div>
      <Button onClick={logout} variant="outline" size="sm">
        Déconnexion
      </Button>
    </div>
  );
}
