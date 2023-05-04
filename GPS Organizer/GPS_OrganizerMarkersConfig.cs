using System.Collections.ObjectModel;
using Torch;

namespace GPS_Organizer
{
    public class GPS_OrganizerMarkersConfig : ViewModel
    {
        private ObservableCollection<MyGpsEntry> _entries = new ObservableCollection<MyGpsEntry>();
        public ObservableCollection<MyGpsEntry> Entries { get => _entries; set => SetValue(ref _entries, value); }

        public GPS_OrganizerMarkersConfig()
        {
            Entries.CollectionChanged += (sender, args) => OnPropertyChanged();
        }
    }
}
