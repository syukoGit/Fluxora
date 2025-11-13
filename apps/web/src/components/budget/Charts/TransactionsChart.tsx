'use client';

import { ChartConfig, ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart';
import { ChartData, getChartData as getPieChartData, getPieChartConfig } from '@/lib/budget/transaction/chartsUtils';
import { CategoryDto, TransactionDto } from '@/lib/budget/transaction/types';
import { cn } from '@/lib/utils';
import React, { useEffect, useState } from 'react';
import { Pie, PieChart, Sector } from 'recharts';
import { PieSectorDataItem } from 'recharts/types/polar/Pie';

interface Props extends React.HTMLAttributes<HTMLDivElement> {
  transactions: TransactionDto[];
  categories: CategoryDto[];
  activeCategoryId?: string | null;
  setActiveCategoryId?: (categoryId: string | null) => void;
  baseColor?: string;
  heading?: React.ReactNode;
}

const TransactionsChart = ({
  transactions,
  categories,
  activeCategoryId,
  setActiveCategoryId,
  baseColor,
  heading,
  ...props
}: Props) => {
  const [chartData, setChartData] = useState<ChartData>([]);
  const [chartConfig, setChartConfig] = useState<ChartConfig>({});

  useEffect(() => {
    const data = getPieChartData(transactions, categories, activeCategoryId, baseColor);
    const config = getPieChartConfig(data, categories, activeCategoryId);

    setChartData(data);
    setChartConfig(config);
  }, [transactions, categories, activeCategoryId, baseColor]);

  const { className, ...rest } = props;

  if (chartData.length === 0) {
    return <></>;
  }

  return (
    <div className={cn('flex w-full min-w-0 flex-col items-center', className)} {...rest}>
      {heading ? <div className='mb-1 text-center text-sm font-medium text-muted-foreground'>{heading}</div> : null}
      <ChartContainer config={chartConfig} className={cn('w-full aspect-square max-h-[400px]')}>
        <PieChart>
          <ChartTooltip
            cursor={false}
            content={<ChartTooltipContent hideLabel valueFormatter={(value) => `${value} €`} />}
          />
          <Pie
            data={chartData}
            dataKey='amount'
            nameKey='categoryId'
            innerRadius={60}
            strokeWidth={5}
            activeShape={({ outerRadius = 0, ...props }: PieSectorDataItem) => (
              <Sector {...props} outerRadius={outerRadius + 10} />
            )}
            onClick={(data) => {
              if (!activeCategoryId && data && data.categoryId) {
                setActiveCategoryId?.(data.categoryId);
              }
            }}
          />
        </PieChart>
      </ChartContainer>
    </div>
  );
};

export default TransactionsChart;
