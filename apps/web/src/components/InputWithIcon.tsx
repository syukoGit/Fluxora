import React from 'react';
import { Input } from './ui/input';
import { cn } from '@/lib/utils';
import { LucideIcon } from 'lucide-react';

interface Props extends React.ComponentPropsWithoutRef<typeof Input> {
  icon: Omit<React.ComponentPropsWithoutRef<LucideIcon>, 'ref'>;
}

const InputWithIcon = ({ icon, ...props }: Props) => {
  return (
    <div className='w-full space-y-2'>
      <div className='relative'>
        <div className='text-muted-foreground pointer-events-none absolute inset-y-0 left-0 flex items-center justify-center pl-3 no-spiner'>
          {React.isValidElement(icon)
            ? React.cloneElement(icon, { className: 'size-4' } as React.HTMLAttributes<LucideIcon>)
            : null}
        </div>
        <Input {...props} className={cn('pl-9', props.className)} />
      </div>
    </div>
  );
};

export default InputWithIcon;
