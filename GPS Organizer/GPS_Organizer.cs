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

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            Save();
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            switch (state)
            {

                case TorchSessionState.Loaded:
                    _multibase = Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>();
                    if (_multibase != null)
                        _multibase.PlayerJoined += multibase_PlayerJoined;
                    else
                        Log.Warn("No multiplayer manager loaded!");
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

            if (_config?.Data == null)
            {
                Log.Info("Create Default Config, because none was found!");

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
                Log.Info("Create Default Markers Config, because none was found!");

                _markersConfig = new Persistent<GPS_OrganizerMarkersConfig>(markersConfigFile, new GPS_OrganizerMarkersConfig());
                _markersConfig.Save();
            }

            _gpsHandler = new GpsHandler(_markersConfig.Data);
        }


        public void Save()
        {
            try
            {
                _config.Save();
                _markersConfig.Save();
                Log.Info("Configuration and Markers Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Configuration and Markers failed to save");
            }
        }


        private void multibase_PlayerJoined(IPlayer obj)
        {
            Log.Info(obj.State.ToString());
            var idendity = MySession.Static.Players.TryGetIdentityId(obj.SteamId);
            if (idendity == 0)
            {
                Log.Info("Identity not found");
                return;
            }

            if (Config.SendMarkerOnJoin)
                _gpsHandler.SendGPSMarkers(idendity);
        }

        public void AddGPSMarker(string name, string description, Vector3D coords, bool ShowOnHud, bool AlwaysVisible, bool IsObjective, long EntityId, long ContractId, Color color)
        {
            var gpsMarker = new MyGpsEntry
            {
                Name = name,
                Description = description,
                Coords = coords,
                IsFinal = false,
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
        }


    }
}
