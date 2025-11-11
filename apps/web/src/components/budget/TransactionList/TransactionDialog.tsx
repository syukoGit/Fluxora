'use client';

import { DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { CategoryDto, TransactionDto } from '@/lib/budget/transaction/types';
import { useEffect, useRef, useState } from 'react';
import { Select, SelectContent, SelectItem, SelectSeparator, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useForm } from 'react-hook-form';
import transactionSchema from '@/lib/budget/transaction/schemas/transaction-form.schema';
import * as z from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import Currency from '@/lib/currencies';
import InputWithIcon from '@/components/InputWithIcon';
import { Baseline, Landmark, LoaderCircleIcon, Trash2 } from 'lucide-react';
import { CurrencyIcon } from '@/components/CurrencyIcon';
import DatePicker from '@/components/DatePicker';
import { createJsonPatchDocument } from '@/lib/api/jsonPatch';
import { apiClient } from '@/lib/api/client';
import { isNullEmptyOrUndefined, normalizeDate } from '@/lib/utils';
import { Button } from '@/components/ui/button';

interface Props {
  transaction?: TransactionDto;
  categories: CategoryDto[];
  open: boolean;
  setOpen: (open: boolean) => void;
  onSuccess?: () => void;
}

const TransactionDialog = ({ transaction, categories, open, setOpen, onSuccess }: Props) => {
  const [updating, setUpdating] = useState(!!transaction);
  const [loading, setLoading] = useState(false);
  const previousCategoryId = useRef<string | null>(undefined);

  useEffect(() => {
    setUpdating(!!transaction);
  }, [transaction]);

  const form = useForm<z.infer<typeof transactionSchema>>({
    resolver: zodResolver(transactionSchema),
    mode: 'onChange',
    defaultValues: {
      name: transaction?.name ?? '',
      amount: transaction?.amount ?? 0,
      currency: transaction?.currency ?? Currency.EUR,
      date: transaction?.date ? new Date(transaction?.date) : new Date(),
      categoryId: transaction?.categoryId,
      subCategoryId: transaction?.subCategoryId,
      bank: transaction?.bank,
    },
  });

  // Reset form when dialog opens
  useEffect(() => {
    if (open) {
      form.reset({
        name: transaction?.name ?? '',
        amount: transaction?.amount ?? 0,
        currency: transaction?.currency ?? Currency.EUR,
        date: transaction?.date ? new Date(transaction?.date) : new Date(),
        categoryId: transaction?.categoryId,
        subCategoryId: transaction?.subCategoryId,
        bank: transaction?.bank ?? '',
      });
      previousCategoryId.current = transaction?.categoryId;
    }
  }, [open, transaction, form]);

  const onSubmit = async (data: z.infer<typeof transactionSchema>) => {
    setLoading(true);

    try {
      let result: TransactionDto;

      const transactionData = {
        ...data,
        date: normalizeDate(data.date),
        categoryId: isNullEmptyOrUndefined(data.categoryId) ? null : data.categoryId,
        subCategoryId: isNullEmptyOrUndefined(data.subCategoryId) ? null : data.subCategoryId,
      };

      if (transaction === undefined) {
        result = await apiClient.fetch('/budget/FinancialTransactions', {
          method: 'POST',
          body: JSON.stringify(transactionData),
        });
      } else {
        const originalData = {
          name: transaction.name,
          amount: transaction.amount,
          currency: transaction.currency,
          date: transaction.date,
          categoryId: transaction.categoryId,
          subCategoryId: transaction.subCategoryId,
          bank: transaction.bank,
        };

        console.log('Original Data:', originalData);
        console.log('Updated Data:', transactionData);

        if (updating && transaction) {
          const patch = createJsonPatchDocument(originalData, transactionData);

          console.log('Generated JSON Patch:', patch);

          if (patch.length === 0) {
            console.log('No changes detected, skipping update.');
            setOpen(false);
            return;
          }

          result = await apiClient.fetch(`/budget/FinancialTransactions/${transaction.id}`, {
            method: 'PATCH',
            body: JSON.stringify(patch),
          });

          console.log('Transaction updated:', result);
        }
      }

      // Close dialog and trigger refresh on successful submission
      onSuccess?.();
      setOpen(false);
    } catch (error) {
      console.error('Error submitting transaction form:', error);
    } finally {
      setLoading(false);
    }
  };

  const onDelete = async () => {
    if (!transaction) return;
    try {
      setLoading(true);
      await apiClient.fetch(`/budget/FinancialTransactions/${transaction.id}`, {
        method: 'DELETE',
      });
      onSuccess?.();
      setOpen(false);
    } catch (error) {
      console.error('Error deleting transaction:', error);
    } finally {
      setLoading(false);
    }
  };

  const categoryId = form.watch('categoryId');
  const currentCategory = categories.find((cat) => cat.id === categoryId);

  // Reset subCategoryId when the category changes
  useEffect(() => {
    if (previousCategoryId.current !== undefined && previousCategoryId.current !== categoryId) {
      form.setValue('subCategoryId', '');
    }
    previousCategoryId.current = categoryId;
  }, [categoryId, form]);

  return (
    <DialogContent aria-description={updating ? 'Modifier une transaction' : 'Ajouter une transaction'}>
      <DialogHeader>
        <DialogTitle>{updating ? 'Modifier une transaction' : 'Ajouter une transaction'}</DialogTitle>
        <DialogDescription>
          Veuillez remplir le formulaire ci-dessous pour {updating ? 'mettre à jour la' : 'ajouter une'} transaction.
        </DialogDescription>
      </DialogHeader>
      <div>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className='grid grid-cols-2 auto-rows-min gap-2'>
            <FormField
              control={form.control}
              name='name'
              disabled={loading}
              render={({ field }) => (
                <FormItem className='space-y-0'>
                  <FormLabel>Nom</FormLabel>
                  <FormControl>
                    <InputWithIcon {...field} icon={<Baseline />} className='disabled:cursor-auto space-y-0' />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <div>
              <div className='flex rounded-md shadow-xs items-end'>
                <FormField
                  control={form.control}
                  name='amount'
                  disabled={loading}
                  render={({ field }) => (
                    <FormItem className='w-full space-y-0'>
                      <FormLabel>Montant</FormLabel>
                      <FormControl>
                        <InputWithIcon
                          {...field}
                          type='number'
                          className='-me-px rounded-r-none shadow-none border-r-0 disabled:cursor-auto'
                          icon={<CurrencyIcon currency={form.watch('currency')} />}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || e.target.value)}
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name='currency'
                  disabled={loading}
                  render={({ field }) => (
                    <FormItem>
                      <Select onValueChange={field.onChange} value={field.value} disabled={loading}>
                        <SelectTrigger className='w-fit rounded-l-none shadow-none gap-2 disabled:cursor-auto'>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent position='popper' className='w-3.5'>
                          {Object.values(Currency).map((currency) => (
                            <SelectItem key={currency} value={currency} className='[&_svg]:hidden'>
                              {Currency[currency]}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </FormItem>
                  )}
                />
              </div>
              <FormField
                control={form.control}
                name='amount'
                render={() => (
                  <FormItem>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name='currency'
                render={() => (
                  <FormItem>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
            <FormField
              control={form.control}
              name='date'
              disabled={loading}
              render={({ field }) => (
                <FormItem className='space-y-0'>
                  <FormLabel>Date</FormLabel>
                  <FormControl>
                    <DatePicker value={field.value} setDate={field.onChange} disabled={loading} />
                  </FormControl>
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name='categoryId'
              disabled={loading}
              render={({ field }) => (
                <FormItem className='space-y-0 flex-1'>
                  <FormLabel>Catégorie</FormLabel>
                  <FormControl>
                    <Select
                      onValueChange={(value) => field.onChange(value === 'none' ? null : value)}
                      value={field.value || ''}
                      disabled={loading}
                    >
                      <SelectTrigger className='disabled:cursor-auto'>
                        <SelectValue placeholder='Sélectionner une catégorie' />
                      </SelectTrigger>
                      <SelectContent position='popper' className='w-full'>
                        <SelectItem value='none'>Aucune catégorie</SelectItem>
                        <SelectSeparator />
                        {categories.map((category) => (
                          <SelectItem key={category.id} value={category.id}>
                            {category.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </FormControl>
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name='bank'
              disabled={loading}
              render={({ field }) => (
                <FormItem className='space-y-0'>
                  <FormLabel>Banque</FormLabel>
                  <FormControl>
                    <InputWithIcon {...field} icon={<Landmark />} className='disabled:cursor-auto' />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name='subCategoryId'
              disabled={loading || !currentCategory || currentCategory.subCategories?.length === 0}
              render={({ field }) => (
                <FormItem className='space-y-0 flex-1'>
                  <FormLabel>Sous-catégorie</FormLabel>
                  <FormControl>
                    <Select
                      key={categoryId}
                      onValueChange={(value) => field.onChange(value)}
                      value={field.value ?? ''}
                      disabled={loading || !currentCategory || currentCategory.subCategories?.length === 0}
                    >
                      <SelectTrigger className='disabled:cursor-auto'>
                        <SelectValue placeholder='Sélectionner une sous-catégorie' />
                      </SelectTrigger>
                      <SelectContent position='popper' className='w-full'>
                        {currentCategory?.subCategories?.map((subCategory) => (
                          <SelectItem key={subCategory.id} value={subCategory.id}>
                            {subCategory.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </FormControl>
                </FormItem>
              )}
            />
            <div className='col-span-2 mt-4 flex justify-end gap-2'>
              <Button type='reset' variant='outline' onClick={() => setOpen(false)} disabled={loading}>
                Annuler
              </Button>
              <Button type='submit' disabled={loading}>
                {loading && <LoaderCircleIcon className='animate-spin' />}
                {updating ? 'Mettre à jour' : 'Ajouter'}
              </Button>
              {transaction && (
                <Button
                  type='button'
                  className='hover:bg-destructive text-destructive border-destructive'
                  variant='outline'
                  disabled={loading}
                  onClick={onDelete}
                >
                  <Trash2 />
                  Supprimer
                </Button>
              )}
            </div>
          </form>
        </Form>
      </div>
    </DialogContent>
  );
};

export default TransactionDialog;
