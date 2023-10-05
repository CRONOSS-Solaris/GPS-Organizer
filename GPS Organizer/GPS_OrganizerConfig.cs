using System.Xml;
using System.Xml.Serialization;
using Torch;

public class GPS_OrganizerConfig : ViewModel
{
    private bool _sendMarkerOnJoin = true;
    private bool _debugMode = false;

    public bool SendMarkerOnJoin { get => _sendMarkerOnJoin; set => SetValue(ref _sendMarkerOnJoin, value); }
    public bool DebugMode { get => _debugMode; set => SetValue(ref _debugMode, value); }

    public void Save(string filePath)
    {
        using (var writer = new XmlTextWriter(filePath, System.Text.Encoding.UTF8))
        {
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();

            writer.WriteComment("DebugMode: When set to true, additional logging information will be output to assist with debugging. Useful for tracking the behavior and state of the plugin in detail.");

            var serializer = new XmlSerializer(typeof(GPS_OrganizerConfig));
            serializer.Serialize(writer, this);

            writer.WriteEndDocument();
        }
    }
}
