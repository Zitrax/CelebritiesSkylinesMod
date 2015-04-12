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
                    SkylinesOverwatch.Settings.Instance.Enable.BuildingMonitor = true;
                    SkylinesOverwatch.Settings.Instance.Enable.HumanMonitor = true;
                    SkylinesOverwatch.Settings.Instance.Enable.Residents = true;
                    // TODO: Is this needed ?
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

        ushort? m_tb = null;

        private void ProcessBuildingsUpdated()
        {
            if (m_tb != null) return;
            ushort[] entries = SkylinesOverwatch.Data.Instance.BuildingsUpdated;
            if (entries.Length == 0) return;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            foreach (ushort i in entries)
            {
                Building building = instance.m_buildings.m_buffer[(short)i];
                m_tb = i;
                ChirpLog.Info("Target building is: " + m_tb);
                ChirpLog.Flush();
                return;
            }
        }

        private void ProcessHumansUpdated()
        {
            //Nothing to do if we have no target
            //if (m_tb == null) return;

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

                if (info.m_gender == Citizen.Gender.Female)
                {
                    info.m_gender = Citizen.Gender.Male;
                    ChirpLog.Info("Updated resident: " + resident.ToString() + " " + i);
                    ChirpLog.Flush();
                }

                // Do something with resident
                CitizenAI ai = (CitizenAI) info.GetAI();
                // TODO: How to get the CitizenInstance ?
                //ai.SetTarget(resident.m_instance, 0, m_tb);
            }
        }
    }
}
