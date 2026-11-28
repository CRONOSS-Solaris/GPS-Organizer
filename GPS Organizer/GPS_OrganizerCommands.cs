using Torch.Commands;
using Torch.API.Managers;
using Torch.API.Session;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Sandbox.Game.World;
using GPS_Organizer;
using Torch.Server;
using System;
using System.Collections.Generic;
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

        [Command("send", "Send all GPS markers to the named player.")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendGPSMarkersToPlayer(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                RespondWithAuthor($"Usage: {GpsCommandFallbacks.CommandPrefix}gpsadmin send <playerName>");
                return;
            }

            var gpsOrganizerPlugin = (GpsOrganizerPlugin)Context.Plugin;
            long identityId = 0;
            string resolvedName = playerName.Trim();

            ulong steamId;
            if (ulong.TryParse(resolvedName, out steamId) && steamId > 0)
                identityId = MySession.Static?.Players?.TryGetIdentityId(steamId) ?? 0;

            if (identityId == 0 && MySession.Static?.Players != null)
            {
                var players = MySession.Static.Players;

                var onlineMatches = new List<MyPlayer>();
                foreach (var player in players.GetOnlinePlayers())
                {
                    if (player == null || player.Identity == null)
                        continue;
                    if (players.TryGetSteamId(player.Identity.IdentityId) == 0)
                        continue;
                    if (string.Equals(player.DisplayName, resolvedName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(player.Identity.DisplayName, resolvedName, StringComparison.OrdinalIgnoreCase))
                        onlineMatches.Add(player);
                }

                if (onlineMatches.Count > 1)
                {
                    RespondWithAuthor($"Multiple identities match '{resolvedName}'. Please use the player's Steam ID.");
                    return;
                }

                if (onlineMatches.Count == 1)
                {
                    identityId = onlineMatches[0].Identity.IdentityId;
                    resolvedName = !string.IsNullOrEmpty(onlineMatches[0].DisplayName)
                        ? onlineMatches[0].DisplayName
                        : onlineMatches[0].Identity.DisplayName;
                }
                else
                {
                    var matches = new List<MyIdentity>();
                    foreach (var ident in players.GetAllIdentities())
                    {
                        if (ident?.DisplayName == null)
                            continue;
                        if (players.TryGetSteamId(ident.IdentityId) == 0)
                            continue;
                        if (string.Equals(ident.DisplayName, resolvedName, StringComparison.OrdinalIgnoreCase))
                            matches.Add(ident);
                    }

                    if (matches.Count > 1)
                    {
                        RespondWithAuthor($"Multiple identities match '{resolvedName}'. Please use the player's Steam ID.");
                        return;
                    }

                    if (matches.Count == 1)
                    {
                        identityId = matches[0].IdentityId;
                        resolvedName = matches[0].DisplayName;
                    }
                }
            }

            string reason;
            if (!gpsOrganizerPlugin.TrySendMarkersToIdentity(identityId, out reason))
            {
                if (identityId == 0)
                    RespondWithAuthor($"Player '{playerName}' was not found.");
                else
                    RespondWithAuthor($"Could not send GPS markers to '{resolvedName}': {reason}.");
                return;
            }

            var isOnline = MySession.Static?.Players != null && MySession.Static.Players.IsPlayerOnline(identityId);
            if (isOnline)
            {
                GpsOrganizerPlugin.Log.Info($"GPS Organizer: admin sent markers to {resolvedName} (identity {identityId}).");
                RespondWithAuthor($"GPS markers have been sent to {resolvedName}.");
            }
            else
            {
                GpsOrganizerPlugin.Log.Info($"GPS Organizer: admin saved markers for offline player {resolvedName} (identity {identityId}). They will appear when the player next logs in.");
                RespondWithAuthor($"GPS markers have been saved for {resolvedName} and will appear when they next log in.");
            }
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

    internal static class GpsCommandFallbacks
    {
        private static IChatManagerServer _chatManager;
        private static CommandManager _commandManager;

        internal static char CommandPrefix
        {
            get { return _commandManager != null ? _commandManager.Prefix : '!'; }
        }

        public static void Subscribe(ITorchSession session)
        {
            Unsubscribe();
            if (session == null || session.Managers == null)
                return;

            _commandManager = session.Managers.GetManager<CommandManager>();
            _chatManager = session.Managers.GetManager<IChatManagerServer>();
            if (_chatManager != null)
                _chatManager.MessageProcessing += OnMessageProcessing;
        }

        public static void Unsubscribe()
        {
            if (_chatManager != null)
            {
                _chatManager.MessageProcessing -= OnMessageProcessing;
                _chatManager = null;
            }
            _commandManager = null;
        }

        private static void OnMessageProcessing(TorchChatMessage msg, ref bool consumed)
        {
            if (_chatManager == null || string.IsNullOrEmpty(msg.Message))
                return;

            var prefix = CommandPrefix;
            if (msg.Message[0] != prefix)
                return;

            var parts = msg.Message.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            var category = parts[0];
            if (!IsGpsCategory(category))
                return;

            var unknown = parts.Length > 1 ? parts[1] : null;
            if (!string.IsNullOrEmpty(unknown) && IsValidSubcommand(category, unknown))
                return;

            if (!msg.AuthorSteamId.HasValue)
                return;

            consumed = true;
            var promoteLevel = MySession.Static != null
                ? MySession.Static.GetUserPromoteLevel(msg.AuthorSteamId.Value)
                : MyPromoteLevel.None;
            _chatManager.SendMessageAsOther("GPS Organizer", FormatReply(category, unknown, promoteLevel), default(Color), msg.AuthorSteamId.Value);
        }

        private static bool IsGpsCategory(string category)
        {
            return string.Equals(category, "gps", StringComparison.OrdinalIgnoreCase)
                || string.Equals(category, "gpsadmin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsValidSubcommand(string category, string subcommand)
        {
            foreach (var name in GetValidSubcommands(category, null))
            {
                if (string.Equals(name, subcommand, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static List<string> GetValidSubcommands(string category, MyPromoteLevel? promoteLevel)
        {
            var names = new List<string>();
            var lookedUpTree = false;
            if (_commandManager != null)
            {
                CommandTree.CommandNode node;
                var path = new List<string> { category };
                if (_commandManager.Commands.GetNode(path, out node) < 0 || node == null)
                {
                    path[0] = category.ToLowerInvariant();
                    if (_commandManager.Commands.GetNode(path, out node) < 0)
                        node = null;
                }
                if (node != null)
                {
                    lookedUpTree = true;
                    foreach (var child in node.GetChildren())
                    {
                        if (!child.IsCommand)
                            continue;
                        if (promoteLevel.HasValue && promoteLevel.Value < child.Command.MinimumPromoteLevel)
                            continue;
                        names.Add(child.Name);
                    }
                }
            }
            if (!lookedUpTree && names.Count == 0)
            {
                if (string.Equals(category, "gpsadmin", StringComparison.OrdinalIgnoreCase))
                {
                    if (!promoteLevel.HasValue || promoteLevel.Value >= MyPromoteLevel.Admin)
                    {
                        names.Add("add");
                        names.Add("edit");
                        names.Add("list");
                        names.Add("send");
                    }
                }
                else
                {
                    names.Add("get");
                }
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        private static string FormatReply(string category, string unknown, MyPromoteLevel promoteLevel)
        {
            var prefix = CommandPrefix;
            var listItems = new List<string>();
            foreach (var name in GetValidSubcommands(category, promoteLevel))
                listItems.Add(prefix + category + " " + name);
            var list = listItems.Count > 0 ? string.Join(", ", listItems) : null;

            if (string.IsNullOrEmpty(unknown))
            {
                if (list == null)
                    return $"Usage: {prefix}{category} <command>.";
                return $"Usage: {prefix}{category} <command>. Valid commands: {list}.";
            }
            if (list == null)
                return $"Unknown command '{prefix}{category} {unknown}'.";
            return $"Unknown command '{prefix}{category} {unknown}'. Valid commands: {list}.";
        }
    }
}
