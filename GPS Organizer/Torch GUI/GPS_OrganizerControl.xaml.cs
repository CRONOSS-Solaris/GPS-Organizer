using GPS_Organizer;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GPS_Organizer
{
    public partial class GPS_OrganizerControl : UserControl
    {

        private GpsOrganizerPlugin _plugin;

        public GPS_OrganizerControl(GpsOrganizerPlugin plugin)
        {
            InitializeComponent();
            _plugin = plugin;
            RefreshGpsList();
        }

        public void RefreshGpsList()
        {
            GpsListBox.ItemsSource = _plugin.Markers.Entries;
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
            if (GpsListBox.SelectedItem == null) return;

            var selectedGps = GpsListBox.SelectedItem as MyGpsEntry;
            var editGpsDialog = new AddGpsDialog(selectedGps);
            editGpsDialog.GpsDataUpdated += EditGpsDialog_GpsDataUpdated;
            editGpsDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            editGpsDialog.ShowDialog();
        }

        private void DeleteGpsButton_Click(object sender, RoutedEventArgs e)
        {
            if (GpsListBox.SelectedItem == null) return;

            var selectedGps = GpsListBox.SelectedItem as MyGpsEntry;
            _plugin.Markers.Entries.Remove(selectedGps);
            _plugin.Save();
            RefreshGpsList();
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            string discordInviteLink = "https://discord.com/invite/TqbCaHu7wr";
            Process.Start(new ProcessStartInfo
            {
                FileName = discordInviteLink,
                UseShellExecute = true
            });
        }

        private void EditGpsDialog_GpsDataUpdated(object sender, GpsDataEventArgs e)
        {
            if (GpsListBox.SelectedItem == null) return;

            var selectedGps = GpsListBox.SelectedItem as MyGpsEntry;

            selectedGps.Name = e.GpsData.Name;
            selectedGps.Description = e.GpsData.Description;
            selectedGps.Coords = e.GpsData.Coords;

            _plugin.Save();
            RefreshGpsList();
        }


        private void AddGpsDialog_GpsDataAdded(object sender, GpsDataEventArgs e)
        {
            // Dodaj nowy GPS do listy, używając danych z e.GpsData
            // Na przykład:
            var gpsData = e.GpsData;
            _plugin.AddGPSMarker(gpsData.Name, gpsData.Description, gpsData.Coords);
            _plugin.Save();
        }
    }

}
