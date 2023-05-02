using System.Windows;
using System.Windows.Controls;

namespace GPS_Organizer
{
    public partial class GPS_OrganizerControl : UserControl
    {

        private GPS_Organizer Plugin { get; }

        private GPS_OrganizerControl()
        {
            InitializeComponent();
        }

        public GPS_OrganizerControl(GPS_Organizer plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }
    }
}
