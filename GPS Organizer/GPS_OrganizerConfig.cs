using System;
using System.Collections.Generic;
using Torch;

namespace GPS_Organizer
{
    public class GPS_OrganizerConfig : ViewModel
    {
        private bool _sendMarkerOnJoin = true;
        public bool SendMarkerOnJoin { get => _sendMarkerOnJoin; set => SetValue(ref _sendMarkerOnJoin, value); }
    }
}
