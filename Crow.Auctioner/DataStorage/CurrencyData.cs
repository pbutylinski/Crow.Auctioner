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
        public Currencies Currency { get; set; }
        public string FormatString { get; set; }
        public decimal BaseExchangeRatio { get; set; }
        public string Name { get; set; }
    }
}
