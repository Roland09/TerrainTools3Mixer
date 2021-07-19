using UnityEditor;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    public class SplinePaintMode : IPaintMode
    {
        public string GetName()
        {
            return "Spline";
        }

        public string GetDescription()
        {
            return "";
        }

        public void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext, BrushSettings brushSettings)
        {
            EditorGUILayout.LabelField("Spline", EditorStyles.boldLabel);

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
