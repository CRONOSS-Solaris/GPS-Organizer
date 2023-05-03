using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Sandbox.Game.World;
using GPS_Organizer;
using Torch.Server;

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
}
