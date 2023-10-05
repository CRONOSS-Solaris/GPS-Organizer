using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace GPS_Organizer
{
    public class MyGpsEntry
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Vector3D Coords { get; set; }
        public TimeSpan? DiscardAt { get; set; }
        public bool ShowOnHud { get; set; }
        public bool AlwaysVisible { get; set; }
        public Color Color { get; set; }
        public long EntityId { get; set; }
        public bool IsObjective { get; set; }
        public long ContractId { get; set; }
        public string DisplayName { get; set; }
    }

}
