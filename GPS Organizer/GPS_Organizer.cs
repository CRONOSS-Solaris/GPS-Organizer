using NLog;
using Sandbox.Game;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using VRageMath;
using Torch.Commands;
using GPS_Organizer.Utils;

namespace GPS_Organizer
{
    public class GpsOrganizerPlugin : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string CONFIG_FILE_NAME = "GPS_OrganizerConfig.cfg";
        private static readonly string CONFIG_FILE_MARKERS = "GPS_OrganizerMarkers.cfg";
        private const int PendingRetryIntervalMs = 3000;
        private const int PendingRetryTimeoutMinutes = 20;

        private IMultiplayerManagerBase _multibase;
        private GPS_OrganizerControl _control;
        private readonly ConcurrentDictionary<ulong, PendingMarkerSend> _pendingMarkerSends = new ConcurrentDictionary<ulong, PendingMarkerSend>();
        private readonly ConcurrentDictionary<ulong, byte> _sentThisConnection = new ConcurrentDictionary<ulong, byte>();
        private Timer _pendingJoinTimer;
        private volatile bool _sessionUnloading;

        public UserControl GetControl() => _control ?? (_control = new GPS_OrganizerControl(this));

        private Persistent<GPS_OrganizerConfig> _config;
        public GPS_OrganizerConfig Config => _config?.Data;

        private Persistent<GPS_OrganizerMarkersConfig> _markersConfig;
        public GPS_OrganizerMarkersConfig Markers => _markersConfig?.Data;
        public GpsHandler _gpsHandler;

        private class PendingMarkerSend
        {
            public string Name { get; set; }
            public DateTime QueuedAtUtc { get; set; }
            public bool QueuedLogged { get; set; }
        }

        public override void Init(ITorchBase torch)
        {
            
            base.Init(torch);
            SetupConfig();
            LoggerHelper.DebugLog(Log, _config.Data, "Init() - Start");

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                LoggerHelper.DebugLog(Log, _config.Data, "Init() - No session manager loaded!");

            Save();
            LoggerHelper.DebugLog(Log, _config.Data, "Init() - End");
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            LoggerHelper.DebugLog(Log, _config.Data, $"SessionChanged() - Session state changed to: {state}");

            switch (state)
            {
                case TorchSessionState.Loaded:
                    _sessionUnloading = false;
                    UnsubscribeSessionEvents();
                    _pendingMarkerSends.Clear();
                    _sentThisConnection.Clear();
                    _multibase = Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>();
                    if (_multibase != null)
                    {
                        _multibase.PlayerJoined += multibase_PlayerJoined;
                        _multibase.PlayerLeft += multibase_PlayerLeft;
                    }
                    else
                        LoggerHelper.DebugLog(Log, _config.Data, "SessionChanged() - No multiplayer manager loaded!");
                    MyVisualScriptLogicProvider.PlayerConnected += OnPlayerConnected;
                    MyVisualScriptLogicProvider.PlayerSpawned += OnPlayerSpawned;
                    StartPendingJoinTimer();
                    GpsCommandFallbacks.Subscribe(session);
                    break;
                case TorchSessionState.Unloading:
                    _sessionUnloading = true;
                    StopPendingJoinTimer();
                    UnsubscribeSessionEvents();
                    _pendingMarkerSends.Clear();
                    _sentThisConnection.Clear();
                    break;
            }
        }

        private void UnsubscribeSessionEvents()
        {
            if (_multibase != null)
            {
                _multibase.PlayerJoined -= multibase_PlayerJoined;
                _multibase.PlayerLeft -= multibase_PlayerLeft;
            }
            MyVisualScriptLogicProvider.PlayerConnected -= OnPlayerConnected;
            MyVisualScriptLogicProvider.PlayerSpawned -= OnPlayerSpawned;
            GpsCommandFallbacks.Unsubscribe();
            _multibase = null;
        }

        public void SetupConfig()
        {
            
            var configFile = Path.Combine(StoragePath, CONFIG_FILE_NAME);
            try
            {
                _config = Persistent<GPS_OrganizerConfig>.Load(configFile);
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
            LoggerHelper.DebugLog(Log, _config.Data, "SetupConfig() - Start");

            if (_config?.Data == null)
            {
                LoggerHelper.DebugLog(Log, _config.Data, "SetupConfig() - Creating default config");
                _config = new Persistent<GPS_OrganizerConfig>(configFile, new GPS_OrganizerConfig());
                _config.Save();
            }

            var markersConfigFile = Path.Combine(StoragePath, CONFIG_FILE_MARKERS);
            try
            {
                _markersConfig = Persistent<GPS_OrganizerMarkersConfig>.Load(markersConfigFile);
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
            if (_markersConfig?.Data == null)
            {
                LoggerHelper.DebugLog(Log, _config.Data, "SetupConfig() - Creating default markers config");
                _markersConfig = new Persistent<GPS_OrganizerMarkersConfig>(markersConfigFile, new GPS_OrganizerMarkersConfig());
                _markersConfig.Save();
            }

            _gpsHandler = new GpsHandler(_config.Data, _markersConfig.Data);
            LoggerHelper.DebugLog(Log, _config.Data, "SetupConfig() - End");
        }

        public void Save()
        {
            LoggerHelper.DebugLog(Log, _config.Data, "Save() - Start");
            try
            {
                _config.Save();
                _markersConfig.Save();
                LoggerHelper.DebugLog(Log, _config.Data, "Save() - Configuration and markers saved");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Save() - Configuration and markers failed to save");
            }
            LoggerHelper.DebugLog(Log, _config.Data, "Save() - End");
        }

        private void StartPendingJoinTimer()
        {
            StopPendingJoinTimer();
            _pendingJoinTimer = new Timer(OnPendingJoinTimer, null, PendingRetryIntervalMs, PendingRetryIntervalMs);
        }

        private void StopPendingJoinTimer()
        {
            var timer = _pendingJoinTimer;
            _pendingJoinTimer = null;
            timer?.Dispose();
        }

        private void OnPendingJoinTimer(object state)
        {
            if (_sessionUnloading || _pendingMarkerSends.IsEmpty)
                return;

            var torch = Torch;
            if (torch == null)
                return;

            torch.Invoke(ProcessPendingMarkerSends);
        }

        private void ProcessPendingMarkerSends()
        {
            if (_sessionUnloading)
                return;

            foreach (var steamId in _pendingMarkerSends.Keys)
                TryDeliverPending(steamId, "retry");
        }

        private void OnPlayerConnected(long identityId)
        {
            TrySendFromIdentityEvent(identityId, "PlayerConnected");
        }

        private void OnPlayerSpawned(long identityId)
        {
            TrySendFromIdentityEvent(identityId, "PlayerSpawned");
        }

        private void TrySendFromIdentityEvent(long identityId, string trigger)
        {
            if (_sessionUnloading || identityId == 0)
                return;

            if (Config == null || !Config.SendMarkerOnJoin)
                return;

            var torch = Torch;
            if (torch == null)
                return;

            torch.Invoke(() =>
            {
                if (_sessionUnloading)
                    return;

                var players = MySession.Static?.Players;
                if (players == null)
                    return;

                var steamId = players.TryGetSteamId(identityId);
                if (steamId == 0)
                    return;

                TryDeliverPending(steamId, trigger);
            });
        }

        private void multibase_PlayerJoined(IPlayer obj)
        {
            if (obj == null)
                return;

            byte stale;
            _sentThisConnection.TryRemove(obj.SteamId, out stale);

            LoggerHelper.DebugLog(Log, _config.Data, $"multibase_PlayerJoined() - Player joined: {obj.SteamId}");

            if (Config == null || !Config.SendMarkerOnJoin)
                return;

            _pendingMarkerSends[obj.SteamId] = new PendingMarkerSend
            {
                Name = string.IsNullOrEmpty(obj.Name) ? obj.SteamId.ToString() : obj.Name,
                QueuedAtUtc = DateTime.UtcNow,
                QueuedLogged = false
            };

            Torch.Invoke(() => TryDeliverPending(obj.SteamId, "PlayerJoined"));
        }

        private void multibase_PlayerLeft(IPlayer obj)
        {
            if (obj == null)
                return;

            byte sent;
            _sentThisConnection.TryRemove(obj.SteamId, out sent);

            PendingMarkerSend pending;
            if (_pendingMarkerSends.TryRemove(obj.SteamId, out pending))
                Log.Info($"GPS Organizer: cancelled pending marker send for {pending.Name} ({obj.SteamId}) because the player left.");
        }

        private void TryDeliverPending(ulong steamId, string trigger)
        {
            try
            {
                if (_sessionUnloading)
                    return;

                if (Config == null || !Config.SendMarkerOnJoin)
                {
                    PendingMarkerSend discarded;
                    _pendingMarkerSends.TryRemove(steamId, out discarded);
                    return;
                }

                if (_sentThisConnection.ContainsKey(steamId))
                {
                    PendingMarkerSend alreadySent;
                    _pendingMarkerSends.TryRemove(steamId, out alreadySent);
                    return;
                }

                PendingMarkerSend pending;
                _pendingMarkerSends.TryGetValue(steamId, out pending);

                string reason;
                if (TrySendMarkers(steamId, out reason))
                {
                    _sentThisConnection[steamId] = 0;
                    _pendingMarkerSends.TryRemove(steamId, out pending);
                    var name = pending != null && !string.IsNullOrEmpty(pending.Name) ? pending.Name : ResolvePlayerName(steamId);
                    Log.Info($"GPS Organizer: sent markers to {name} ({steamId}) via {trigger}.");
                    return;
                }

                if (pending == null)
                    return;

                var elapsed = DateTime.UtcNow - pending.QueuedAtUtc;
                if (elapsed.TotalMinutes >= PendingRetryTimeoutMinutes)
                {
                    _pendingMarkerSends.TryRemove(steamId, out pending);
                    Log.Warn($"GPS Organizer: timed out after {PendingRetryTimeoutMinutes} minutes waiting to send markers to {pending.Name} ({steamId}): {reason}.");
                    return;
                }

                if (!pending.QueuedLogged)
                {
                    pending.QueuedLogged = true;
                    Log.Info($"GPS Organizer: queued marker send for {pending.Name} ({steamId}): {reason}. Retrying until the player is online or {PendingRetryTimeoutMinutes} minutes elapse.");
                }
                else
                    LoggerHelper.DebugLog(Log, _config.Data, $"TryDeliverPending() - still waiting for {pending.Name} ({steamId}): {reason}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"GPS Organizer: failed to deliver markers to {steamId} via {trigger}.");
            }
        }

        private bool TrySendMarkers(ulong steamId, out string reason)
        {
            var players = MySession.Static?.Players;
            if (players == null)
            {
                reason = "session players are not available";
                return false;
            }

            var identityId = players.TryGetIdentityId(steamId);
            if (identityId == 0)
            {
                reason = "identity not yet created";
                return false;
            }

            var identity = players.TryGetIdentity(identityId);
            if (identity == null)
            {
                reason = "identity not yet created";
                return false;
            }

            if (!players.IsPlayerOnline(identityId))
            {
                reason = "player is not online";
                return false;
            }

            if (_gpsHandler == null)
            {
                reason = "GPS handler is not loaded";
                return false;
            }

            _gpsHandler.SendGPSMarkers(identityId);
            reason = null;
            return true;
        }

        private string ResolvePlayerName(ulong steamId)
        {
            PendingMarkerSend pending;
            if (_pendingMarkerSends.TryGetValue(steamId, out pending) && !string.IsNullOrEmpty(pending.Name))
                return pending.Name;

            var players = MySession.Static?.Players;
            var identityId = players != null ? players.TryGetIdentityId(steamId) : 0;
            var identity = identityId != 0 ? players.TryGetIdentity(identityId) : null;
            if (identity != null && !string.IsNullOrEmpty(identity.DisplayName))
                return identity.DisplayName;

            return steamId.ToString();
        }

        /// <summary>
        /// Sends the configured markers to an identity immediately. Used by admin commands.
        /// </summary>
        public bool TrySendMarkersToIdentity(long identityId, out string reason)
        {
            if (identityId == 0)
            {
                reason = "identity was not found";
                return false;
            }

            if (_gpsHandler == null)
            {
                reason = "GPS handler is not loaded";
                return false;
            }

            _gpsHandler.SendGPSMarkers(identityId);

            var players = MySession.Static?.Players;
            var steamId = players?.TryGetSteamId(identityId) ?? 0;
            if (steamId != 0 && players.IsPlayerOnline(identityId))
            {
                _sentThisConnection[steamId] = 0;
                PendingMarkerSend pending;
                _pendingMarkerSends.TryRemove(steamId, out pending);
            }

            reason = null;
            return true;
        }

        public void AddGPSMarker(string name, string description, Vector3D coords, bool showOnHud, bool alwaysVisible, bool isObjective, long entityId, long contractId, Color color, TimeSpan? discardAt)
        {

            var gpsMarker = new MyGpsEntry
            {
                Name = name,
                Description = description,
                Coords = coords,
                DiscardAt = discardAt,
                ShowOnHud = showOnHud,
                AlwaysVisible = alwaysVisible,
                Color = color,
                EntityId = entityId,
                IsObjective = isObjective,
                ContractId = contractId,
                DisplayName = name
            };

            // Use Dispatcher.Invoke to modify ObservableCollection on the UI thread
            _control.Dispatcher.Invoke(() =>
            {
                lock (_markersConfig.Data.EntriesLock)
                    _markersConfig.Data.Entries.Add(gpsMarker);
                _control.RefreshGpsList();
            });

            Save();
            LoggerHelper.DebugLog(Log, _config.Data, $"AddGPSMarker() - Marker {name} added");
        }

    }
}
