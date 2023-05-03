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
        private GpsOrganizerPlugin _plugin;

        public GPS_OrganizerCommands(GpsOrganizerPlugin plugin)
        {
            _plugin = plugin;
        }

        [Command("get", "Sends existing GPS markers to the player.")]
        [Permission(MyPromoteLevel.None)]
        public void GetGps()
        {
            if (Context.Player == null)
            {
                Context.Respond("This command can only be used by a player.");
                return;
            }

            var steamId = Context.Player.SteamUserId;
            var identityId = MySession.Static.Players.TryGetIdentityId(steamId);

            if (identityId == 0)
            {
                Context.Respond("Player identity not found.");
                return;
            }

            _plugin._gpsHandler.SendGPSMarkers(identityId);
            Context.Respond("Sending GPS markers to you.");
        }

        [Command("reload", "Reload the GPS markers configuration.")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadGps()
        {
            _plugin.SetupConfig();
            Context.Respond("GPS markers configuration reloaded.");
        }

        [Command("add", "Add a GPS marker at the player's current position.")]
        [Permission(MyPromoteLevel.Admin)]
        public void AddGps(string name, string description)
        {
            if (Context.Player == null)
            {
                Context.Respond("This command can only be used by a player.");
                return;
            }

            var steamId = Context.Player.SteamUserId;
            var identityId = MySession.Static.Players.TryGetIdentityId(steamId);

            if (identityId == 0)
            {
                Context.Respond("Player identity not found.");
                return;
            }

            var playerPosition = Context.Player.GetPosition();
            _plugin._gpsHandler.AddGpsMarker(name, description, playerPosition);
            _plugin.Save();
            Context.Respond($"GPS marker '{name}' added at your current position.");
        }

    }
}
