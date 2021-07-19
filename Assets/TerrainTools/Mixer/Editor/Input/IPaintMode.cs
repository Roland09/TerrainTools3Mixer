using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    public interface IPaintMode
    {
        string GetName();

        string GetDescription();

        void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext, BrushSettings brushSettings);

        void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext, BrushSettings brushSettings);

        StrokeSegment[] OnPaint(Terrain terrain, IOnPaint editContext, BrushSettings brushSettings);
    }
}
