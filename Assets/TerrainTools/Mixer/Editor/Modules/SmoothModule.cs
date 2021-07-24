using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
using UnityEngine.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    [Serializable]
    public class SmoothModule : ModuleEditor
    {
        #region Fields

        [SerializeField]
        float direction = 0.0f;     // -1 to 1

        #endregion Fields

        private static Color smoothBrushColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        public SmoothModule(Type type, bool active, float brushSize, float brushStrength, int sceneGuiOrder, int paintSegmentOrder) : base(type, active, brushSize, brushStrength, sceneGuiOrder, paintSegmentOrder)
        {
        }

        override public string GetName()
        {
            return "Smooth";
        }

        override public string GetDescription()
        {
            return "";
        }


        override public void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext, BrushSettings brushSettings)
        {
            if (editContext.hitValidTerrain)
            {
                Terrain terrain = currentTerrain;

                // the brush size is relative to the main brush size
                float brushSize = brushSettings.brushSize * this.brushSize / 100f;

                BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord, brushSize, brushSettings.brushRotationDegrees);
                PaintContext ctx = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
                Material brushPreviewMat = BrushUtilities.GetDefaultBrushPreviewMaterial();
                brushPreviewMat.color = smoothBrushColor;
                BrushUtilities.DrawBrushPreview(ctx, BrushUtilities.BrushPreview.SourceRenderTexture, editContext.brushTexture, brushXform, brushPreviewMat, 0);
                TerrainPaintUtility.ReleaseContextResources(ctx);
            }
        }

        override public bool hasAdditionalInspector()
        {
            return false;
        }

        override public void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext, BrushSettings brushSettings)
        {
            /*
            EditorGUILayout.LabelField("Smooth", EditorStyles.boldLabel);
            
            brushSize = EditorGUILayout.Slider(new GUIContent("Brush Size [% of Main Brush]", ""), brushSize, 0.0f, 300.0f);
            brushStrength = EditorGUILayout.Slider(new GUIContent("Brush Strength", ""), brushStrength, 0.0f, 100.0f);

            direction = EditorGUILayout.Slider(new GUIContent("Blur Direction", "Blur only up (1.0), only down (-1.0) or both (0.0)"), direction, -1.0f, 1.0f);
            */
        }

        override public void PaintSegments(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                StrokeSegment segment = segments[i];

                Smooth(segment.currTerrain, editContext, segment.currUV, brushSettings);

            }
        }

        private bool Smooth(Terrain terrain, IOnPaint editContext, Vector2 currUV, BrushSettings brushSettings)
        {
            // the brush size is relative to the main brush size
            float brushSize = brushSettings.brushSize * this.brushSize / 100f;

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, brushSize, brushSettings.brushRotationDegrees);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());

            Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();

            float brushStrength = this.brushStrength / 100f; // editContext.brushStrength;

            // brushStrength = Event.current.shift ? -brushStrength : brushStrength;
            Vector4 smoothWeights = new Vector4(
               Mathf.Clamp01(1.0f - Mathf.Abs(direction)),   // centered
               Mathf.Clamp01(-direction),                    // min
               Mathf.Clamp01(direction),                     // max
               0.0f);                                          // unused

            Vector4 brushParams = new Vector4(brushStrength, 0.0f, 0.0f, 0.0f);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            mat.SetVector("_SmoothWeights", smoothWeights);

            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);

            #if UNITY_2021_2_OR_NEWER
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainBuiltinPaintMaterialPasses.SmoothHeights);
            #else
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SmoothHeights);
            #endif
            

            // custom Undo. otherwise undo of mixing texture paint and terrain modification won't work
            BrushUndo.RegisterUndo(terrain, paintContext, "MixerTool");

            // no undo, we have our own
            TerrainPaintUtility.EndPaintHeightmap(paintContext, null);

            return true;
        }

        [CustomPropertyDrawer(typeof(SmoothModule))]
        public class SmoothModulePropertyDrawer : ModuleEditorPropertyDrawer
        {
            private GUIContent directionLabel = EditorGUIUtility.TrTextContent("Blur Direction", "Blur only up (1.0), only down (-1.0) or both (0.0)");

            public override void AddDetailProperties(Rect position, SerializedProperty property)
            {

                SerializedProperty direction = property.FindPropertyRelative("direction");

                float labelWidth = Mathf.Max(GUI.skin.label.CalcSize(directionLabel).x);

                AddSlider(position, direction, -1f, 1f, labelWidth, directionLabel);
            }

            /* background panel (disabled for now)
            override public float GetDetailHeight()
            {
                return EditorGUIUtility.singleLineHeight * 1;

            }
            */
        }
    }
}
