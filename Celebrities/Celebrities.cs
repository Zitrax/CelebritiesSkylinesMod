using ICities;
using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using ChirpLogger;

namespace Celebrities
{
    public class Celebrities: ThreadingExtensionBase
    {
        private bool _initialized;
        private bool _terminated;

        protected bool IsOverwatched()
        {
            // Note that logger messages ends up in C:\Program Files (x86)\Steam\SteamApps\common\Cities_Skylines\_DebugChirpLogs\Skeleton_v1.0
            ChirpLogger.ChirpLog.Info("IsOverwatched");
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin.publishedFileID.AsUInt64 == 421028969)
                    return true;
            }

            return false;
        }

        public override void OnCreated(IThreading threading)
        {
            ChirpLog.Info("OnCreated");
            ChirpLog.Flush();

            _initialized = false;
            _terminated = false;

            base.OnCreated(threading);
        }

        public override void OnReleased()
        {
            ChirpLog.Info("OnReleased");
            ChirpLog.Flush();
            _initialized = false;
            _terminated = false;

            base.OnReleased();
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (_terminated) return;

            try
            {
                if (!_initialized)
                {
                    if (!IsOverwatched())
                    {
                        ChirpLogger.ChirpLog.Error("Skylines Overwatch not found. Terminating...");
                        ChirpLog.Flush();
                        _terminated = true;

                        return;
                    }

                    SkylinesOverwatch.Settings.Instance.Enable.HumanMonitor = true;
                    SkylinesOverwatch.Settings.Instance.Enable.Residents = true;
                    SkylinesOverwatch.Settings.Instance.Debug.HumanMonitor = true;

                    _initialized = true;

                }
                else
                {
                    ProcessHumansUpdated();
                }
            }
            catch (Exception e)
            {
                string error = "Failed to initialize\r\n";
                error += String.Format("Error: {0}\r\n", e.Message);
                error += "\r\n";
                error += "==== STACK TRACE ====\r\n";
                error += e.StackTrace;

                ChirpLog.Error(error);
                ChirpLog.Flush();

                _terminated = true;
            }

            base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }

        private void ProcessHumansUpdated()
        {
            uint[] entries = SkylinesOverwatch.Data.Instance.HumansUpdated;

            if (entries.Length == 0) return;

            CitizenManager instance = Singleton<CitizenManager>.instance;

            foreach (uint i in entries)
            {
                Citizen resident = instance.m_citizens.m_buffer[(int)i];

                if (resident.Dead)
                    continue;

                if ((resident.m_flags & Citizen.Flags.Created) == Citizen.Flags.None)
                    continue;

                CitizenInfo info = resident.GetCitizenInfo(i);

                if (info == null)
                    continue;

                if (!(info.m_citizenAI is ResidentAI))
                    continue;

                // Do something with resident
                ChirpLog.Info("Updated resident: " + resident.ToString());
            }
        }
    }
}
