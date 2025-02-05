using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEditor.AdaptivePerformance.Editor;
using GiantArmy.AdaptivePerformance.iOS;

namespace GiantArmy.AdaptivePerformance.iOS.Editor
{ 
    public class iOSProviderBuildProcess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// Override of <see cref="IPreprocessBuildWithReport"/> and <see cref="IPostprocessBuildWithReport"/>.
        /// </summary>
        public int callbackOrder
        {
            get { return 0; }
        }

        /// <summary>
        /// Clears out settings which could be left over from previous unsuccessfull runs.
        /// </summary>
        void CleanOldSettings()
        {
            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets == null)
                return;

            var oldSettings = from s in preloadedAssets
                              where s != null && s.GetType() == typeof(iOSProviderSettings)
                              select s;

            if (oldSettings != null && oldSettings.Any())
            {
                var assets = preloadedAssets.ToList();
                foreach (var s in oldSettings)
                {
                    assets.Remove(s);
                }

                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        List<PluginImporter> m_DisabledPlugins = new List<PluginImporter>();

        void ExcludeNativeLibsWhenProviderDisabled()
        {
            var generalSettings = AdaptivePerformanceGeneralSettingsPerBuildTarget.AdaptivePerformanceGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
            foreach (var loader in generalSettings.AssignedSettings.loaders)
            {
                if (loader is iOSProviderLoader)
                {
                    return;
                }
            }

            foreach (var p in PluginImporter.GetImporters(BuildTarget.iOS))
            {
                if (p.ShouldIncludeInBuild() && p.assetPath.Contains("AdaptivePerformanceiOS"))
                {
                    p.SetIncludeInBuildDelegate(path => false);
                    m_DisabledPlugins.Add(p);
                }
            }
        }

        void RestoreNativeLibs()
        {
            foreach (var p in m_DisabledPlugins)
            {
                p.SetIncludeInBuildDelegate(path => true);
            }
            m_DisabledPlugins.Clear();
        }

        /// <summary>
        /// Override of <see cref="IPreprocessBuildWithReport"/>.
        /// </summary>
        /// <param name="report">Build report.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            ExcludeNativeLibsWhenProviderDisabled();

            // Always remember to clean up preloaded assets after build to make sure we don't
            // dirty later builds with assets that may not be needed or are out of date.
            CleanOldSettings();

            iOSProviderSettings settings = null;
            EditorBuildSettings.TryGetConfigObject(iOSProviderConstants.k_SettingsKey, out settings);
            if (settings == null)
                return;

            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();

            if (!preloadedAssets.Contains(settings))
            {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        /// <summary>Override of <see cref="IPostprocessBuildWithReport"/></summary>.
        /// <param name="report">Build report.</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            // Always remember to clean up preloaded assets after build to make sure we don't
            // dirty later builds with assets that may not be needed or are out of date.
            CleanOldSettings();

            RestoreNativeLibs();
        }
    }
}
