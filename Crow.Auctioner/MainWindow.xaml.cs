using Crow.Auctioner.DataStorage;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Crow.Auctioner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PresentationWindow _presentationWindow;
        private ImageWindow _photoWindow;
        private SaveFile _saveFile;
        private int _selectedIndex;
        private bool _isLoading = false;

        AuctionItem CurrentItem { get { return ItemsDataGrid.SelectedItem as AuctionItem; } }

        public MainWindow()
        {
            InitializeComponent();
            InitializePresentationWindow();
            InitializePhotoWindow();
            LoadSaveFile();

            _selectedIndex = -1;
        }

        void InitializePresentationWindow()
        {
            _presentationWindow = new PresentationWindow();
        }

        void InitializePhotoWindow()
        {
            _photoWindow = new ImageWindow();
        }

        void LoadSaveFile()
        {
            _saveFile = SaveFile.Load();
            ReloadItems();
        }

        void ReloadItems()
        {
            ItemsDataGrid.ItemsSource = null;
            ItemsDataGrid.ItemsSource = _saveFile.AuctionItems;

            if (_selectedIndex < 0 && _saveFile.AuctionItems.Any())
                _selectedIndex = 0;

            if (_saveFile.AuctionItems.Any() == false)
                DetailsGrid.Visibility = Visibility.Hidden;

            ItemsDataGrid.SelectedIndex = _selectedIndex;
        }

        void LoadCurrentItemDetails()
        {
            if (CurrentItem == null)
            {
                DetailsGrid.Visibility = Visibility.Hidden;
                return;
            }

            _isLoading = true;

            DetailsGrid.Visibility = Visibility.Visible;

            ItemOriginalPriceTextBox.Text = CurrentItem.StartingPrice.Value.ToString();
            ItemPriceTextBox.Text = CurrentItem.CurrentPrice.Value.ToString();
            ItemDisplayNameTextBox.Text = CurrentItem.DisplayName;
            ItemSoldCheckBox.IsChecked = CurrentItem.IsSold;
            ItemCharityTextBox.Text = CurrentItem.ForCharityPercentage.ToString();
            ItemFromTextBox.Text = CurrentItem.Submissioner?.Name;
            ItemWinnerTextBox.Text = CurrentItem.AuctionWinner?.Name;
            

            try
            {
                ItemPicture.Source = null;

                var picturePath = SaveFile.GetPhotoPath(CurrentItem.PhotoFileName);
                ItemPicture.Source = new BitmapImage(new Uri(picturePath));
            }
            catch { }

            _isLoading = false;
        }

        void Save()
        {
            _saveFile.AuctionItems = ItemsDataGrid.Items.Cast<AuctionItem>().ToList();

            _saveFile.Save();
        }

        private void MakePresentationWndFullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            _presentationWindow.ToggleWindowState();
        }

        private void ShowPresentationWindowButton_Click(object sender, RoutedEventArgs e)
        {
            _presentationWindow.Close();
            InitializePresentationWindow();
            _presentationWindow.Show();
        }

        private void AuctionItemField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateCurrentItemData();
                UpdatePreview();

                (sender as TextBox)?.SelectAll();
            }
        }

        void GetCurrentItemData()
        {
            if (CurrentItem == null) return;

            if (Decimal.TryParse(ItemPriceTextBox.Text, out var newAmount))
                CurrentItem.CurrentPrice.Value = newAmount;

            if (Decimal.TryParse(ItemOriginalPriceTextBox.Text, out var origAmount))
                CurrentItem.StartingPrice.Value = origAmount;

            if (Decimal.TryParse(ItemCharityTextBox.Text, out var charity))
                CurrentItem.ForCharityPercentage = charity;

            CurrentItem.DisplayName = ItemDisplayNameTextBox.Text;
            CurrentItem.IsSold = ItemSoldCheckBox.IsChecked ?? false;
            CurrentItem.AuctionWinner = new Attendee { Name = ItemWinnerTextBox.Text };
            CurrentItem.Submissioner = new Attendee { Name = ItemFromTextBox.Text };
        }

        void UpdateCurrentItemData()
        {
            GetCurrentItemData();
            Save();
            ReloadItems();
        }

        private void ItemsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCurrentItemDetails();

            if (ItemsDataGrid.SelectedIndex >= 0)
                _selectedIndex = ItemsDataGrid.SelectedIndex;
        }

        private void AddNewAuctionItemButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentItemData();

            var newItem = new AuctionItem
            {
                DisplayName = "New item",
                StartingPrice = new Money(_saveFile.PrimaryCurrency),
                CurrentPrice = new Money(_saveFile.PrimaryCurrency)
            };

            _saveFile.AuctionItems.Add(newItem);

            Save();
            ReloadItems();
            ItemsDataGrid.SelectedItem = newItem;
            _selectedIndex = ItemsDataGrid.SelectedIndex;

            ItemDisplayNameTextBox.Focus();
            ItemDisplayNameTextBox.SelectAll();
        }

        private void DisplayItemButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentItemData();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            _presentationWindow.DisplayItem(CurrentItem);
            _photoWindow.DisplayItem(CurrentItem);
        }

        private void SaveItemButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentItemData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var msgResult = MessageBox.Show("Do you really want to exit the application?", "Crow.Auctioner",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (msgResult == MessageBoxResult.Yes)
            {
                UpdateCurrentItemData();
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var sw = new SettingsWindow();
            sw.ShowDialog();

            _saveFile = SaveFile.Load();

            LoadCurrentItemDetails();
        }

        private void ItemSoldCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            CurrentItem.IsSold = true;
            UpdateCurrentItemData();
            UpdatePreview();
        }

        private void ItemSoldCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            CurrentItem.IsSold = false;
            UpdateCurrentItemData();
            UpdatePreview();
        }

        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            var fop = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All files|*.*"
            };

            if (fop.ShowDialog() == true && File.Exists(fop.FileName))
            {
                try
                {
                    var fi = new FileInfo(fop.FileName);
                    var newFilename = Guid.NewGuid().ToString() + fi.Extension;
                    var newPath = SaveFile.GetPhotoPath(newFilename);

                    File.Copy(fop.FileName, newPath);

                    CurrentItem.PhotoFileName = newFilename;
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString(), "Error adding photo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            UpdateCurrentItemData();
            UpdatePreview();
        }

        private void ShowPhotoWindow_Click(object sender, RoutedEventArgs e)
        {
            _photoWindow.Close();
            InitializePhotoWindow();
            _photoWindow.Show();
        }

        private void TogglePhotoWindowState_Click(object sender, RoutedEventArgs e)
        {
            _photoWindow.ToggleWindowState();
        }

        private void ClearImageButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentItem.PhotoFileName = null;
            UpdateCurrentItemData();
            UpdatePreview();
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var data = _saveFile.AuctionItems.Select(x => new
            {
                Name = x.DisplayName,
                StartingPrice = x.StartingPrice.Value,
                CurrentPrice = x.CurrentPrice.Value,
                Currency = x.StartingPrice.Currency.Name,
                Submissioner = x.Submissioner.Name,
                Winner = x.AuctionWinner.Name,
                Charity = x.ForCharityPercentage + "%",
                IsSold = x.IsSold ? "Yes" : "No",
                Photo = x.PhotoFileName
            });

            var sfd = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "CSV files|*.csv|All files|*.*"
            };

            if (sfd.ShowDialog() == true)
            {
                var csvHelperConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    Delimiter = ";"
                };

                try
                {
                    using (var writer = new StreamWriter(sfd.FileName))
                    using (var csv = new CsvWriter(writer, csvHelperConfig))
                    {
                        csv.WriteRecords(data);
                    }

                    MessageBox.Show($"Succesfully exported a report with {data.Count()} items",
                        "Crow.d Auctioner",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show($"Failed to export",
                        "Crow.d Auctioner",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
