using System.Collections.Generic;
using GiantArmy.AdaptivePerformance.iOS;
using UnityEditor;
using UnityEditor.AdaptivePerformance.Editor.Metadata;
using UnityEngine;

namespace GiantArmy.AdaptivePerformance.iOS.Editor
{
    internal class iOSProviderMetadata : IAdaptivePerformancePackage
    {
        private class iOSPackageMetadata : IAdaptivePerformancePackageMetadata
        {
            public string packageName => "Adaptive Performance iOS";
            public string packageId => "com.giantarmy.adaptiveperformance.ios";
            public string settingsType => "GiantArmy.AdaptivePerformance.iOS.iOSProviderSettings";
            public string licenseURL => "https://universesandbox.com";
            public List<IAdaptivePerformanceLoaderMetadata> loaderMetadata => s_LoaderMetadata;

            private readonly static List<IAdaptivePerformanceLoaderMetadata> s_LoaderMetadata = new List<IAdaptivePerformanceLoaderMetadata>() { new iOSLoaderMetadata() };
        }

        private class iOSLoaderMetadata : IAdaptivePerformanceLoaderMetadata
        {
            public string loaderName => "iOS Provider";
            public string loaderType => "GiantArmy.AdaptivePerformance.iOS.iOSProviderLoader";
            public List<BuildTargetGroup> supportedBuildTargets => s_SupportedBuildTargets;

            private readonly static List<BuildTargetGroup> s_SupportedBuildTargets = new List<BuildTargetGroup>()
            {
                BuildTargetGroup.iOS
            };
        }

        private static IAdaptivePerformancePackageMetadata s_Metadata = new iOSPackageMetadata();
        public IAdaptivePerformancePackageMetadata metadata => s_Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            var settings = obj as iOSProviderSettings;
            if (settings != null)
            {
                settings.logging = false;
                settings.statsLoggingFrequencyInFrames = 50;
                settings.automaticPerformanceMode = true;

                return true;
            }

            return false;
        }
    }
}
