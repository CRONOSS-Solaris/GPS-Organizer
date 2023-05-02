using System;
using System.Windows;
using System.Windows.Controls;

namespace GPS_Organizer
{
    public partial class AddGpsDialog : Window
    {
        public event EventHandler<GpsDataEventArgs> GpsDataAdded;

        public AddGpsDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var gpsData = new GpsData
            {
                Name = NameTextBox.Text,
                Coords = CoordsTextBox.Text,
                Description = DescriptionTextBox.Text
            };

            GpsDataAdded?.Invoke(this, new GpsDataEventArgs(gpsData));
        }
    }

    public class GpsDataEventArgs : EventArgs
    {
        public GpsData GpsData { get; }

        public GpsDataEventArgs(GpsData gpsData)
        {
            GpsData = gpsData;
        }
    }

    public class GpsData
    {
        public string Name { get; set; }
        public string Coords { get; set; }
        public string Description { get; set; }
    }
}
