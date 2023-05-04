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
            }
            else // if adding a new GPS entry
            {
                ShowOnHudCheckBox.IsChecked = true; // set ShowOnHud to true by default
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
    }
}
