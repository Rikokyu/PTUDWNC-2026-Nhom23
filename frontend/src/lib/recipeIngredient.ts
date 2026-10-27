const numericPattern = /^[+-]?(?:\d+(?:[.,]\d*)?|[.,]\d+)$/;
const fractionPattern =
  /^([+-]?(?:\d+(?:[.,]\d*)?|[.,]\d+))\s*\/\s*([+-]?(?:\d+(?:[.,]\d*)?|[.,]\d+))$/;

function parseQuantityNumber(value: string): number | undefined {
  const normalized = value.replace(",", ".");
  return numericPattern.test(value) ? Number(normalized) : undefined;
}

export function isNonNegativeIngredientQuantity(quantity: string): boolean {
  const value = quantity.trim();
  if (!value) return true;

  const numericValue = parseQuantityNumber(value);
  if (numericValue !== undefined) return Number.isFinite(numericValue) && numericValue >= 0;

  const fraction = fractionPattern.exec(value);
  if (fraction) {
    const numerator = Number(fraction[1].replace(",", "."));
    const denominator = Number(fraction[2].replace(",", "."));
    return Number.isFinite(numerator)
      && Number.isFinite(denominator)
      && (denominator === 0 || numerator / denominator >= 0);
  }

  return !/(?:^|\s)[−-]\s*\d/.test(value);
}
