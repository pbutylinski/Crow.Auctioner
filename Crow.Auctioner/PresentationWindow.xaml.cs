using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Crow.Auctioner.DataStorage;

namespace Crow.Auctioner
{
    /// <summary>
    /// Interaction logic for PresentationWindow.xaml
    /// </summary>
    public partial class PresentationWindow : Window
    {
        SaveFile _saveFile;

        public PresentationWindow()
        {
            InitializeComponent();

            _saveFile = SaveFile.Load();
            TotalCharityLabel.Content = GetTotalCharity();
        }

        public void DisplayItem(AuctionItem item)
        {
            if (item == null) return;
            
            _saveFile = SaveFile.Load();

            ItemNameLabel.Content = item.DisplayName;
            ItemCharityLabel.Content = item.ForCharityPercentage <= 0 ? "-" : String.Format("{0}%", item.ForCharityPercentage);
            ItemFromLabel.Content = item.Submissioner?.Name ?? "-";

            ItemPriceCenterLabel.Content = item.CurrentPrice.ConvertCurrency(_saveFile.PrimaryCurrency).ToString();
            
            ItemPriceLeftLabel.Content = _saveFile.SecondaryCurrency == null ? String.Empty :
                item.CurrentPrice.ConvertCurrency(_saveFile.SecondaryCurrency).ToString();

            ItemPriceRightLabel.Content = _saveFile.TertiaryCurrency == null ? String.Empty :
                item.CurrentPrice.ConvertCurrency(_saveFile.TertiaryCurrency).ToString();

            SoldLabel.Visibility = item.IsSold ? Visibility.Visible : Visibility.Collapsed;
            TotalCharityLabel.Content = GetTotalCharity();
        }

        public void ToggleWindowState()
        {
            if (WindowStyle == WindowStyle.SingleBorderWindow)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
        }

        public string GetTotalCharity()
        {
            var total = _saveFile.AlreadyInCharity.Copy();

            foreach (var item in _saveFile.AuctionItems.Where(k => k.IsSold))
                total += item.GetCharityAmount();

            return total.ConvertCurrency(_saveFile.PrimaryCurrency).ToString();
        }
    }
}
