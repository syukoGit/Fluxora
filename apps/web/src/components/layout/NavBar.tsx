'use client';

import { Button } from '@/components/ui/button';
import { HomeIcon } from 'lucide-react';
import Link from 'next/link';

export default function NavBar() {
  return (
    <nav className="w-fit h-full p-1 bg-secondary flex flex-col justify-start gap-1">
      <Button asChild variant="ghost" className="p-4 h-12 w-12">
        <Link href="/">
          <HomeIcon />
        </Link>
      </Button>
    </nav>
  );
}
