'use client';

import { Skeleton } from '@/components/ui/skeleton';

const LoadingList = () => {
  return (
    <div className='h-full w-full flex flex-col gap-2'>
      <Skeleton className='h-8 w-full rounded-md' />
      <div className='w-full pl-2'>
        <Skeleton className='h-8 w-full rounded-md' />
      </div>
      <div className='flex flex-col gap-2 w-full pl-4'>
        <Skeleton className='h-8 w-full rounded-md' />
        <Skeleton className='h-8 w-full rounded-md' />
      </div>
      <div className='w-full pl-2'>
        <Skeleton className='h-8 w-full rounded-md' />
      </div>
      <div className='flex flex-col gap-2 w-full pl-4'>
        <Skeleton className='h-8 w-full rounded-md' />
      </div>
      <Skeleton className='h-8 w-full rounded-md' />
      <div className='w-full pl-2'>
        <Skeleton className='h-8 w-full rounded-md' />
      </div>
      <div className='flex flex-col gap-2 w-full pl-4'>
        <Skeleton className='h-8 w-full rounded-md' />
      </div>
    </div>
  );
};

export default LoadingList;
