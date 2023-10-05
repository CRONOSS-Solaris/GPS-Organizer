using NLog;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using System;
using System.IO;
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
        private IMultiplayerManagerBase _multibase;
        private GPS_OrganizerControl _control;

        public UserControl GetControl() => _control ?? (_control = new GPS_OrganizerControl(this));

        private Persistent<GPS_OrganizerConfig> _config;
        public GPS_OrganizerConfig Config => _config?.Data;

        private Persistent<GPS_OrganizerMarkersConfig> _markersConfig;
        public GPS_OrganizerMarkersConfig Markers => _markersConfig?.Data;
        public GpsHandler _gpsHandler;

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
                    _multibase = Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>();
                    if (_multibase != null)
                        _multibase.PlayerJoined += multibase_PlayerJoined;
                    else
                        LoggerHelper.DebugLog(Log, _config.Data, "SessionChanged() - No multiplayer manager loaded!");
                    break;
                case TorchSessionState.Unloading:
                    if (_multibase != null)
                        _multibase.PlayerJoined -= multibase_PlayerJoined;
                    break;
            }
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

        private void multibase_PlayerJoined(IPlayer obj)
        {
            LoggerHelper.DebugLog(Log, _config.Data, $"multibase_PlayerJoined() - Player joined: {obj.SteamId}");
            var identity = MySession.Static.Players.TryGetIdentityId(obj.SteamId);
            if (identity == 0)
            {
                LoggerHelper.DebugLog(Log, _config.Data, "multibase_PlayerJoined() - Identity not found");
                return;
            }

            if (Config.SendMarkerOnJoin)
                _gpsHandler.SendGPSMarkers(identity);
        }

        public void AddGPSMarker(string name, string description, Vector3D coords, bool ShowOnHud, bool AlwaysVisible, bool IsObjective, long EntityId, long ContractId, Color color)
        {
            LoggerHelper.DebugLog(Log, _config.Data, $"AddGPSMarker() - Adding GPS marker: {name}");
            var gpsMarker = new MyGpsEntry
            {
                Name = name,
                Description = description,
                Coords = coords,
                DiscardAt = null,
                ShowOnHud = ShowOnHud,
                AlwaysVisible = AlwaysVisible,
                Color = color,
                EntityId = EntityId,
                IsObjective = IsObjective,
                ContractId = ContractId,
                DisplayName = name
            };

            // Use Dispatcher.Invoke to modify ObservableCollection on the UI thread
            _control.Dispatcher.Invoke(() =>
            {
                _markersConfig.Data.Entries.Add(gpsMarker);
                _control.RefreshGpsList();
            });

            Save();
            LoggerHelper.DebugLog(Log, _config.Data, $"AddGPSMarker() - Marker {name} added");
        }

    }
}