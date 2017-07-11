using Crow.Auctioner.DataStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow.Auctioner
{
    public static class MoneyExtensions
    {
        public static Money ConvertCurrency(this Money money, CurrencyData currency)
        {
            var baseValue = money.Value / money.Currency.ExchangeRate;
            return new Money(currency, baseValue * currency.ExchangeRate);
        }
    }
}
