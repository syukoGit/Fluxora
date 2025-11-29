import * as React from 'react';
import * as NavigationMenuPrimitive from '@radix-ui/react-navigation-menu';
import { cva } from 'class-variance-authority';
import { ChevronDown, ChevronLeft, ChevronRight, ChevronUp } from 'lucide-react';

import { cn } from '@/lib/utils';

type NavigationMenuContentDirection = 'top' | 'right' | 'bottom' | 'left';

const NavigationMenuDirectionContext = React.createContext<NavigationMenuContentDirection>('bottom');

const NavigationMenu = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.Root> & {
    direction?: NavigationMenuContentDirection;
    viewport?: boolean;
  }
>(({ className, children, direction = 'bottom', viewport, ...props }, ref) => {
  const isVertical = direction === 'left' || direction === 'right';
  const showViewport = viewport ?? !isVertical;

  return (
    <NavigationMenuDirectionContext.Provider value={direction}>
      <NavigationMenuPrimitive.Root
        ref={ref}
        className={cn('relative z-10 flex max-w-max flex-1 items-center justify-center', className)}
        {...props}
      >
        {children}
        {showViewport && <NavigationMenuViewport />}
      </NavigationMenuPrimitive.Root>
    </NavigationMenuDirectionContext.Provider>
  );
});
NavigationMenu.displayName = NavigationMenuPrimitive.Root.displayName;

const NavigationMenuList = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.List>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.List>
>(({ className, ...props }, ref) => {
  const direction = React.useContext(NavigationMenuDirectionContext);
  const isVertical = direction === 'left' || direction === 'right';

  return (
    <NavigationMenuPrimitive.List
      ref={ref}
      className={cn(
        'group flex flex-1 list-none items-center justify-center',
        isVertical ? 'flex-col space-y-1' : 'space-x-1',
        className
      )}
      {...props}
    />
  );
});
NavigationMenuList.displayName = NavigationMenuPrimitive.List.displayName;

const NavigationMenuItem = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.Item>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.Item>
>(({ className, ...props }, ref) => {
  const direction = React.useContext(NavigationMenuDirectionContext);
  const isVertical = direction === 'left' || direction === 'right';

  return <NavigationMenuPrimitive.Item ref={ref} className={cn(isVertical ? 'relative' : '', className)} {...props} />;
});
NavigationMenuItem.displayName = NavigationMenuPrimitive.Item.displayName;

const navigationMenuTriggerStyle = cva(
  'group inline-flex h-9 w-max items-center justify-center rounded-md bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground focus:outline-none disabled:pointer-events-none disabled:opacity-50 data-[state=open]:text-accent-foreground data-[state=open]:bg-accent/50 data-[state=open]:hover:bg-accent data-[state=open]:focus:bg-accent'
);

const NavigationMenuTrigger = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.Trigger>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.Trigger>
>(({ className, children, ...props }, ref) => {
  const direction = React.useContext(NavigationMenuDirectionContext);

  const ChevronIcon =
    {
      top: ChevronUp,
      right: ChevronRight,
      left: ChevronLeft,
      bottom: ChevronDown,
    }[direction] ?? ChevronDown;

  return (
    <NavigationMenuPrimitive.Trigger
      ref={ref}
      className={cn(navigationMenuTriggerStyle(), 'group', className)}
      {...props}
    >
      {children}{' '}
      <ChevronIcon
        className='relative top-[1px] ml-1 h-3 w-3 transition duration-300 group-data-[state=open]:rotate-180'
        aria-hidden='true'
      />
    </NavigationMenuPrimitive.Trigger>
  );
});
NavigationMenuTrigger.displayName = NavigationMenuPrimitive.Trigger.displayName;

const NavigationMenuContent = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.Content>
>(({ className, ...props }, ref) => {
  return (
    <NavigationMenuPrimitive.Content
      ref={ref}
      className={cn(
        'absolute left-0 top-0 w-full data-[motion^=from-]:animate-in data-[motion^=to-]:animate-out data-[motion^=from-]:fade-in data-[motion^=to-]:fade-out data-[motion=from-end]:slide-in-from-right-52 data-[motion=from-start]:slide-in-from-left-52 data-[motion=to-end]:slide-out-to-right-52 data-[motion=to-start]:slide-out-to-left-52 md:absolute md:w-auto ',

        className
      )}
      {...props}
    />
  );
});
NavigationMenuContent.displayName = NavigationMenuPrimitive.Content.displayName;

const NavigationMenuLink = NavigationMenuPrimitive.Link;

const NavigationMenuViewport = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.Viewport>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.Viewport>
>(({ className, ...props }, ref) => {
  const direction = React.useContext(NavigationMenuDirectionContext);

  const viewportPositionClasses = {
    bottom: 'left-0 top-full mt-1.5',
    top: 'left-0 bottom-full mb-1.5',
    right: 'left-full top-0 ml-1.5',
    left: 'right-full top-0 mr-1.5',
  }[direction];

  return (
    <NavigationMenuPrimitive.Viewport
      className={cn(
        'absolute origin-top-center h-[var(--radix-navigation-menu-viewport-height)] w-full overflow-hidden rounded-md border bg-popover text-popover-foreground shadow data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-90 md:w-[var(--radix-navigation-menu-viewport-width)]',
        viewportPositionClasses,
        className
      )}
      ref={ref}
      {...props}
    />
  );
});
NavigationMenuViewport.displayName = NavigationMenuPrimitive.Viewport.displayName;

const NavigationMenuIndicator = React.forwardRef<
  React.ElementRef<typeof NavigationMenuPrimitive.Indicator>,
  React.ComponentPropsWithoutRef<typeof NavigationMenuPrimitive.Indicator>
>(({ className, ...props }, ref) => (
  <NavigationMenuPrimitive.Indicator
    ref={ref}
    className={cn(
      'top-full z-[1] flex h-1.5 items-end justify-center overflow-hidden data-[state=visible]:animate-in data-[state=hidden]:animate-out data-[state=hidden]:fade-out data-[state=visible]:fade-in',
      className
    )}
    {...props}
  >
    <div className='relative top-[60%] h-2 w-2 rotate-45 rounded-tl-sm bg-border shadow-md' />
  </NavigationMenuPrimitive.Indicator>
));
NavigationMenuIndicator.displayName = NavigationMenuPrimitive.Indicator.displayName;

export {
  navigationMenuTriggerStyle,
  NavigationMenu,
  NavigationMenuList,
  NavigationMenuItem,
  NavigationMenuContent,
  NavigationMenuTrigger,
  NavigationMenuLink,
  NavigationMenuIndicator,
  NavigationMenuViewport,
};
