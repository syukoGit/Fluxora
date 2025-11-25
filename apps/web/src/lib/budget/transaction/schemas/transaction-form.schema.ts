import Currency from '@/lib/currencies';
import * as z from 'zod';

const transactionSchema = z.object({
  name: z
    .string()
    .min(1, 'Le nom est requis')
    .min(3, 'Le nom doit contenir au moins 3 caractères')
    .max(50, 'Le nom ne peut pas dépasser 50 caractères'),
  amount: z.number('Le montant doit être un nombre'),
  currency: z.enum(Currency),
  date: z.date(),
  categoryId: z.string().optional().nullable(),
  subCategoryId: z.string().optional().nullable(),
  bank: z.string().max(100, 'Le nom de la banque ne peut pas dépasser 100 caractères.').optional(),
});

export default transactionSchema;
