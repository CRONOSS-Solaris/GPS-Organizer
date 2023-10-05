using GPS_Organizer.Utils;
using NLog;
using Sandbox.Engine.Utils;
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
        private GPS_OrganizerConfig _config;

        public GpsHandler(GPS_OrganizerConfig config, GPS_OrganizerMarkersConfig markersConfig)
        {
            _config = config;
            _markersConfig = markersConfig;
            LoggerHelper.DebugLog(Log, _config, "GpsHandler() - Constructor with both config and markersConfig invoked");
        }

        public void SendGPSMarkers(long identityId)
        {
            LoggerHelper.DebugLog(Log, _config, $"SendGPSMarkers() - Start sending markers for identity: {identityId}");

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
                    AlwaysVisible = entry.AlwaysVisible,
                    GPSColor = entry.Color,
                    EntityId = entry.EntityId,
                    IsObjective = entry.IsObjective,
                    ContractId = entry.ContractId,
                    DisplayName = entry.DisplayName,
                    //DiscardAt = null
                };


                LoggerHelper.DebugLog(Log, _config, $"SendGPSMarkers() - Creating marker: {entry.Name}");

                // Send the GPS marker to the player with the specified identity ID.
                MyAPIGateway.Session?.GPS.AddGps(identityId, gps);

                LoggerHelper.DebugLog(Log, _config, $"SendGPSMarkers() - Sent marker: {entry.Name}");
            }

            LoggerHelper.DebugLog(Log, _config, $"SendGPSMarkers() - Finished sending markers for identity: {identityId}");
        }
    }
}