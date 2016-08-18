using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow.Auctioner
{
    public static class MoneyExtensions
    {
        public static Money ConvertCurrency(this Money money, Currencies newCurrency)
        {
            if (money.Currency == newCurrency) return money;

            var baseValue = money.Value / CurrencyFactory.GetExchangeRate(money.Currency);
            return new Money(newCurrency, baseValue * CurrencyFactory.GetExchangeRate(newCurrency));
        }
    }
}
