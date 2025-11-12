'use client';

import { ChartConfig, ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart';
import { Empty, EmptyHeader, EmptyMedia } from '@/components/ui/empty';
import { ChartData, getChartData as getPieChartData, getPieChartConfig } from '@/lib/budget/transaction/chartsUtils';
import { CategoryDto, TransactionDto } from '@/lib/budget/transaction/types';
import { ChartPie } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Pie, PieChart, Sector } from 'recharts';
import { PieSectorDataItem } from 'recharts/types/polar/Pie';

interface Props {
  transactions: TransactionDto[];
  categories: CategoryDto[];
  activeCategoryId?: string | null;
  setActiveCategoryId?: (categoryId: string | null) => void;
  baseColor?: string;
}

const TransactionsChart = ({ transactions, categories, activeCategoryId, setActiveCategoryId, baseColor }: Props) => {
  const [chartData, setChartData] = useState<ChartData>([]);
  const [chartConfig, setChartConfig] = useState<ChartConfig>({});

  useEffect(() => {
    const data = getPieChartData(transactions, categories, activeCategoryId, baseColor);
    const config = getPieChartConfig(data, categories, activeCategoryId);

    setChartData(data);
    setChartConfig(config);
  }, [transactions, categories, activeCategoryId, baseColor]);

  if (chartData.length === 0) {
    return (
      <Empty className='min-h-[200px] w-full'>
        <EmptyMedia variant='icon'>
          <ChartPie />
        </EmptyMedia>
        <EmptyHeader>
          Aucune transaction disponible pour{' '}
          {activeCategoryId
            ? `la catégorie \"${categories.find((category) => category.id === activeCategoryId)?.name}\"`
            : 'ces catégories'}
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <ChartContainer config={chartConfig} className='min-h-[200px] w-full'>
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
  );
};

export default TransactionsChart;
