'use client';

import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/button';
import { usePathname } from 'next/navigation';
import { User } from 'lucide-react';
import { HoverCard, HoverCardContent, HoverCardTrigger } from '@/components/ui/hover-card';
import { Separator } from './ui/separator';
import Link from 'next/link';

export function UserProfile() {
  const { user, isAuthenticated, logout } = useAuth();

  const pathname = usePathname();

  if (pathname === '/login' || pathname === '/register') {
    return <></>;
  }

  return (
    <HoverCard>
      <HoverCardTrigger asChild>
        <Button variant='ghost' size='icon' aria-label='Profil' className='p-4 h-12 w-12'>
          <User className='size-5' aria-hidden='true' />
        </Button>
      </HoverCardTrigger>
      <HoverCardContent side='bottom' className='flex flex-col items-center gap-2 w-fit'>
        {isAuthenticated ? (
          <>
            <p className='font-medium'>{user?.name}</p>
            <Separator className='w-full' />
            <Button asChild variant='outline' size='sm' className='w-full'>
              <Link href='/profile'>Mon profil</Link>
            </Button>
            <Button variant='outline' size='sm' onClick={logout} className='w-full'>
              Se déconnecter
            </Button>
          </>
        ) : (
          <>
            <Button asChild variant='outline' size='sm' className='w-full'>
              <Link href='/login'>Se connecter</Link>
            </Button>
            <Button asChild variant='outline' size='sm' className='w-full'>
              <Link href='/register'>S&apos;inscrire</Link>
            </Button>
          </>
        )}
      </HoverCardContent>
    </HoverCard>
  );
}
