using System.Xml;
using System.Xml.Serialization;
using Torch;

public class GPS_OrganizerConfig : ViewModel
{
    private bool _sendMarkerOnJoin = true;
    private bool _debugMode = false;

    public bool SendMarkerOnJoin { get => _sendMarkerOnJoin; set => SetValue(ref _sendMarkerOnJoin, value); }
    public bool DebugMode { get => _debugMode; set => SetValue(ref _debugMode, value); }

}
