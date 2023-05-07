using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Sandbox.Game.World;
using GPS_Organizer;
using Torch.Server;
using System;
using VRageMath;

namespace GPS_Organizer
{
    [Category("gps")]
    public class GPS_OrganizerCommands : CommandModule
    {
        private void RespondWithAuthor(string message, string author = "GPS Organizer")
        {
            Context.Respond(message, author);
        }


        [Command("get", "Get all GPS markers.")]
        [Permission(MyPromoteLevel.None)]
        public void GetGPSMarkers()
        {
            var gpsOrganizerPlugin = (GpsOrganizerPlugin)Context.Plugin;
            gpsOrganizerPlugin._gpsHandler.SendGPSMarkers(Context.Player.IdentityId);

            // Notify the player that all saved GPS markers have been added.
            RespondWithAuthor("All GPS points have been re-added.");
        }

    }

    [Category("gpsadmin")]
    public class GPS_OrganizerAdminCommands : CommandModule
    {
        private void RespondWithAuthor(string message, string author = "GPS Organizer")
        {
            Context.Respond(message, author);
        }

        private Color HexToColor(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte r = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
            byte g = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
            byte b = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);

            return new Color(r, g, b);
        }

        [Command("add", "Add a new GPS marker.")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddGPSMarker(string name, string description, string colorCode = null, bool showOnHud = true, bool alwaysVisible = false, bool isObjective = false, long entityId = 0, long contractId = 0)
        {
            var gpsOrganizerPlugin = (GpsOrganizerPlugin)Context.Plugin;
            var coords = Context.Player.GetPosition(); // Get the player's current position as the GPS marker coordinates

            // If a color code is provided, convert it to Color, otherwise use a default color
            var color = colorCode != null ? HexToColor(colorCode) : new Color(255, 255, 255);

            // Add the new GPS marker
            gpsOrganizerPlugin.AddGPSMarker(name, description, coords, showOnHud, alwaysVisible, isObjective, entityId, contractId, color);

            // Notify the player that the GPS marker has been added
            RespondWithAuthor($"GPS marker '{name}' has been added with description '{description}'.");
        }
    }
}
