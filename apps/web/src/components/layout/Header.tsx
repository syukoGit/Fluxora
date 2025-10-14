'use client';

import { ThemeToggle } from '../ThemeToggle';
import { UserProfile } from '../UserProfile';

export function Header() {
  return (
    <header className="w-full h-fit p-1 bg-secondary flex flex-row justify-end gap-1">
      <UserProfile />
      <ThemeToggle />
    </header>
  );
}
