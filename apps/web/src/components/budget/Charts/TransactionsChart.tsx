'use client';

import { ChartConfig, ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart';
import { Empty, EmptyDescription, EmptyMedia, EmptyTitle } from '@/components/ui/empty';
import { ChartData, getChartData as getPieChartData, getPieChartConfig } from '@/lib/budget/transaction/chartsUtils';
import { CategoryDto, TransactionDto } from '@/lib/budget/transaction/types';
import { cn } from '@/lib/utils';
import { PieChart as PieChartIcon } from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { Pie, PieChart, Sector } from 'recharts';
import { PieSectorDataItem } from 'recharts/types/polar/Pie';

interface Props extends React.HTMLAttributes<HTMLDivElement> {
  transactions: TransactionDto[];
  categories: CategoryDto[];
  activeCategoryId?: string | null;
  setActiveCategoryId?: (categoryId: string | null) => void;
  activeSubCategoryId?: string | null;
  setActiveSubCategoryId?: (subCategoryId: string | null) => void;
  baseColor?: string;
  heading?: React.ReactNode;
}

const TransactionsChart = ({
  transactions,
  categories,
  activeCategoryId,
  setActiveCategoryId,
  activeSubCategoryId,
  setActiveSubCategoryId,
  baseColor,
  heading,
  ...props
}: Props) => {
  const [chartData, setChartData] = useState<ChartData>([]);
  const [chartConfig, setChartConfig] = useState<ChartConfig>({});
  const [innerRadius, setInnerRadius] = useState<number>(30);
  const containerRef = React.useRef<HTMLDivElement>(null);

  useEffect(() => {
    const data = getPieChartData(transactions, categories, activeCategoryId, activeSubCategoryId, baseColor);
    const config = getPieChartConfig(data, transactions, categories, activeCategoryId, activeSubCategoryId);

    setChartData(data);
    setChartConfig(config);
  }, [transactions, categories, activeCategoryId, activeSubCategoryId, baseColor]);

  useEffect(() => {
    const updateInnerRadius = () => {
      if (containerRef.current) {
        const width = containerRef.current.offsetWidth;

        const calculatedRadius = Math.min(Math.max(width * 0.15, 20), 60);
        setInnerRadius(calculatedRadius);
      }
    };

    updateInnerRadius();
    window.addEventListener('resize', updateInnerRadius);

    return () => window.removeEventListener('resize', updateInnerRadius);
  }, []);

  const { className, ...rest } = props;

  let activeCategoryName: string | null = null;
  let activeSubCategoryName: string | null = null;

  if (activeCategoryId) {
    const category = categories.find((cat) => cat.id === activeCategoryId);
    activeCategoryName = category ? category.name : '';

    if (activeSubCategoryId && category?.subCategories) {
      const subCategory = category.subCategories.find((subCat) => subCat.id === activeSubCategoryId);
      activeSubCategoryName = subCategory ? subCategory.name : '';
    }
  }
  return (
    <div ref={containerRef} className={cn('flex w-full min-w-0 flex-col items-center', className)} {...rest}>
      {heading ? <div className='mb-1 text-center text-sm font-medium text-muted-foreground'>{heading}</div> : null}
      {chartData.length === 0 ? (
        <Empty className='w-full aspect-square max-h-[400px]'>
          <EmptyMedia variant='icon'>
            <PieChartIcon />
          </EmptyMedia>
          <EmptyTitle>Pas de données</EmptyTitle>
          <EmptyDescription>
            Aucune transaction{' '}
            {activeSubCategoryName
              ? `dans la sous-catégorie ${activeSubCategoryName}`
              : activeCategoryName
                ? `dans la catégorie ${activeCategoryName}`
                : ''}
          </EmptyDescription>
        </Empty>
      ) : (
        <ChartContainer config={chartConfig} className={cn('w-full aspect-square max-h-[400px]')}>
          <PieChart>
            <ChartTooltip
              cursor={false}
              content={<ChartTooltipContent hideLabel valueFormatter={(value) => `${value} €`} />}
            />
            <Pie
              data={chartData}
              dataKey='amount'
              nameKey='id'
              innerRadius={innerRadius}
              strokeWidth={5}
              activeShape={({ outerRadius = 0, ...props }: PieSectorDataItem) => (
                <Sector {...props} outerRadius={outerRadius + 10} />
              )}
              onClick={(data) => {
                if (!data?.id) return;

                if (!activeCategoryId) {
                  setActiveCategoryId?.(data.id);
                  setActiveSubCategoryId?.(null);
                } else if (activeCategoryId && !activeSubCategoryId) {
                  if (activeCategoryId !== 'uncategorized') {
                    setActiveSubCategoryId?.(data.id);
                  }
                }
              }}
            />
          </PieChart>
        </ChartContainer>
      )}
    </div>
  );
};

export default TransactionsChart;
