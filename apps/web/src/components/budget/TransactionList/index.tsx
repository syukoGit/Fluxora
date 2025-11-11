'use client';

import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '@/components/ui/accordion';
import { CategoryDto, TransactionDto, TransactionNestedMap } from '@/lib/budget/transaction/types';
import { mapTransactionDtosToCategories, sortCategorizedTransactions } from '@/lib/budget/transaction/utils';
import { useEffect, useState } from 'react';
import TransactionListElement from './TransactionListElement';
import LoadingList from './LoadingList';
import { Empty, EmptyContent, EmptyDescription, EmptyHeader } from '@/components/ui/empty';
import { Button } from '@/components/ui/button';

interface Props {
  loading?: boolean;
  transactions: TransactionDto[];
  categories: CategoryDto[];
  onTransactionUpdated?: () => void;
}

const TransactionList = ({ loading, transactions, categories, onTransactionUpdated }: Props) => {
  const [categorized, setCategorized] = useState<Omit<TransactionNestedMap, 'Uncategorized'>>();
  const [uncategorized, setUncategorized] = useState<TransactionDto[]>();

  useEffect(() => {
    let mounted = true;

    (() => {
      try {
        const transactionsCat = mapTransactionDtosToCategories(
          transactions as TransactionDto[],
          categories as CategoryDto[]
        );

        if (mounted) {
          const { Uncategorized: uncategorizedTransactions, ...categorizedTransactions } = transactionsCat;

          const sortedCategorized = sortCategorizedTransactions(categorizedTransactions);

          setCategorized(sortedCategorized);
          setUncategorized(uncategorizedTransactions);
        }
      } catch (error) {
        console.error('Error fetching transactions:', error);
      }
    })();

    return () => {
      mounted = false;
    };
  }, [categories, transactions]);

  if (loading) {
    return <LoadingList />;
  }

  if ((!categorized || Object.keys(categorized).length === 0) && (!uncategorized || uncategorized.length === 0)) {
    return (
      <Empty className='w-full h-full border-[1px] border-solid rounded-md'>
        <EmptyHeader>Aucune transaction trouvée</EmptyHeader>
        <EmptyDescription>Il n&apos;y a pas de transactions à afficher sur ce mois-ci.</EmptyDescription>
        <EmptyContent>
          <Button variant='outline'>Ajouter une transaction</Button>
        </EmptyContent>
      </Empty>
    );
  }

  return (
    <Accordion type='single' collapsible className='w-full h-full flex flex-col border-[1px] rounded-md'>
      {categorized &&
        Object.entries(categorized).map(([categoryName, subCategories]) => (
          <AccordionItem key={categoryName} value={categoryName} className='p-2'>
            <AccordionTrigger className='p-0 text-base hover:no-underline'>{categoryName}</AccordionTrigger>
            <AccordionContent className='flex flex-col gap-2 p-0'>
              {Object.entries(subCategories).map(([subCategoryName, transactions]) => (
                <Accordion key={subCategoryName} type='single' collapsible className='w-full p-0 gap-2 first:pt-2'>
                  <AccordionItem key={subCategoryName} value={subCategoryName} className='border-b-0'>
                    <AccordionTrigger className='p-0 pl-2 text-base hover:no-underline'>
                      {subCategoryName}
                    </AccordionTrigger>
                    <AccordionContent className='w-full p-0 gap-2 first:pt-2 last:pb-2 flex flex-col'>
                      {transactions.map((transaction) => (
                        <TransactionListElement
                          key={transaction.id}
                          transaction={transaction}
                          categories={categories}
                          {...(onTransactionUpdated && { onTransactionUpdated })}
                        />
                      ))}
                    </AccordionContent>
                  </AccordionItem>
                </Accordion>
              ))}
            </AccordionContent>
          </AccordionItem>
        ))}
      {uncategorized && (
        <AccordionItem key='Uncategorized' value='Uncategorized' className='p-2'>
          <AccordionTrigger className='p-0 text-base hover:no-underline'>Non catégorisé</AccordionTrigger>
          <AccordionContent className='flex flex-col gap-2 p-0'>
            {uncategorized.map((transaction) => (
              <TransactionListElement
                key={transaction.id}
                transaction={transaction}
                categories={categories}
                {...(onTransactionUpdated && { onTransactionUpdated })}
              />
            ))}
          </AccordionContent>
        </AccordionItem>
      )}
    </Accordion>
  );
};

export default TransactionList;
