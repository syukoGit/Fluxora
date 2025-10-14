'use client';

import { Header } from '@/components/layout/Header';
import NavBar from '@/components/layout/NavBar';
import { useAuth } from '@/hooks/useAuth';
import { useRouter } from 'next/navigation';
import { useEffect, useRef } from 'react';

export default function AppLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const { status } = useAuth();
  const router = useRouter();


  useEffect(() => {
    if (status === 'unauthenticated') {
      router.replace('/login');
    }
  }, [status, router]);

  if (status !== 'authenticated') {
    return null;
  }

  return (
    <main className="flex flex-col flex-1">
      <Header />
      <div className="flex flex-row flex-1">
        <NavBar />
        <div className="flex flex-col flex-1 items-center justify-center gap-8">
          {children}
        </div>
      </div>
    </main>
  );
}
