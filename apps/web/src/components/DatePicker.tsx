'use client';

import { useState } from 'react';
import { Popover, PopoverContent, PopoverTrigger } from './ui/popover';
import { Button, ButtonProps } from './ui/button';
import { CalendarIcon, ChevronDownIcon } from 'lucide-react';
import { Calendar } from './ui/calendar';
import { cn } from '@/lib/utils';

interface Props extends Omit<ButtonProps, 'value'> {
  value: Date | undefined;
  setDate: (value: Date | undefined) => void;
}

const DatePicker = ({ value, setDate, ...props }: Props) => {
  const [open, setOpen] = useState(false);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant='outline'
          id='date'
          className={cn('w-full h-9 flex flex-col items-stretch font-normal px-3 pr-3 pl-0', props.className)}
          noAnimation
          {...props}
        >
          <div className='relative'>
            <div className='text-muted-foreground pointer-events-none absolute inset-y-0 left-0 flex items-center justify-center pl-3'>
              <CalendarIcon className='size-4' />
            </div>
            <span className='flex justify-between items-center pl-9'>
              {value ? value.toLocaleDateString() : 'Sélectionner une date'}
              <ChevronDownIcon className='text-muted-foreground' />
            </span>
          </div>
        </Button>
      </PopoverTrigger>
      <PopoverContent className='w-auto overflow-hidden p-0' align='start'>
        <Calendar
          mode='single'
          selected={value}
          onSelect={(date) => {
            setDate(date);
            setOpen(false);
          }}
        />
      </PopoverContent>
    </Popover>
  );
};

export default DatePicker;
