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
using Crow.Auctioner.DataStorage;

namespace Crow.Auctioner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PresentationWindow _presentationWindow;
        SaveFile _saveFile;
        static int _selectedIndex;
        bool _isLoading = false;

        AuctionItem CurrentItem { get { return ItemsDataGrid.SelectedItem as AuctionItem; } }

        public MainWindow()
        {
            InitializeComponent();
            InitializePresentationWindow();
            LoadSaveFile();

            _selectedIndex = -1;
        }

        void InitializePresentationWindow()
        {
            _presentationWindow = new PresentationWindow();
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
                _presentationWindow.DisplayItem(CurrentItem);

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
            _presentationWindow.DisplayItem(CurrentItem);
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
            _presentationWindow.DisplayItem(CurrentItem);
        }

        private void ItemSoldCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            CurrentItem.IsSold = false;
            UpdateCurrentItemData();
            _presentationWindow.DisplayItem(CurrentItem);
        }
    }
}
