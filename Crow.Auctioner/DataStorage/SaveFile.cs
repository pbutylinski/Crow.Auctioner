using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Crow.Auctioner.DataStorage
{
    [Serializable]
    public class SaveFile
    {
        public List<AuctionItem> AuctionItems { get; set; }
        public List<CurrencyData> CurrencyDatas { get; set; }

        public Currencies MainCurrency { get; set; }
        public Currencies? SideCurrencyA { get; set; }
        public Currencies? SideCurrencyB { get; set; }

        public Money AlreadyInCharity { get; set; }

        public SaveFile()
        {
            AuctionItems = new List<AuctionItem>();
            CurrencyDatas = new List<CurrencyData>();
            AlreadyInCharity = new Money(MainCurrency);
        }

        public void Save()
        {
            var xmlSerializer = new XmlSerializer(typeof(SaveFile));
            var path = GetFilePath();

            using (var fs = new FileStream(path, FileMode.Create))
            {
                xmlSerializer.Serialize(fs, this);
            }
        }

        public static SaveFile Load()
        {
            var xmlSerializer = new XmlSerializer(typeof(SaveFile));
            var path = GetFilePath();

            if (!File.Exists(path)) return GetDefaultFile();

            using (var fs = new FileStream(path, FileMode.Open))
            {
                return xmlSerializer.Deserialize(fs) as SaveFile;
            }
        }

        private static string GetFilePath()
        {
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var crowDir = Path.Combine(appDataDir, "Crow.d");
            var fullPath = Path.Combine(crowDir, "Crow.Auctioner.Data.xml");

            if (!Directory.Exists(crowDir)) Directory.CreateDirectory(crowDir);

            return fullPath;
        }

        private static SaveFile GetDefaultFile()
        {
            return new SaveFile
            {
                MainCurrency = Currencies.PLN,
                SideCurrencyA = Currencies.EUR,
                SideCurrencyB = Currencies.CZK,
                CurrencyDatas = new List<CurrencyData>
                {
                    new CurrencyData
                    {
                        Currency = Currencies.PLN,
                        BaseExchangeRatio = 1,
                        FormatString = "{0:0.00}zł",
                        Name = "Polski złoty"
                    },
                    new CurrencyData
                    {
                        Currency = Currencies.EUR,
                        BaseExchangeRatio = (decimal)0.225,
                        FormatString = "{0:0.00}€",
                        Name = "Euro"
                    },
                    new CurrencyData
                    {
                        Currency = Currencies.CZK,
                        BaseExchangeRatio = (decimal)12.3,
                        FormatString = "{0:0}CZK",
                        Name = "Czech koruna"
                    }
                }
            };
        }
    }
}
