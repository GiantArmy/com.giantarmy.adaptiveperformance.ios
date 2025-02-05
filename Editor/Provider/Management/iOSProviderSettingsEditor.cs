using UnityEngine;
using UnityEditor.AdaptivePerformance.Editor;

using GiantArmy.AdaptivePerformance.iOS;
using UnityEditor;

namespace GiantArmy.AdaptivePerformance.iOS.Editor
{
    /// <summary>
    /// This is custom Editor for iOS Provider Settings.
    /// </summary>
    [CustomEditor(typeof(iOSProviderSettings))]
    public class iOSProviderSettingsEditor : ProviderSettingsEditor
    {
        const string k_iOSProviderLogging = "m_iOSProviderLogging";

        static GUIContent s_iOSProviderLoggingLabel = EditorGUIUtility.TrTextContent(L10n.Tr("iOS Provider Logging"), L10n.Tr("Only active in development mode."));

        static string s_UnsupportedInfo = L10n.Tr("Adaptive Performance iOS settings not available on this platform.");
        SerializedProperty m_iOSProviderLoggingProperty;

        /// <summary>
        /// Override of provider options to indicate if boost is available.
        /// </summary>
        protected override bool IsBoostAvailable => false;
        /// <summary>
        /// Override of provider options to indicate if Auto Performance mode is available.
        /// </summary>
        protected override bool IsAutoPerformanceModeAvailable => false;

#if UNITY_2023_1_OR_NEWER
        /// <summary>
        /// Controls whether or not the 'AutomaticGameModeEnabled' option is available. Default value is <c>false</c>.
        /// </summary>
        protected override bool IsAutoGameModeAvailable => false;
#endif

        /// <summary>
        /// Override of Editor callback to display custom settings.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (!DisplayBaseSettingsBegin())
                return;

            if (m_iOSProviderLoggingProperty == null)
                m_iOSProviderLoggingProperty = serializedObject.FindProperty(k_iOSProviderLogging);

            BuildTargetGroup selectedBuildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();

            if (selectedBuildTargetGroup == BuildTargetGroup.iOS)
            {
                EditorGUIUtility.labelWidth = 180; // some property labels are cut-off
                DisplayBaseRuntimeSettings();
                EditorGUILayout.Space();

                DisplayBaseDeveloperSettings();
                if (m_ShowDevelopmentSettings)
                {
                    EditorGUI.indentLevel++;
                    GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
                    EditorGUILayout.PropertyField(m_iOSProviderLoggingProperty, s_iOSProviderLoggingLabel);
                    GUI.enabled = true;
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.HelpBox(s_UnsupportedInfo, MessageType.Info);
                EditorGUILayout.Space();
            }
            DisplayBaseSettingsEnd();
        }
    }
}
