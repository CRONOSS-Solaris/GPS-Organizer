using GPS_Organizer;
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
            var addGpsDialog = new AddGpsDialog();
            addGpsDialog.GpsDataAdded += AddGpsDialog_GpsDataAdded;
            addGpsDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            addGpsDialog.ShowDialog();
        }


        private void AddGpsDialog_GpsDataAdded(object sender, GpsDataEventArgs e)
        {
            // Dodaj nowy GPS do listy, używając danych z e.GpsData
            // Na przykład:
            var gpsData = e.GpsData;
            _plugin.AddGPSMarker(gpsData.Name, gpsData.Coords, gpsData.Description);
        }

    }
}
