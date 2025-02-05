using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace GiantArmy.AdaptivePerformance.iOS
{
    /// <summary>
    /// Provider Settings for iOS provider, which controls the runtime asset instance that stores the settings.
    /// </summary>
    [System.Serializable]
    [AdaptivePerformanceConfigurationData("iOS", iOSProviderConstants.k_SettingsKey)]
    public class iOSProviderSettings : IAdaptivePerformanceSettings
    {
        [SerializeField, Tooltip("Enable Logging in Devmode")]
        bool m_iOSProviderLogging = false;

        /// <summary>
        ///  Control debug logging of the iOS provider.
        ///  This setting only affects development builds. All logging is disabled in release builds.
        ///  You can also control the global logging setting after startup by using <see cref="IDevelopmentSettings.Logging"/>.
        ///  Logging is disabled by default.
        /// </summary>
        /// <value>Set this to true to enable debug logging, or false to disable it (default: false).</value>
        public bool iOSProviderLogging
        {
            get { return m_iOSProviderLogging; }
            set { m_iOSProviderLogging = value; }
        }

        /// <summary>Static instance that holds the runtime asset instance Unity creates during the build process.</summary>
#if !UNITY_EDITOR
        public static iOSProviderSettings s_RuntimeInstance = null;
#endif
        void Awake()
        {
#if !UNITY_EDITOR
            s_RuntimeInstance = this;
#endif
        }

        /// <summary>
        /// Returns iOS Provider Settings which are used by Adaptive Performance to apply Provider Settings.
        /// </summary>
        /// <returns>iOS Provider Settings</returns>
        public static iOSProviderSettings GetSettings()
        {
            iOSProviderSettings settings = null;
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject<iOSProviderSettings>(iOSProviderConstants.k_SettingsKey, out settings);
#else
            settings = s_RuntimeInstance;
#endif
            return settings;
        }
    }
}
