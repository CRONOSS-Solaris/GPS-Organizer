using GPS_Organizer;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VRage.Plugins;
using static GPS_Organizer.AddGpsDialog;

namespace GPS_Organizer
{
    public partial class GPS_OrganizerControl : UserControl
    {

        private GpsOrganizerPlugin _plugin;

        public GPS_OrganizerConfig PluginConfig { get; private set; }


        public GPS_OrganizerControl(GpsOrganizerPlugin plugin)
        {
            InitializeComponent();
            _plugin = plugin;
            DataContext = _plugin.Config;
            PluginConfig = plugin.Config;
            RefreshGpsList();
        }

        public void RefreshGpsList()
        {
            GpsDataGrid.ItemsSource = _plugin.Markers.Entries;
        }

        private void AddGpsButton_Click(object sender, RoutedEventArgs e)
        {
            var addGpsDialog = new AddGpsDialog(null); // Przekazujemy null jako parametr
            addGpsDialog.GpsDataAdded += AddGpsDialog_GpsDataAdded;
            addGpsDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            addGpsDialog.ShowDialog();
        }

        private void EditGpsButton_Click(object sender, RoutedEventArgs e)
        {
            if (GpsDataGrid.SelectedItem == null) return;

            var selectedGps = GpsDataGrid.SelectedItem as MyGpsEntry;
            var editGpsDialog = new AddGpsDialog(selectedGps);
            editGpsDialog.GpsDataUpdated += EditGpsDialog_GpsDataUpdated;
            editGpsDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            editGpsDialog.ShowDialog();
        }

        private void DeleteGpsButton_Click(object sender, RoutedEventArgs e)
        {
            if (GpsDataGrid.SelectedItem == null) return;

            var selectedGps = GpsDataGrid.SelectedItem as MyGpsEntry;
            _plugin.Markers.Entries.Remove(selectedGps);
            _plugin.Save();
            RefreshGpsList();
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            string discordInviteLink = "https://discord.gg/BUnUnXz5xJ";
            Process.Start(new ProcessStartInfo
            {
                FileName = discordInviteLink,
                UseShellExecute = true
            });
        }

        private void EditGpsDialog_GpsDataUpdated(object sender, GpsDataEventArgs e)
        {
            if (GpsDataGrid.SelectedItem == null) return;

            var selectedGps = GpsDataGrid.SelectedItem as MyGpsEntry;

            // Usuń stary wpis z listy
            _plugin.Markers.Entries.Remove(selectedGps);

            // Stwórz nowy wpis z zaktualizowanymi danymi
            var updatedGps = new MyGpsEntry
            {
                Name = e.GpsData.Name,
                Description = e.GpsData.Description,
                Coords = e.GpsData.Coords,
                ShowOnHud = e.GpsData.ShowOnHud,
                AlwaysVisible = e.GpsData.AlwaysVisible,
                IsObjective = e.GpsData.IsObjective,
                EntityId = e.GpsData.EntityId,
                ContractId = e.GpsData.ContractId,
                Color = e.GpsData.Color,
            };

            // Dodaj zaktualizowany wpis do listy
            _plugin.Markers.Entries.Add(updatedGps);

            _plugin.Save();
            RefreshGpsList();
        }



        private void AddGpsDialog_GpsDataAdded(object sender, GpsDataEventArgs e)
        {
            // Dodaj nowy GPS do listy, używając danych z e.GpsData
            // Na przykład:
            var gpsData = e.GpsData;
            _plugin.AddGPSMarker(gpsData.Name, gpsData.Description, gpsData.Coords, gpsData.ShowOnHud, gpsData.AlwaysVisible, gpsData.IsObjective, gpsData.EntityId, gpsData.ContractId, gpsData.Color);
            _plugin.Save();
        }

        private void SendMarkerOnJoinToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PluginConfig != null)
            {
                PluginConfig.SendMarkerOnJoin = true;
                _plugin.Save();
            }
        }

        private void SendMarkerOnJoinToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PluginConfig != null)
            {
                PluginConfig.SendMarkerOnJoin = false;
                _plugin.Save();
            }
        }

        private void ListBoxItem_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                TextBlock descriptionTextBlock = FindTextBlockInVisualTree(item, "DESCRIPTION: ");
                if (descriptionTextBlock != null)
                {
                    ToolTip tt = descriptionTextBlock.ToolTip as ToolTip;
                    if (tt != null)
                    {
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void ListBoxItem_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                TextBlock descriptionTextBlock = FindTextBlockInVisualTree(item, "DESCRIPTION: ");
                if (descriptionTextBlock != null)
                {
                    ToolTip tt = descriptionTextBlock.ToolTip as ToolTip;
                    if (tt != null)
                    {
                        tt.IsOpen = false;
                    }
                }
            }
        }


        private TextBlock FindTextBlockInVisualTree(DependencyObject parent, string text)
        {
            if (parent == null) return null;

            TextBlock foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                TextBlock childOfType = child as TextBlock;
                if (childOfType != null && childOfType.Text.StartsWith(text))
                {
                    foundChild = childOfType;
                    break;
                }

                foundChild = FindTextBlockInVisualTree(child, text);

                if (foundChild != null) break;
            }

            return foundChild;
        }

    }

}
