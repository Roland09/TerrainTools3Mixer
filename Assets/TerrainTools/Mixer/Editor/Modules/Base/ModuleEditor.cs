using System;
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
    /// <summary>
    /// The module editor & settings base class.
    /// Must be Serializable because it's used in a SerialReference list
    /// </summary>
    [Serializable]
    public abstract class ModuleEditor
    {
        public enum Type
        {
            Sculpt,
            Paint,
            Underlay
        }

        [HideInInspector]
        [SerializeField]
        public Type type;

        [SerializeField]
        public string name = "<ModuleEditor.name>";

        [SerializeField]
        public string description = "<ModuleEditor.description>";

        [SerializeField]
        public bool active;

        [SerializeField]
        public float brushSize = 100f;

        [SerializeField]
        public float brushStrength = 80f;

        [HideInInspector]
        [SerializeField]
        public int sceneGuiOrder;

        [HideInInspector]
        [SerializeField]
        public int paintSegmentOrder;

        public ModuleEditor( Type type, bool active, float brushSize, float brushStrength, int sceneGuiOrder, int paintSegmentOrder)
        {
            this.type = type;
            this.name = GetName();
            this.description = GetDescription();

            this.active = active;
            this.brushSize = brushSize;
            this.brushStrength = brushStrength;

            this.sceneGuiOrder = sceneGuiOrder;
            this.paintSegmentOrder = paintSegmentOrder;
        }

        public abstract string GetName();

        public abstract string GetDescription();

        public abstract bool hasAdditionalInspector();

        public abstract void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext, BrushSettings brushSettings);

        public abstract void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext, BrushSettings brushSettings);

        public abstract void PaintSegments(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings);

    }
}