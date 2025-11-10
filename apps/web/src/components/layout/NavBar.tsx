'use client';

import { Button } from '@/components/ui/button';
import { HomeIcon, ScrollText } from 'lucide-react';
import Link from 'next/link';

export default function NavBar() {
  return (
    <nav className='w-fit h-full p-1 bg-secondary flex flex-col justify-start gap-1'>
      <Button
        asChild
        variant='ghost'
        className='p-4 h-12 w-12'
        aria-label='Accueil'
      >
        <Link href='/'>
          <HomeIcon aria-hidden='true' />
        </Link>
      </Button>
      <Button
        asChild
        variant='ghost'
        className='p-4 h-12 w-12'
        aria-label='Budget'
      >
        <Link href='/budget'>
          <ScrollText aria-hidden='true' />
        </Link>
      </Button>
    </nav>
  );
}
