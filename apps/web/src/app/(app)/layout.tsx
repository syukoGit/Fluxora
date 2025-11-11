'use client';

import { Header } from '@/components/layout/Header';
import NavBar from '@/components/layout/NavBar';

export default function AppLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <main className='flex flex-col flex-1'>
      <Header />
      <div className='flex flex-row flex-1'>
        <NavBar />
        <div className='flex flex-col flex-1 items-center justify-center'>
          {children}
        </div>
      </div>
    </main>
  );
}
