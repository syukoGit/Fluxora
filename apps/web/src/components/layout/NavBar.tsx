'use client';

import { Button } from '@/components/ui/button';
import { HomeIcon, ScrollText } from 'lucide-react';
import Link from 'next/link';
import {
  NavigationMenu,
  NavigationMenuContent,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
  navigationMenuTriggerStyle,
} from '../ui/navigation-menu';

export default function NavBar() {
  return (
    <NavigationMenu direction='right' className='items-start p-2 bg-background border-r border-r-sidebar-border'>
      <NavigationMenuList className='gap-2'>
        <NavigationMenuItem>
          <NavigationMenuLink asChild className={navigationMenuTriggerStyle()}>
            <Link href='/dashboard'>
              <HomeIcon />
            </Link>
          </NavigationMenuLink>
        </NavigationMenuItem>
        <NavigationMenuItem>
          <NavigationMenuTrigger>
            <ScrollText />
          </NavigationMenuTrigger>
          <NavigationMenuContent className='absolute left-full top-0 ml-2 w-48 bg-popover p-2 rounded-md border shadow-md'>
            <ul className='grid gap-2'>
              <li>
                <NavigationMenuLink asChild>
                  <Link href='/budget/recurringExpenses'>
                    <Button variant='ghost' className='w-full justify-start text-foreground'>
                      Recurring Expenses
                    </Button>
                  </Link>
                </NavigationMenuLink>
              </li>
              {/* Add more navigation links as needed */}
            </ul>
          </NavigationMenuContent>
        </NavigationMenuItem>
      </NavigationMenuList>
    </NavigationMenu>
  );
}
