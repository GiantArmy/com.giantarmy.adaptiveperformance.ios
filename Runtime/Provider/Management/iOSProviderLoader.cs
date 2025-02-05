using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AdaptivePerformance;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AdaptivePerformance.Editor;
#endif
using System.Runtime.InteropServices;
using UnityEngine.AdaptivePerformance.Provider;

namespace GiantArmy.AdaptivePerformance.iOS
{
    /// <summary>
    /// iOSProviderLoader implements the loader for Adaptive Performance on devices running iOS.
    /// </summary>
#if UNITY_EDITOR
    [AdaptivePerformanceSupportedBuildTargetAttribute(BuildTargetGroup.iOS)]
#endif
    public class iOSProviderLoader : AdaptivePerformanceLoaderHelper
    {
        static List<AdaptivePerformanceSubsystemDescriptor> s_iOSSubsystemDescriptors =
            new List<AdaptivePerformanceSubsystemDescriptor>();

        /// <summary>
        /// Returns if the provider loader was initialized successfully.
        /// </summary>
        public override bool Initialized
        {
            get
            {
#if UNITY_IOS
                return iOSSubsystem != null;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Returns if the provider loader is currently running.
        /// </summary>
        public override bool Running
        {
            get
            {
#if UNITY_IOS
                return iOSSubsystem != null && iOSSubsystem.running;
#else
                return false;
#endif
            }
        }

#if UNITY_IOS
        /// <summary>Returns the currently active iOS Subsystem instance, if an instance exists.</summary>
        public iOSAdaptivePerformanceSubsystem iOSSubsystem
        {
            get { return GetLoadedSubsystem<iOSAdaptivePerformanceSubsystem>(); }
        }
#endif
        /// <summary>
        /// Implementation of <see cref="AdaptivePerformanceLoader.GetDefaultSubsystem"/>.
        /// </summary>
        /// <returns>Returns the iOS Subsystem, which is the loaded default subsystem. Because only one subsystem can be present at a time, Adaptive Performance always initializes the first subsystem and uses it as a default. You can change subsystem order in the Adaptive Performance Provider Settings.</returns>
        public override ISubsystem GetDefaultSubsystem()
        {
#if UNITY_IOS
            return iOSSubsystem;
#else
            return null;
#endif
        }

        /// <summary>
        /// Implementation of <see cref="AdaptivePerformanceLoader.GetSettings"/>.
        /// </summary>
        /// <returns>Returns the iOS settings.</returns>
        public override IAdaptivePerformanceSettings GetSettings()
        {
            return iOSProviderSettings.GetSettings();
        }

        /// <summary>Implementation of <see cref="AdaptivePerformanceLoader.Initialize"/>.</summary>
        /// <returns>True if successfully initialized the iOS subsystem, false otherwise.</returns>
        public override bool Initialize()
        {
#if UNITY_IOS
            CreateSubsystem<AdaptivePerformanceSubsystemDescriptor, iOSAdaptivePerformanceSubsystem>(s_iOSSubsystemDescriptors, "iOS");
            if (iOSSubsystem == null)
            {
                Debug.LogError("Unable to start the iOS subsystem.");
            }

            return iOSSubsystem != null;
#else
            return false;
#endif
        }

        /// <summary>Implementation of <see cref="AdaptivePerformanceLoader.Start"/>.</summary>
        /// <returns>True if successfully started the iOS subsystem, false otherwise.</returns>
        public override bool Start()
        {
#if UNITY_IOS
            StartSubsystem<iOSAdaptivePerformanceSubsystem>();
            return true;
#else
            return false;
#endif
        }

        /// <summary>Implementaion of <see cref="AdaptivePerformanceLoader.Stop"/>.</summary>
        /// <returns>True if successfully stopped the iOS subsystem, false otherwise.</returns>
        public override bool Stop()
        {
#if UNITY_IOS
            StopSubsystem<iOSAdaptivePerformanceSubsystem>();
            return true;
#else
            return false;
#endif
        }

        /// <summary>Implementaion of <see cref="AdaptivePerformanceLoader.Deinitialize"/>.</summary>
        /// <returns>True if successfully deinitialized the iOS subsystem, false otherwise.</returns>
        public override bool Deinitialize()
        {
#if UNITY_IOS
            DestroySubsystem<iOSAdaptivePerformanceSubsystem>();
            return base.Deinitialize();
#else
            return false;
#endif
        }
    }
}
