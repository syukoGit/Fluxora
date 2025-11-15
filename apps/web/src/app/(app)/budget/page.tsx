'use client';

import TransactionList from '@/components/budget/TransactionList';
import { Button } from '@/components/ui/button';
import { ButtonGroup } from '@/components/ui/button-group';
import { apiClient } from '@/lib/api/client';
import { CategoryDto, TransactionDto } from '@/lib/budget/transaction/types';
import { Plus } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Dialog, DialogTrigger } from '@/components/ui/dialog';
import TransactionDialog from '@/components/budget/TransactionList/TransactionDialog';
import TransactionsChart from '@/components/budget/Charts/TransactionsChart';

export default function BudgetPage() {
  const [transactions, setTransactions] = useState<TransactionDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [activeCategoryId, setActiveCategoryId] = useState<string | null>(null);
  const [activeSubCategoryId, setActiveSubCategoryId] = useState<string | null>(null);

  const fetchTransactions = async () => {
    try {
      setLoading(true);
      const transactions = await apiClient.fetch('/budget/financialtransactions');
      const categories = await apiClient.fetch('/budget/categories');

      setTransactions(transactions ?? []);
      setCategories(categories ?? []);
    } catch (error) {
      console.error('Error fetching budget data:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransactions();
  }, []);

  const filteredTransactions = activeCategoryId
    ? transactions.filter((transaction) => transaction.categoryId === activeCategoryId)
    : transactions;
  const negativeTransactions = filteredTransactions.filter((transaction) => transaction.amount < 0);
  const positiveTransactions = filteredTransactions.filter((transaction) => transaction.amount >= 0);

  return (
    <div className='h-full w-full grid grid-cols-2 grid-rows-2 place-items-stretch *:p-3'>
      <div className='flex flex-row w-full min-w-0 gap-4 overflow-hidden items-center justify-center'>
        <TransactionsChart
          transactions={positiveTransactions}
          categories={categories}
          activeCategoryId={activeCategoryId}
          setActiveCategoryId={setActiveCategoryId}
          activeSubCategoryId={activeSubCategoryId}
          setActiveSubCategoryId={setActiveSubCategoryId}
          baseColor='--success'
          heading='Revenus'
          className='flex-1 min-w-0'
        />
        <TransactionsChart
          transactions={negativeTransactions}
          categories={categories}
          activeCategoryId={activeCategoryId}
          setActiveCategoryId={setActiveCategoryId}
          activeSubCategoryId={activeSubCategoryId}
          setActiveSubCategoryId={setActiveSubCategoryId}
          baseColor='--destructive'
          heading='Dépenses'
          className='flex-1 min-w-0'
        />
      </div>
      <div className='h-full w-full flex flex-col items-end gap-2'>
        <ButtonGroup>
          <Dialog open={addDialogOpen} onOpenChange={setAddDialogOpen}>
            <DialogTrigger asChild>
              <Button variant='outline'>
                <Plus />
              </Button>
            </DialogTrigger>
            <TransactionDialog
              categories={categories}
              open={addDialogOpen}
              setOpen={setAddDialogOpen}
              onSuccess={fetchTransactions}
            />
          </Dialog>
        </ButtonGroup>
        <TransactionList
          loading={loading}
          transactions={transactions}
          categories={categories}
          onTransactionUpdated={fetchTransactions}
        />
      </div>
      <div className='bg-primary/10 p-6 rounded-lg shadow-md w-64 h-40 flex flex-col items-center justify-center'>
        <h2 className='text-lg font-semibold mb-2'>Diagramme de Sankey</h2>
      </div>
    </div>
  );
}
