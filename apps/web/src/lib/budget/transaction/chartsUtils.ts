import { ChartConfig } from '@/components/ui/chart';
import { CategoryDto, TransactionDto } from './types';
import { isNullEmptyOrUndefined } from '@/lib/utils';

export type ChartData = {
  id: string;
  amount: number;
  fill: string;
}[];

export type ChartLevel = 'category' | 'subcategory' | 'transaction';

/**
 * Generates chart data based on the current navigation level
 * @param transactions - List of transactions
 * @param categories - List of categories
 * @param activeCategoryId - Selected category ID (null = category level)
 * @param activeSubCategoryId - Selected subcategory ID (null = subcategory level)
 * @param baseColor - Base color for chart segments
 * @returns ChartData with amounts aggregated by level
 */
export function getChartData(
  transactions: TransactionDto[],
  categories: CategoryDto[],
  activeCategoryId?: string | null,
  activeSubCategoryId?: string | null,
  baseColor?: string
): ChartData {
  let dataMap: Map<string, number>;

  // Level 3: Display individual transactions
  if (activeCategoryId && activeSubCategoryId) {
    dataMap = getTransactionLevelData(transactions, activeCategoryId, activeSubCategoryId);
  }
  // Level 2: Display subcategories
  else if (activeCategoryId) {
    const category = categories.find((cat) => cat.id === activeCategoryId);

    // Special case: 'uncategorized' displays transactions directly
    if (activeCategoryId === 'uncategorized' || !category) {
      dataMap = getTransactionLevelData(transactions, 'uncategorized', null);
    } else {
      dataMap = getSubCategoryLevelData(transactions, category);
    }
  }
  // Level 1: Display categories
  else {
    dataMap = getCategoryLevelData(transactions, categories);
  }

  // Create chart data with colors
  let colorIndex = 1;
  const data = Array.from(dataMap.entries())
    .sort(([, amountA], [, amountB]) => amountB - amountA)
    .map(([id, amount]) => ({
      id,
      amount,
      fill: `hsl(var(${baseColor ?? '--primary'})/${Math.max(1 - 0.15 * colorIndex++, 0.1)})`,
    }));

  return data;
}

/**
 * Level 1: Aggregates transactions by category
 */
function getCategoryLevelData(transactions: TransactionDto[], categories: CategoryDto[]): Map<string, number> {
  const validCategoryIds = new Set(categories.map((cat) => cat.id));
  const categoryAmounts = new Map<string, number>();

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

  return categoryAmounts;
}

/**
 * Level 2: Aggregates transactions by subcategory
 */
function getSubCategoryLevelData(transactions: TransactionDto[], category: CategoryDto): Map<string, number> {
  const validSubCategoryIds = new Set(category.subCategories?.map((subCat) => subCat.id) || []);
  const subCategoryAmounts = new Map<string, number>();

  // Filter transactions for this category
  const categoryTransactions = transactions.filter((transaction) => transaction.categoryId === category.id);

  categoryTransactions.forEach((transaction) => {
    let subCategoryId: string;

    if (!transaction.subCategoryId || !validSubCategoryIds.has(transaction.subCategoryId)) {
      subCategoryId = 'uncategorized';
    } else {
      subCategoryId = transaction.subCategoryId;
    }

    const currentAmount = subCategoryAmounts.get(subCategoryId) || 0;
    subCategoryAmounts.set(subCategoryId, currentAmount + Math.abs(transaction.amount));
  });

  return subCategoryAmounts;
}

/**
 * Level 3: Displays individual transactions
 * For 'uncategorized' at category level: activeSubCategoryId = null
 * For normal subcategories: activeSubCategoryId is defined
 */
function getTransactionLevelData(
  transactions: TransactionDto[],
  activeCategoryId: string,
  activeSubCategoryId: string | null
): Map<string, number> {
  const transactionAmounts = new Map<string, number>();

  let filteredTransactions: TransactionDto[];

  // Special case: 'uncategorized' at category level
  if (activeCategoryId === 'uncategorized') {
    filteredTransactions = transactions.filter((t) => isNullEmptyOrUndefined(t.categoryId));
  }
  // Transactions for a specific subcategory
  else if (activeSubCategoryId === 'uncategorized') {
    filteredTransactions = transactions.filter(
      (t) => t.categoryId === activeCategoryId && isNullEmptyOrUndefined(t.subCategoryId)
    );
  } else if (activeSubCategoryId) {
    filteredTransactions = transactions.filter(
      (t) => t.categoryId === activeCategoryId && t.subCategoryId === activeSubCategoryId
    );
  } else {
    filteredTransactions = [];
  }

  filteredTransactions.forEach((transaction) => {
    transactionAmounts.set(transaction.id, Math.abs(transaction.amount));
  });

  return transactionAmounts;
}

/**
 * Generates chart configuration based on the navigation level
 */
export function getPieChartConfig(
  chartData: ChartData,
  transactions: TransactionDto[],
  categories: CategoryDto[],
  activeCategoryId?: string | null,
  activeSubCategoryId?: string | null
): ChartConfig {
  // Level 3: Configuration for transactions
  if (activeCategoryId && activeSubCategoryId) {
    return getTransactionLevelConfig(chartData, transactions);
  }
  // Level 2: Configuration for subcategories
  else if (activeCategoryId) {
    const category = categories.find((cat) => cat.id === activeCategoryId);

    // Special case: 'uncategorized' displays transactions
    if (activeCategoryId === 'uncategorized' || !category) {
      return getTransactionLevelConfig(chartData, transactions);
    }

    return getSubCategoryLevelConfig(chartData, category);
  }
  // Level 1: Configuration for categories
  else {
    return getCategoryLevelConfig(chartData, categories);
  }
}

/**
 * Level 1 configuration: Categories
 */
function getCategoryLevelConfig(chartData: ChartData, categories: CategoryDto[]): ChartConfig {
  const config: ChartConfig = {};

  chartData.forEach((dataItem) => {
    if (dataItem.id === 'uncategorized') {
      config['uncategorized'] = {
        label: 'Non catégorisé',
      };
    } else {
      const category = categories.find((cat) => cat.id === dataItem.id);
      if (category) {
        config[category.id] = {
          label: category.name,
        };
      }
    }
  });

  return config;
}

/**
 * Level 2 configuration: Subcategories
 */
function getSubCategoryLevelConfig(chartData: ChartData, category: CategoryDto): ChartConfig {
  const config: ChartConfig = {};

  chartData.forEach((dataItem) => {
    if (dataItem.id === 'uncategorized') {
      config['uncategorized'] = {
        label: 'Non catégorisé',
      };
    } else {
      const subCategory = category.subCategories?.find((subCat) => subCat.id === dataItem.id);
      if (subCategory) {
        config[subCategory.id] = {
          label: subCategory.name,
        };
      }
    }
  });

  return config;
}

/**
 * Level 3 configuration: Individual transactions
 */
function getTransactionLevelConfig(chartData: ChartData, transactions: TransactionDto[]): ChartConfig {
  const config: ChartConfig = {};

  chartData.forEach((dataItem) => {
    const transaction = transactions.find((t) => t.id === dataItem.id);
    if (transaction) {
      // Use transaction name as label
      config[transaction.id] = {
        label: transaction.name || `Transaction ${transaction.id.substring(0, 8)}`,
      };
    }
  });

  return config;
}
