using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS_Organizer
{
    using NLog;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.ModAPI;
    using System.Collections.Generic;
    using VRage.Game.ModAPI;
    using VRageMath;

    namespace GPS_Organizer
    {
        public class GpsHandler
        {
            public static readonly Logger Log = LogManager.GetCurrentClassLogger();

            private GPS_OrganizerMarkersConfig _markersConfig;

            public GpsHandler(GPS_OrganizerMarkersConfig markersConfig)
            {
                _markersConfig = markersConfig;
            }

            public void SendGPSMarkers(long identityId)
            {
                if (_markersConfig == null)
                {
                    Log.Warn("Markers config not loaded. Cannot send GPS markers.");
                    return;
                }

                foreach (var entry in _markersConfig.Entries)
                {
                    var gps = new MyGps
                    {
                        Name = entry.Name,
                        Description = entry.Description,
                        Coords = entry.Coords,
                        ShowOnHud = entry.ShowOnHud,
                        GPSColor = entry.Color,
                        EntityId = entry.EntityId,
                        IsObjective = entry.IsObjective,
                        ContractId = entry.ContractId,
                        DisplayName = entry.DisplayName
                    };

                    // Send the GPS marker to the player with the specified identity ID.
                    MyAPIGateway.Session?.GPS.AddGps(identityId, gps);
                }
            }
        }
    }

}
