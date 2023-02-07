using Crow.Auctioner.DataStorage;
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

namespace Crow.Auctioner
{
    public partial class SettingsWindow : Window
    {
        SaveFile _saveFile;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            _saveFile = SaveFile.Load();

            FormatStringATextBox.Text = _saveFile.PrimaryCurrency.FormatString;
            FormatStringBTextBox.Text = _saveFile.SecondaryCurrency.FormatString;
            FormatStringCTextBox.Text = _saveFile.TertiaryCurrency.FormatString;

            ExchangeRateATextBox.Text = _saveFile.PrimaryCurrency.ExchangeRate.ToString();
            ExchangeRateBTextBox.Text = _saveFile.SecondaryCurrency.ExchangeRate.ToString();
            ExchangeRateCTextBox.Text = _saveFile.TertiaryCurrency.ExchangeRate.ToString();

            NameATextBox.Text = _saveFile.PrimaryCurrency.Name;
            NameBTextBox.Text = _saveFile.SecondaryCurrency.Name;
            NameCTextBox.Text = _saveFile.TertiaryCurrency.Name;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _saveFile.PrimaryCurrency.FormatString = FormatStringATextBox.Text;
                _saveFile.SecondaryCurrency.FormatString = FormatStringBTextBox.Text;
                _saveFile.TertiaryCurrency.FormatString = FormatStringCTextBox.Text;

                _saveFile.PrimaryCurrency.ExchangeRate = Decimal.Parse(ExchangeRateATextBox.Text);
                _saveFile.SecondaryCurrency.ExchangeRate = Decimal.Parse(ExchangeRateBTextBox.Text);
                _saveFile.TertiaryCurrency.ExchangeRate = Decimal.Parse(ExchangeRateCTextBox.Text);

                _saveFile.PrimaryCurrency.Name = NameATextBox.Text;
                _saveFile.SecondaryCurrency.Name = NameBTextBox.Text;
                _saveFile.TertiaryCurrency.Name = NameCTextBox.Text;

                _saveFile.Save();

                Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error saving data:\r\n\r\n" + exc.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadConfiguration();
            }
        }
    }
}
