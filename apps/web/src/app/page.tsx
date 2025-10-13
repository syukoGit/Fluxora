'use client';

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';
import { UserProfile } from '@/components/UserProfile';

export default function Home() {
  const { status, isAuthenticated } = useAuth();

  if (status === 'loading') {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
          <p className="mt-4 text-gray-600">Chargement...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-8">
      <h1 className="text-4xl font-bold">Fluxora</h1>

      {isAuthenticated ? (
        <div className="flex flex-col items-center gap-4">
          <UserProfile />
          <p className="text-gray-600">Vous êtes connecté !</p>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-4">
          <p className="text-gray-600">
            Connectez-vous pour accéder à l'application
          </p>
          <Button variant="outline" asChild>
            <Link href="/login">Se connecter</Link>
          </Button>
        </div>
      )}
    </div>
  );
}
