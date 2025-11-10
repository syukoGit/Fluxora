'use client';

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';
import { Header } from '@/components/layout/Header';
import { User } from 'lucide-react';
import { useEffect, useState } from 'react';

export default function Home() {
  const { status, isAuthenticated } = useAuth();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted || status === 'loading') {
    return (
      <div className='flex items-center justify-center min-h-screen'>
        <div className='text-center'>
          <div className='animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto'></div>
          <p className='mt-4 text-gray-600'>Chargement...</p>
        </div>
      </div>
    );
  }

  return (
    <main className='flex flex-col flex-1'>
      <Header />
      {isAuthenticated ? (
        <div className='flex flex-col w-full h-full justify-center items-center gap-4'>
          <User />
          <p className='text-gray-600'>Vous êtes connecté !</p>
        </div>
      ) : (
        <div className='flex flex-col items-center gap-4'>
          <p className='text-gray-600'>
            Connectez-vous pour accéder à l&apos;application
          </p>
          <Button variant='outline' asChild>
            <Link href='/login'>Se connecter</Link>
          </Button>
        </div>
      )}
    </main>
  );
}
