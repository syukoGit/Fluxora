import { CategoryDto, TransactionDto, TransactionNestedMap } from './types';

/**
 * Sorts transactions by date (most recent first) and categories/subcategories alphabetically
 */
export function sortCategorizedTransactions(
  categoryMap: Omit<TransactionNestedMap, 'Uncategorized'>
): Omit<TransactionNestedMap, 'Uncategorized'> {
  // Sort transactions by date (most recent first) within each subcategory
  Object.keys(categoryMap).forEach((categoryName) => {
    Object.keys(categoryMap[categoryName]!).forEach((subCategoryName) => {
      categoryMap[categoryName]![subCategoryName]!.sort((a, b) => {
        return new Date(b.date).getTime() - new Date(a.date).getTime();
      });
    });
  });

  // Sort categories and subcategories alphabetically
  const sortedCategoryMap: TransactionNestedMap = {};
  const sortedCategoryNames = Object.keys(categoryMap).sort((a, b) => a.localeCompare(b));

  sortedCategoryNames.forEach((categoryName) => {
    const sortedSubCategoryNames = Object.keys(categoryMap[categoryName]!).sort((a, b) => a.localeCompare(b));
    sortedCategoryMap[categoryName] = {};

    sortedSubCategoryNames.forEach((subCategoryName) => {
      const transactions = categoryMap[categoryName]![subCategoryName];
      if (transactions) {
        sortedCategoryMap[categoryName]![subCategoryName] = transactions;
      }
    });
  });

  return sortedCategoryMap;
}

export function mapTransactionDtosToCategories(
  transactions: TransactionDto[],
  categories: CategoryDto[]
): TransactionNestedMap {
  const categoryMap: TransactionNestedMap = {};

  transactions.forEach((transaction) => {
    const category = categories.find((cat) => cat.id == transaction.categoryId) ?? 'Uncategorized';

    if (category === 'Uncategorized') {
      if (!categoryMap.Uncategorized) {
        categoryMap.Uncategorized = [];
      }

      categoryMap.Uncategorized.push(transaction);
    } else {
      const subCategory =
        category.subCategories?.find((sub) => sub.id === transaction.subCategoryId)?.name ?? 'Non catégorisé';

      if (!categoryMap[category.name]) {
        categoryMap[category.name] = {};
      }

      if (!categoryMap[category.name]![subCategory]) {
        categoryMap[category.name]![subCategory] = [];
      }

      categoryMap[category.name]![subCategory]?.push(transaction);
    }
  });

  return categoryMap;
}
