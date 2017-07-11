using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow.Auctioner.DataStorage
{
    [Serializable]
    public class CurrencyData
    {
        public string FormatString { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Name { get; set; }

        public CurrencyData()
        {
            FormatString = "{0.00}";
            ExchangeRate = 1;
        }
    }
}
