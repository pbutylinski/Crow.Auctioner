using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow.Auctioner
{
    [Serializable]
    public class Money
    {
        public decimal Value { get; set; }
        public Currencies Currency { set; get; }

        public Money()
        {
        }

        public Money(Currencies currency)
        {
            Currency = currency;
        }

        public Money(Currencies currency, decimal value)
            : this(currency)
        {
            Value = value;
        }

        public Money Copy()
        {
            return new Money(Currency, Value);
        }

        public static Money operator +(Money a, Money b)
        {
            b = b.ConvertCurrency(a.Currency);
            a.Value += b.Value;
            return a;
        }

        public static Money operator -(Money a, Money b)
        {
            b = b.ConvertCurrency(a.Currency);
            a.Value -= b.Value;
            return a;
        }

        public static Money operator *(Money a, decimal b)
        {
            a.Value *= b;
            return a;
        }

        public static Money operator /(Money a, decimal b)
        {
            a.Value /= b;
            return a;
        }
        
        public override string ToString()
        {
            return CurrencyFactory.FormatCurrency(Currency, Value);
        }
    }
}
