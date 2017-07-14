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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Timers;

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
        private Timer _refreshTimer;
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
            _refreshTimer = new Timer(333);
            _refreshTimer.Elapsed += (a, b) => Dispatcher.BeginInvoke(new Action(ShowPreview));
            _refreshTimer.Start();
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
            PresenterPreviewImage.Source = null;
            _presentationWindow.DisplayItem(CurrentItem);
            _photoWindow.DisplayItem(CurrentItem);
        }

        private void ShowPreview()
        {
            if (_presentationWindow.MainGrid.ActualWidth <= 0)
            {
                PresenterPreviewImage.Source = null;
                PreviewBorder.BorderBrush = Brushes.Red;
                return;
            }

            try
            {
                PreviewBorder.BorderBrush = Brushes.Blue;

                PreviewBorder.Background = _presentationWindow.Background;

                var rtb = new RenderTargetBitmap(
                      (int)_presentationWindow.MainGrid.ActualWidth,
                      (int)_presentationWindow.MainGrid.ActualHeight,
                      96,
                      96,
                      PixelFormats.Pbgra32);
                rtb.Render(_presentationWindow.MainGrid);
                PresenterPreviewImage.Source = rtb;

                PreviewBorder.BorderBrush = Brushes.Black;
            }
            catch (Exception exc)
            {
                PreviewBorder.BorderBrush = Brushes.Red;
            }
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
    }
}
