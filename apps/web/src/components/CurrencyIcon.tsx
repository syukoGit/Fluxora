import { DollarSign, Euro, LucideIcon, PoundSterling } from 'lucide-react';
import Currency from '../lib/currencies';

interface Props extends Omit<React.ComponentPropsWithoutRef<LucideIcon>, 'ref'> {
  currency: Currency;
}

export const CurrencyIcon = ({ currency, ...props }: Props) => {
  switch (currency) {
    case Currency.EUR:
      return <Euro {...props} />;
    case Currency.USD:
      return <DollarSign {...props} />;
    case Currency.GBP:
      return <PoundSterling {...props} />;
  }
};
