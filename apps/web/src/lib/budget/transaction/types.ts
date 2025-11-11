import Currency from '@/lib/currencies';

// API-side types
export interface TransactionDto {
  id: string;
  name: string;
  amount: number;
  currency: Currency;
  date: string;
  categoryId?: string;
  subCategoryId?: string;
  bank?: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  subCategories?: SubcategoryDto[];
}

export interface SubcategoryDto {
  id: string;
  name: string;
}

export type TransactionNestedMap = {
  [key: string]: {
    [key: string]: TransactionDto[];
  };
} & {
  Uncategorized?: TransactionDto[];
};
