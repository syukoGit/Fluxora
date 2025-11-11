'use client';

import { Dialog, DialogTrigger } from '@/components/ui/dialog';
import { CategoryDto, TransactionDto } from '@/lib/budget/transaction/types';
import { formatDate } from '@/lib/utils';
import TransactionDialog from './TransactionDialog';
import { useState } from 'react';

interface Props {
  transaction: TransactionDto;
  categories: CategoryDto[];
  onTransactionUpdated?: () => void;
}

const TransactionListElement = ({ transaction, categories, onTransactionUpdated }: Props) => {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger className='w-full flex flex-row justify-between hover:bg-secondary rounded-md p-2 pl-4 items-center cursor-pointer'>
        <div className='flex flex-col items-start'>
          <div className='text-base font-semibold'>{transaction.name}</div>
          <div className='text-xs text-secondary-foreground font-thin text'>{formatDate(transaction.date)}</div>
        </div>
        <div className={`text-sm ${transaction.amount < 0 ? 'text-red-500' : 'text-green-500'} pr-6`}>
          {transaction.amount.toFixed(2)} €
        </div>
      </DialogTrigger>
      <TransactionDialog
        transaction={transaction}
        categories={categories}
        open={open}
        setOpen={setOpen}
        {...(onTransactionUpdated && { onSuccess: onTransactionUpdated })}
      />
    </Dialog>
  );
};

export default TransactionListElement;
