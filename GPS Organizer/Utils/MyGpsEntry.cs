using System;
using System.Xml;
using System.Xml.Serialization;
using VRageMath;

namespace GPS_Organizer
{
    public class MyGpsEntry
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Vector3D Coords { get; set; }

        [XmlIgnore]
        public TimeSpan? DiscardAt { get; set; }

        // Serialized as a string for XML, include even if null
        [XmlElement("DiscardAt", IsNullable = true)]
        public string DiscardAtString
        {
            get => DiscardAt.HasValue ? XmlConvert.ToString(DiscardAt.Value) : null;
            set => DiscardAt = string.IsNullOrEmpty(value) ? (TimeSpan?)null : XmlConvert.ToTimeSpan(value);
        }

        public bool ShowOnHud { get; set; }
        public bool AlwaysVisible { get; set; }
        public Color Color { get; set; }
        public long EntityId { get; set; }
        public bool IsObjective { get; set; }
        public long ContractId { get; set; }
        public string DisplayName { get; set; }
    }
}