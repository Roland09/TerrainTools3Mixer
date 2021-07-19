using UnityEditor;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    public class WaypointPaintMode : IPaintMode
    {
        public string GetName()
        {
            return "Waypoint";
        }

        public string GetDescription()
        {
            return "";
        }

        public void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContex, BrushSettings brushSettingst)
        {
            EditorGUILayout.LabelField("Waypoint", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(MixerStyles.notImplemented.text, MessageType.Error);
        }

        public StrokeSegment[] OnPaint(Terrain terrain, IOnPaint editContext, BrushSettings brushSettings)
        {
            // TODO implement feature
            return null;
        }

        public void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext, BrushSettings brushSettings)
        {
            // TODO implement feature
        }
    }
}
