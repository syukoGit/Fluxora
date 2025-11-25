enum Currency {
  EUR = 'EUR',
  USD = 'USD',
  GBP = 'GBP',
}

export default Currency;

export const isCurrency = (value: string): value is Currency => {
  return Object.values(Currency).includes(value as Currency);
};
