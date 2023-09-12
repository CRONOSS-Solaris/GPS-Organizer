using GPS_Organizer.Utils;
using System;
using System.Diagnostics.Contracts;
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

        private void ColorPicker_ColorChanged(object sender, RoutedEventArgs e)
        {
            // Pobierz kolor z ColorPicker
            var mediaColor = ColorPicker.SelectedColor.Value;

            // Konwertuj System.Windows.Media.Color na VRageMath.Color
            var vrageColor = new VRageMath.Color(mediaColor.R, mediaColor.G, mediaColor.B, mediaColor.A);

            // Przypisz vrageColor do odpowiedniego pola/obiektu w Twoim kodzie
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

            }
            else // if adding a new GPS entry
            {
                ShowOnHudCheckBox.IsChecked = true; // set ShowOnHud to true by default
                EntityIdTextBox.Text = "0";
                ContractIdTextBox.Text = "0";
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double x, y, z;

            if (!double.TryParse(XCoordTextBox.Text, out x) ||
                !double.TryParse(YCoordTextBox.Text, out y) ||
                !double.TryParse(ZCoordTextBox.Text, out z))
            {
                MessageBox.Show("Invalid coordinates input. Please enter valid numbers.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(EntityIdTextBox.Text, out long entityId))
            {
                MessageBox.Show("Invalid Entity ID input. Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(ContractIdTextBox.Text, out long contractId))
            {
                MessageBox.Show("Invalid Contract ID input. Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gpsData = new GpsData
            {
                Name = NameTextBox.Text,
                Description = DescriptionTextBox.Text,
                Coords = new Vector3D(x, y, z),
                ShowOnHud = ShowOnHudCheckBox.IsChecked.GetValueOrDefault(),
                AlwaysVisible = AlwaysVisibleCheckBox.IsChecked.GetValueOrDefault(),
                IsObjective = IsObjectiveCheckBox.IsChecked.GetValueOrDefault(),
                EntityId = entityId,
                ContractId = contractId,
                Color = new VRageMath.Color
                {
                    A = ColorPicker.SelectedColor.Value.A,
                    R = ColorPicker.SelectedColor.Value.R,
                    G = ColorPicker.SelectedColor.Value.G,
                    B = ColorPicker.SelectedColor.Value.B
                }

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
            // Adjust window height based on DescriptionTextBox content
            if (DescriptionTextBox.LineCount > 1)
            {
                this.Height = MinHeight + (DescriptionTextBox.LineCount - 1) * DescriptionTextBox.FontSize;
            }
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
        public bool ShowOnHud { get; set; }
        public bool AlwaysVisible { get; set; }
        public bool IsObjective { get; set; }
        public long EntityId { get; set; }
        public long ContractId { get; set; }
        public Color Color { get; set; }

    }

    private void OnParseGpsStringButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the GPS string from the text box
            var gpsString = GpsStringInput.Text;

            // Parse the GPS string
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
            MessageBox.Show("Invalid GPS string format: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Handle other errors
            MessageBox.Show("An error occurred while parsing the GPS string: " + ex.Message);
        }
    }
}