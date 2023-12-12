using GPS_Organizer.Utils;
using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using VRageMath;
using Xceed.Wpf.Toolkit;

namespace GPS_Organizer
{
    public partial class AddGpsDialog : Window
    {
        public event EventHandler<GpsDataEventArgs> GpsDataAdded;
        public event EventHandler<GpsDataEventArgs> GpsDataUpdated;

        private MyGpsEntry _selectedGps;

        private void ColorPicker_ColorChanged(object sender, RoutedEventArgs e)
        {
            var mediaColor = ColorPicker.SelectedColor.Value;
            var vrageColor = new VRageMath.Color(mediaColor.R, mediaColor.G, mediaColor.B, mediaColor.A);
        }

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
                ShowOnHudCheckBox.IsChecked = selectedGps.ShowOnHud;
                AlwaysVisibleCheckBox.IsChecked = selectedGps.AlwaysVisible;
                IsObjectiveCheckBox.IsChecked = selectedGps.IsObjective;
                EntityIdTextBox.Text = selectedGps.EntityId.ToString();
                ContractIdTextBox.Text = selectedGps.ContractId.ToString();
                ColorPicker.SelectedColor = new System.Windows.Media.Color
                {
                    A = _selectedGps.Color.A,
                    R = _selectedGps.Color.R,
                    G = _selectedGps.Color.G,
                    B = _selectedGps.Color.B
                };
                DiscardAtTimePicker.Value = _selectedGps.DiscardAt.HasValue
                    ? DateTime.Today.Add(_selectedGps.DiscardAt.Value)
                    : (DateTime?)null;
            }
            else
            {
                ShowOnHudCheckBox.IsChecked = true;
                EntityIdTextBox.Text = "0";
                ContractIdTextBox.Text = "0";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please enter a name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(XCoordTextBox.Text, out double x) ||
                !double.TryParse(YCoordTextBox.Text, out double y) ||
                !double.TryParse(ZCoordTextBox.Text, out double z))
            {
                System.Windows.MessageBox.Show("Invalid coordinates input. Please enter valid numbers.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(EntityIdTextBox.Text, out long entityId))
            {
                System.Windows.MessageBox.Show("Invalid Entity ID input. Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(ContractIdTextBox.Text, out long contractId))
            {
                System.Windows.MessageBox.Show("Invalid Contract ID input. Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            VRageMath.Color vrageColor;
            if (ColorPicker.SelectedColor.HasValue)
            {
                var mediaColor = ColorPicker.SelectedColor.Value;
                vrageColor = new VRageMath.Color(mediaColor.R, mediaColor.G, mediaColor.B, mediaColor.A);
            }
            else
            {
                vrageColor = VRageMath.Color.White; // default value or another color you'd like
            }

            TimeSpan? discardAt = DiscardAtTimePicker.Value?.TimeOfDay;

            var gpsData = new GpsData
            {
                Name = NameTextBox.Text,
                Description = DescriptionTextBox.Text,
                Coords = new Vector3D(x, y, z),
                DiscardAt = (TimeSpan)discardAt,
                ShowOnHud = ShowOnHudCheckBox.IsChecked.GetValueOrDefault(),
                AlwaysVisible = AlwaysVisibleCheckBox.IsChecked.GetValueOrDefault(),
                IsObjective = IsObjectiveCheckBox.IsChecked.GetValueOrDefault(),
                EntityId = entityId,
                ContractId = contractId,
                Color = vrageColor
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

        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DescriptionTextBox.LineCount > 1)
            {
                this.Height = MinHeight + (DescriptionTextBox.LineCount - 1) * DescriptionTextBox.FontSize;
            }
        }

        private void OnParseGpsStringButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the GPS string from the text box
                var gpsString = GpsStringInput.Text;

                // Parse the GPS string using the correct class and method
                var parsedData = OnParseGpsString.ParseGpsString(gpsString);

                // Use the parsed data to populate the fields in the dialog
                NameTextBox.Text = parsedData.Name;
                XCoordTextBox.Text = parsedData.XCoord.ToString();
                YCoordTextBox.Text = parsedData.YCoord.ToString();
                ZCoordTextBox.Text = parsedData.ZCoord.ToString();
                ColorPicker.SelectedColor = parsedData.Color;
            }
            catch (FormatException ex)
            {
                // Handle invalid GPS string format
                System.Windows.MessageBox.Show("Invalid GPS string format: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other errors
                System.Windows.MessageBox.Show("An error occurred while parsing the GPS string: " + ex.Message);
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
            public TimeSpan DiscardAt { get; set; }
            public bool ShowOnHud { get; set; }
            public bool AlwaysVisible { get; set; }
            public bool IsObjective { get; set; }
            public long EntityId { get; set; }
            public long ContractId { get; set; }
            public Color Color { get; set; }
        }
    }
}