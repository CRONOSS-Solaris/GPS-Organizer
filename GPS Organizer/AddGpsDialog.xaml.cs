using System;
using System.Windows;
using System.Windows.Controls;
using VRageMath;

namespace GPS_Organizer
{
    public partial class AddGpsDialog : Window
    {
        public event EventHandler<GpsDataEventArgs> GpsDataAdded;
        public event EventHandler<GpsDataEventArgs> GpsDataUpdated;

        private MyGpsEntry _selectedGps;

        public AddGpsDialog(MyGpsEntry selectedGps)
        {
            InitializeComponent();

            _selectedGps = selectedGps;

            if (_selectedGps != null)
            {
                NameTextBox.Text = selectedGps.Name;
                DescriptionTextBox.Text = selectedGps.Description;
                XCoordTextBox.Text = selectedGps.Coords.X.ToString();
                YCoordTextBox.Text = selectedGps.Coords.Y.ToString();
                ZCoordTextBox.Text = selectedGps.Coords.Z.ToString();
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            double x, y, z;

            if (!double.TryParse(XCoordTextBox.Text, out x) ||
                !double.TryParse(YCoordTextBox.Text, out y) ||
                !double.TryParse(ZCoordTextBox.Text, out z))
            {
                MessageBox.Show("Invalid coordinates input. Please enter valid numbers.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gpsData = new GpsData
            {
                Name = NameTextBox.Text,
                Description = DescriptionTextBox.Text,
                Coords = new Vector3D(x, y, z)
            };

            if (_selectedGps == null)
            {
                GpsDataAdded?.Invoke(this, new GpsDataEventArgs(gpsData));
            }
            else
            {
                GpsDataUpdated?.Invoke(this, new GpsDataEventArgs(gpsData));
            }

            Close();
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
        public Vector3D Coords { get; set; }
        public string Description { get; set; }
    }
}
