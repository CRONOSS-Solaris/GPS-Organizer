using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Sandbox.Game.World;
using GPS_Organizer;
using Torch.Server;
using System;
using VRageMath;
using System.Text;

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
        public void AddGPSMarker(string name, string description, string colorCode = null, bool showOnHud = true, bool alwaysVisible = false, bool isObjective = false, long entityId = 0, long contractId = 0, string discardAtString = null)
        {
            var gpsOrganizerPlugin = (GpsOrganizerPlugin)Context.Plugin;
            var coords = Context.Player.GetPosition(); // Get the player's current position as the GPS marker coordinates

            // If a color code is provided, convert it to Color, otherwise use a default color
            var color = colorCode != null ? HexToColor(colorCode) : new Color(255, 255, 255);

            // Parse DiscardAt if provided
            TimeSpan? discardAt = null;
            if (!string.IsNullOrEmpty(discardAtString))
            {
                if (TimeSpan.TryParse(discardAtString, out var parsedTimeSpan))
                {
                    discardAt = parsedTimeSpan;
                }
                else
                {
                    RespondWithAuthor($"Invalid DiscardAt format. Please provide a valid TimeSpan string.");
                    return;
                }
            }

            // Add the new GPS marker
            gpsOrganizerPlugin.AddGPSMarker(name, description, coords, showOnHud, alwaysVisible, isObjective, entityId, contractId, color, discardAt);

            // Notify the player that the GPS marker has been added
            RespondWithAuthor($"GPS marker '{name}' has been added with description '{description}'.");
        }


        [Command("list", "Show the entire list of GPS markers.")]
        [Permission(MyPromoteLevel.Admin)]
        public void ListGPSMarkers()
        {
            var gpsOrganizerPlugin = (GpsOrganizerPlugin)Context.Plugin;
            var gpsMarkers = gpsOrganizerPlugin.Markers.Entries;

            if (gpsMarkers.Count == 0)
            {
                RespondWithAuthor("There are no GPS markers available.");
                return;
            }

            var gpsListBuilder = new StringBuilder();
            gpsListBuilder.AppendLine("List of GPS markers:");

            for (int i = 0; i < gpsMarkers.Count; i++)
            {
                var marker = gpsMarkers[i];
                gpsListBuilder.AppendLine($"#{i + 1}: Name: {marker.Name}, Description: {marker.Description}, Coords: {marker.Coords}");
            }

            RespondWithAuthor(gpsListBuilder.ToString());
        }

        [Command("edit", "Edit a GPS marker.")]
        [Permission(MyPromoteLevel.Admin)]
        public void EditGPSMarker(int markerIndex, string newName = null, string newDescription = null, string newColorCode = null, bool? showOnHud = null, bool? alwaysVisible = null, bool? isObjective = null, long? entityId = null, long? contractId = null)
        {
            var gpsOrganizerPlugin = (GpsOrganizerPlugin)Context.Plugin;
            var gpsMarkers = gpsOrganizerPlugin.Markers.Entries;

            if (markerIndex < 1 || markerIndex > gpsMarkers.Count)
            {
                RespondWithAuthor("Invalid marker index. Please choose a valid index.");
                return;
            }

            var marker = gpsMarkers[markerIndex - 1];

            if (newName != null)
            {
                marker.Name = newName;
                marker.DisplayName = newName;
            }

            if (newDescription != null)
            {
                marker.Description = newDescription;
            }

            if (newColorCode != null)
            {
                Color newColor;
                try
                {
                    newColor = HexToColor(newColorCode);
                }
                catch
                {
                    RespondWithAuthor($"Invalid color code: {newColorCode}");
                    return;
                }

                // Update the marker color
                marker.Color = newColor;
            }

            if (showOnHud.HasValue)
            {
                marker.ShowOnHud = showOnHud.Value;
            }

            if (alwaysVisible.HasValue)
            {
                marker.AlwaysVisible = alwaysVisible.Value;
            }

            if (isObjective.HasValue)
            {
                marker.IsObjective = isObjective.Value;
            }

            if (entityId.HasValue)
            {
                marker.EntityId = entityId.Value;
            }

            if (contractId.HasValue)
            {
                marker.ContractId = contractId.Value;
            }

            gpsOrganizerPlugin.Save();
            RespondWithAuthor($"GPS marker #{markerIndex} has been updated.");
        }
    }
}
