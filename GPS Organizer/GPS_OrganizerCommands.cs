﻿using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Sandbox.Game.World;
using GPS_Organizer;

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
    }
}
