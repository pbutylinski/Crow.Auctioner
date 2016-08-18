using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crow.Auctioner.DataStorage;

namespace Crow.Auctioner
{
    public static class CurrencyFactory
    {
        private static CurrencyData GetCurrencyData(Currencies currency)
        {
            var data = SaveFile.Load();
            return data.CurrencyDatas.FirstOrDefault(c => c.Currency == currency);
        }

        public static decimal GetExchangeRate(Currencies currency)
        {
            var currencyData = GetCurrencyData(currency);
            return currencyData == null ? 1 : currencyData.BaseExchangeRatio;
        }

        public static string FormatCurrency(Currencies currency, decimal amount)
        {
            var currencyData = GetCurrencyData(currency);
            return String.Format(currencyData.FormatString, amount);
        }
    }
}
