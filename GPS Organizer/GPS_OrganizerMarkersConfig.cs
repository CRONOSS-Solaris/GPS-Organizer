using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using VRage.Collections;

namespace GPS_Organizer
{
    public class GPS_OrganizerMarkersConfig : ViewModel
    {
        private ObservableCollection<MyGpsEntry> _entries = new ObservableCollection<MyGpsEntry>();
        public ObservableCollection<MyGpsEntry> Entries { get => _entries; set => SetValue(ref _entries, value); }
    }
}
