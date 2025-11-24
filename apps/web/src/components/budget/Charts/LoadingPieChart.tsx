'use client';

import { ChartContainer } from '@/components/ui/chart';
import { cn } from '@/lib/utils';
import { useEffect, useRef, useState } from 'react';
import { Pie, PieChart } from 'recharts';

const chartConfig = {
  A: {
    label: 'A',
  },
  B: {
    label: 'B',
  },
  C: {
    label: 'C',
  },
  D: {
    label: 'D',
  },
  E: {
    label: 'E',
  },
};

const chartData = [
  { id: 'A', amount: 40, fill: 'hsl(var(--primary) / 0.25)' },
  { id: 'B', amount: 25, fill: 'hsl(var(--primary) / 0.20)' },
  { id: 'C', amount: 15, fill: 'hsl(var(--primary) / 0.15)' },
  { id: 'D', amount: 15, fill: 'hsl(var(--primary) / 0.10)' },
  { id: 'E', amount: 5, fill: 'hsl(var(--primary) / 0.05)' },
];

const LoadingPieChart = () => {
  const [innerRadius, setInnerRadius] = useState<number>(30);
  const containerRef = useRef<HTMLDivElement>(null);

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

  return (
    <div ref={containerRef} className='flex w-full min-w-0 flex-col items-center'>
      <ChartContainer config={chartConfig} className={cn('w-full aspect-square max-h-[400px]')}>
        <PieChart>
          <Pie
            className='animate-pulse'
            data={chartData}
            dataKey='amount'
            nameKey='id'
            innerRadius={innerRadius}
            strokeWidth={5}
          />
        </PieChart>
      </ChartContainer>
    </div>
  );
};

export default LoadingPieChart;
