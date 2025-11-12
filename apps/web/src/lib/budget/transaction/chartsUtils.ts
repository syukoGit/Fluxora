import { ChartConfig } from '@/components/ui/chart';
import { CategoryDto, TransactionDto } from './types';
import { isNullEmptyOrUndefined } from '@/lib/utils';

export type ChartData = {
  categoryId: string;
  amount: number;
  fill: string;
}[];

export function getChartData(
  transactions: TransactionDto[],
  categories: CategoryDto[],
  activeCategoryId?: string | null,
  baseColor?: string
): ChartData {
  let categoryAmounts: Map<string, number>;

  if (activeCategoryId) {
    categoryAmounts = getPieChartDataWithActiveCategory(
      transactions,
      categories.find((cat) => cat.id === activeCategoryId) ?? 'uncategorized'
    );
  } else {
    const validCategoryIds = new Set(categories.map((cat) => cat.id));

    categoryAmounts = new Map<string, number>();

    transactions.forEach((transaction) => {
      let categoryId: string;

      if (!transaction.categoryId || !validCategoryIds.has(transaction.categoryId)) {
        categoryId = 'uncategorized';
      } else {
        categoryId = transaction.categoryId;
      }

      const currentAmount = categoryAmounts.get(categoryId) || 0;
      categoryAmounts.set(categoryId, currentAmount + Math.abs(transaction.amount));
    });
  }

  // Create chart data with colors
  let colorIndex = 1;
  const data = Array.from(categoryAmounts.entries())
    .sort(([, amountA], [, amountB]) => amountB - amountA)
    .map(([categoryId, amount]) => ({
      categoryId,
      amount,
      fill: `hsl(var(${baseColor ?? '--primary'})/${1 - 0.15 * colorIndex++})`,
    }));

  return data;
}

function getPieChartDataWithActiveCategory(
  transactions: TransactionDto[],
  category: CategoryDto | 'uncategorized'
): Map<string, number> {
  const transacToDisplay = transactions.filter((transaction) =>
    category === 'uncategorized'
      ? isNullEmptyOrUndefined(transaction.categoryId)
      : transaction.categoryId === category.id
  );

  if (category === 'uncategorized') {
    return new Map([
      ['uncategorized', transacToDisplay.reduce((sum, transaction) => sum + Math.abs(transaction.amount), 0)],
    ]);
  }

  const categoryAmounts = new Map<string, number>();

  transacToDisplay.forEach((transaction) => {
    let subCatId: string;

    if (!transaction.subCategoryId || !category.subCategories?.find((cat) => cat.id === transaction.subCategoryId)) {
      subCatId = 'uncategorized';
    } else {
      subCatId = transaction.subCategoryId;
    }

    const currentAmount = categoryAmounts.get(subCatId) || 0;
    categoryAmounts.set(subCatId, currentAmount + Math.abs(transaction.amount));
  });

  return categoryAmounts;
}

export function getPieChartConfig(
  chartData: ChartData,
  categories: CategoryDto[],
  activeCategoryId?: string | null
): ChartConfig {
  if (activeCategoryId) {
    return getPieChartConfigWithActiveCategory(
      chartData,
      categories.find((cat) => cat.id === activeCategoryId) ?? 'uncategorized'
    );
  }

  const config: ChartConfig = {};

  chartData.forEach((dataItem) => {
    const category = categories.find((cat) => cat.id === dataItem.categoryId);

    if (category) {
      config[category.id] = {
        label: category.name,
      };
    } else {
      config['uncategorized'] = {
        label: 'Non catégorisé',
      };
    }
  });

  return config;
}

function getPieChartConfigWithActiveCategory(
  chartData: ChartData,
  category: CategoryDto | 'uncategorized'
): ChartConfig {
  if (category === 'uncategorized' || category.subCategories?.length === 0) {
    return {
      uncategorized: {
        label: 'Non catégorisé',
      },
    };
  }

  const config: ChartConfig = {};

  chartData.forEach((dataItem) => {
    const subCategory = category.subCategories!.find((subCat) => subCat.id === dataItem.categoryId);

    if (subCategory) {
      config[subCategory.id] = {
        label: subCategory.name,
      };
    } else {
      config['uncategorized'] = {
        label: 'Non catégorisé',
      };
    }
  });

  return config;
}
