#if UNITY_IOS

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using UnityEngine.AdaptivePerformance.Provider;
using AOT;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.iOS;


namespace GiantArmy.AdaptivePerformance.iOS
{
    internal static class ADPFLog
    {
        static iOSProviderSettings settings = iOSProviderSettings.GetSettings();

        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string format, params object[] args)
        {
            if (settings != null && settings.iOSProviderLogging)
                UnityEngine.Debug.Log(System.String.Format("[AP iOS] " + format, args));
        }
    }

    [Preserve]
    public class iOSAdaptivePerformanceSubsystem : AdaptivePerformanceSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static AdaptivePerformanceSubsystemDescriptor RegisterDescriptor()
        {
            if (!iOSAdaptivePerformanceSubsystemProvider.NativeApi.IsAvailable())
            {
                ADPFLog.Debug($"The native API for this provider is not available. Aborting registering the Adaptive Performance provider descriptor.");
                return null;
            }

            var registeredDesc = AdaptivePerformanceSubsystemDescriptor.RegisterDescriptor(new AdaptivePerformanceSubsystemDescriptor.Cinfo
            {
                id = "iOS",
                providerType = typeof(iOSAdaptivePerformanceSubsystem.iOSAdaptivePerformanceSubsystemProvider),
                subsystemTypeOverride = typeof(iOSAdaptivePerformanceSubsystem)
            });
            return registeredDesc;
        }

        public class iOSAdaptivePerformanceSubsystemProvider : APProvider, IApplicationLifecycle, IDevicePerformanceLevelControl
        {
           
            PerformanceDataRecord m_Data = new PerformanceDataRecord();
            object m_DataLock = new object();

            float m_Temperature;
            float m_TemperatureUpdateTimestamp;

            Version m_Version = null;

            public override IApplicationLifecycle ApplicationLifecycle { get { return this; } }
            public override IDevicePerformanceLevelControl PerformanceLevelControl { get { return this; } }

            public int MaxCpuPerformanceLevel { get; set; }
            public int MaxGpuPerformanceLevel { get; set; }

            static iOSProviderSettings s_Settings = iOSProviderSettings.GetSettings();

            public iOSAdaptivePerformanceSubsystemProvider()
            {
                MaxCpuPerformanceLevel = 3;
                MaxGpuPerformanceLevel = 3;
            }

            void OnPerformanceWarning()
            {
                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.WarningLevel;
                    //m_Data.WarningLevel = warningLevel;
                }
            }

            void ImmediateUpdateTemperature()
            {
                if (!Capabilities.HasFlag(Feature.TemperatureLevel))
                    return;

                UpdateTemperatureLevel();
                m_TemperatureUpdateTimestamp = Time.time;

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.TemperatureLevel;
                    m_Data.TemperatureLevel = m_Temperature;
                }
            }

            void TimedUpdateTemperature()
            {
                if (!Capabilities.HasFlag(Feature.TemperatureLevel))
                    return;

                var timestamp = Time.time;
                var canUpdateTemperature = (timestamp - m_TemperatureUpdateTimestamp) > 1.0f;

                if (!canUpdateTemperature)
                    return;

                var previousTemperature = m_Temperature;
                UpdateTemperatureLevel();
                var isTemperatureChanged = previousTemperature != m_Temperature;
                m_TemperatureUpdateTimestamp = timestamp;

                if (!isTemperatureChanged)
                    return;

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.TemperatureLevel;
                    m_Data.TemperatureLevel = m_Temperature;
                }
            }

            void ImmediateUpdateThermalStatus()
            {
                if (!Capabilities.HasFlag(Feature.WarningLevel))
                    return;

                /*
                var warningLevel = m_Api.GetThermalStatusWarningLevel();

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.WarningLevel;
                    m_Data.WarningLevel = warningLevel;
                }
                */
            }

            void ImmediateUpdatePerformanceMode()
            {
                if (!Capabilities.HasFlag(Feature.PerformanceMode))
                    return;

                
                /*
                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.PerformanceMode;
                    m_Data.PerformanceMode = m_PerformanceMode;
                }
                */
            }

            static bool TryParseVersion(string versionString, out Version version)
            {
                try
                {
                    version = new Version(versionString);
                }
                catch (Exception)
                {
                    version = null;
                    return false;
                }
                return true;
            }

            protected override bool TryInitialize()
            {
                if (Initialized)
                {
                    return true;
                }

                if (!base.TryInitialize())
                {
                    return false;
                }

                Capabilities = Feature.None;

                /*
                if (!m_Api.Initialize())
                {
                    return false;
                }

                if (TryParseVersion(m_Api.GetVersion(), out m_Version))
                {
                    if (m_Version >= new Version(11, 0))
                    {
                        Initialized = true;
                        MaxCpuPerformanceLevel = m_Api.GetMaxCpuPerformanceLevel();
                        MaxGpuPerformanceLevel = m_Api.GetMaxGpuPerformanceLevel();
                        Capabilities = Feature.CpuPerformanceLevel | Feature.GpuPerformanceLevel | Feature.WarningLevel;
                    }
                    if (m_Version >= new Version(12, 0))
                    {
                        Capabilities |= Feature.PerformanceMode;
                        Capabilities |= Feature.TemperatureTrend;
                        Capabilities |= Feature.TemperatureLevel;
                        Capabilities |= Feature.PerformanceLevelControl;
                    }

                    if (m_Version < new Version(11, 0))
                    {
                        m_Api.Terminate();
                        Initialized = false;
                    }
                }
                */

                m_Data.PerformanceLevelControlAvailable = true;

                return Initialized;
            }

            public override void Start()
            {
                if (!Initialized)
                {
                    return;
                }

                if (m_Running)
                {
                    return;
                }

                ImmediateUpdateTemperature();
                ImmediateUpdateThermalStatus();
                ImmediateUpdatePerformanceMode();

                m_Running = true;

            }

            public override void Stop()
            {
                m_Running = false;
            }

            public override void Destroy()
            {
                if (m_Running)
                {
                    Stop();
                }

                if (Initialized)
                {
                    Initialized = false;
                }
            }

            public override string Stats => $"Stats=";

            public override PerformanceDataRecord Update()
            {
                /*
                if(Capabilities.HasFlag(Feature.PerformanceLevelControl))
                    m_Api.UpdateHintSystem();
                */

                ImmediateUpdatePerformanceMode();

                TimedUpdateTemperature();

                lock (m_DataLock)
                {
                    PerformanceDataRecord result = m_Data;
                    m_Data.ChangeFlags = Feature.None;

                    return result;
                }
            }

            public override Version Version
            {
                get
                {
                    return m_Version;
                }
            }

            public override Feature Capabilities { get; set; }

            public override bool Initialized { get; set; }

            public bool SetPerformanceLevel(ref int cpuLevel, ref int gpuLevel)
            {
                return false;
            }

            public bool EnableCpuBoost()
            {
                return false;
            }

            public bool EnableGpuBoost()
            {
                return false;
            }

            public void ApplicationPause() { }

            public void ApplicationResume()
            {
                ImmediateUpdateTemperature();
                ImmediateUpdatePerformanceMode();
            }

            void UpdateTemperatureLevel()
            {
                if (!Capabilities.HasFlag(Feature.TemperatureLevel))
                    return;
                //m_Temperature = (float)Math.Round(20, 2, MidpointRounding.AwayFromZero);
            }

            internal class NativeApi
            {
                static bool s_IsAvailable = false;

                public NativeApi()
                {
                    StaticInit();
                }

                

                static void StaticInit()
                {
                    if (s_IsAvailable)
                        return;

                    s_IsAvailable = true;
                }

                public static bool IsAvailable()
                {
                    StaticInit();
                    return s_IsAvailable;
                }


                public bool Initialize()
                {
                    bool isInitialized = false;
                    try
                    {
                        //Init
                    }
                    catch (Exception ex)
                    {
                        ADPFLog.Debug($"[Exception] Initialize() failed due to {ex}!");
                    }

                    return isInitialized;
                }

                public void Terminate()
                {
                    try
                    {
                        //Terminate
                    }
                    catch (Exception ex)
                    {
                        ADPFLog.Debug($"[Exception] Terminate() failed due to {ex}!");
                    }
                }

                public string GetVersion()
                {
                    string sdkVersion = "1";
                    try
                    {
                        //Get OS Version
                    }
                    catch (Exception ex)
                    {
                        ADPFLog.Debug($"[Exception] getVersion() failed! {ex}");
                    }
                    return sdkVersion;
                }

                public void UpdateHintSystem()
                {
                    UnityEngine.FrameTiming[] frameTimings = new UnityEngine.FrameTiming[1];
                    FrameTimingManager.CaptureFrameTimings();
                    uint res = FrameTimingManager.GetLatestTimings(1, frameTimings);
                    if (res > 0 && frameTimings != null)
                    {
                        var frameTimeInInt = (int)((frameTimings[0].cpuMainThreadFrameTime + frameTimings[0].cpuRenderThreadFrameTime) * 1000000.0);
                        var desiredDuration = 16666666;
                        if (OnDemandRendering.effectiveRenderFrameRate > 0)
                            desiredDuration = (int)((1.0 / (double)OnDemandRendering.effectiveRenderFrameRate) * 1000000000.0);

                        //ReportCompletionTime(frameTimeInInt);
                        //UpdateTargetWorkDuration(desiredDuration);
                    }
                    else
                    {
                        ADPFLog.Debug($"FrameTimingManager does not have results, skip reporting.");
                    }
                }

                public bool EnableCpuBoost()
                {
                    return false;
                }

                public bool EnableGpuBoost()
                {
                    return false;
                }

                public int GetClusterInfo()
                {
                    int result = -999;
                    return result;
                }

                public int GetMaxCpuPerformanceLevel()
                {
                    int maxCpuPerformanceLevel = -1;
                    return maxCpuPerformanceLevel;
                }

                public int GetMaxGpuPerformanceLevel()
                {
                    int maxGpuPerformanceLevel = -1;
                    return maxGpuPerformanceLevel;
                }
            }
        }
    }
}
#endif
