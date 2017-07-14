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
        public Money AlreadyInCharity { get; set; }
        public CurrencyData PrimaryCurrency { get; set; }
        public CurrencyData SecondaryCurrency { get; set; }
        public CurrencyData TertiaryCurrency { get; set; }
        public string Title { get; set; }

        public SaveFile()
        {
            AuctionItems = new List<AuctionItem>();
            PrimaryCurrency = new CurrencyData();
            SecondaryCurrency = new CurrencyData();
            TertiaryCurrency = new CurrencyData();
            AlreadyInCharity = new Money(PrimaryCurrency);
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

            if (!File.Exists(path))
            {
                return GetDefaultFile();
            }

            using (var fs = new FileStream(path, FileMode.Open))
            {
                return xmlSerializer.Deserialize(fs) as SaveFile;
            }
        }

        public static string GetSaveDir()
        {
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var crowDir = Path.Combine(appDataDir, "Crow.d");
            if (!Directory.Exists(crowDir)) Directory.CreateDirectory(crowDir);
            return crowDir;
        }

        public static string GetPhotoPath(string photoFileName)
        {
            var newDir = Path.Combine(SaveFile.GetSaveDir(), "pics");

            if (Directory.Exists(newDir) == false)
            {
                Directory.CreateDirectory(newDir);
            }

            var path = Path.Combine(newDir, photoFileName);
            return path;
        }

        private static string GetFilePath()
        {
            var crowDir = GetSaveDir();
            var fullPath = Path.Combine(crowDir, "Crow.Auctioner.Data.v2.xml");
            return fullPath;
        }

        private static SaveFile GetDefaultFile()
        {
            var primaryCurrency = new CurrencyData
            {
                ExchangeRate = 1,
                FormatString = "{0:0.00}€",
                Name = "Euro"
            };

            return new SaveFile
            {
                Title = "Auctioner",
                PrimaryCurrency = primaryCurrency,
                SecondaryCurrency = new CurrencyData
                {
                    ExchangeRate = 1,
                    FormatString = "{0:0.00}Kč",
                    Name = "Czech Koruna"
                },
                TertiaryCurrency = new CurrencyData
                {
                    ExchangeRate = 1,
                    FormatString = "${0:0.00}",
                    Name = "US Dollar"
                },
                AlreadyInCharity = new Money
                {
                    Currency = primaryCurrency,
                    Value = 0
                }
            };
        }
    }
}
