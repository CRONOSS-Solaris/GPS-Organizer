using GPS_Organizer.GPS_Organizer;
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

namespace GPS_Organizer
{
    public class GpsOrganizerPlugin : TorchPluginBase, IWpfPlugin
    {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string CONFIG_FILE_NAME = "GPS_OrganizerConfig.cfg";
        private static readonly string CONFIG_FILE_MARKERS = "GPS_OrganizerMarkers.cfg";

        private GPS_OrganizerControl _control;
        public UserControl GetControl() => _control ?? (_control = new GPS_OrganizerControl(this));

        private Persistent<GPS_OrganizerConfig> _config;
        public GPS_OrganizerConfig Config => _config?.Data;

        private Persistent<GPS_OrganizerMarkersConfig> _markersConfig;
        public GPS_OrganizerMarkersConfig Markers => _markersConfig?.Data;
        private GpsHandler _gpsHandler;

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
                    Log.Info("Session Loaded!");
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    break;
            }
        }

        private void SetupConfig()
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
                Log.Info("Configuration Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Configuration failed to save");
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

        public void AddGPSMarker(string name, string description, Vector3D coords)
        {
            var gpsMarker = new MyGpsEntry
            {
                Name = name,
                Description = description,
                Coords = coords,
                IsFinal = false,
                ShowOnHud = true,
                AlwaysVisible = false,
                Color = new Color(27, 220, 220, 255),
                EntityId = 0,
                IsObjective = false,
                ContractId = 0,
                DisplayName = name
            };

            _markersConfig.Data.Entries.Add(gpsMarker);
            _control.RefreshGpsList();
            Save();
        }

    }
}
