import { CategoryDto, TransactionDto, TransactionNestedMap } from './types';

export function mapTransactionDtosToCategories(transactions: TransactionDto[], categories: CategoryDto[]): TransactionNestedMap {
  const categoryMap: TransactionNestedMap = {};

  transactions.forEach((transaction) => {
    const category = categories.find((cat) => cat.id == transaction.categoryId) ?? 'Uncategorized';

    if (category === 'Uncategorized') {
      if (!categoryMap['Uncategorized']) {
        categoryMap['Uncategorized'] = {
          Uncategorized: [],
        };
      }

      categoryMap['Uncategorized']['Uncategorized']!.push(transaction);
    } else {
      const subCategory = category.subCategories?.find((sub) => sub.id === transaction.subCategoryId)?.name ?? 'Uncategorized';
      console.log('Mapping transaction:', transaction, 'to category:', category.name, 'and subcategory:', transaction.subCategoryId);

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
