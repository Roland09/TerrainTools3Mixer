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
    public class HeightModule : ModuleEditor
    {

        #region Fields

        #endregion Fields

        private static Color heightBrushColor = new Color(1.0f, 1.0f, 0.4f, 0.3f);

        public HeightModule(bool active, float brushSize, float brushStrength, int sceneGuiOrder, int paintSegmentOrder) : base(active, brushSize, brushStrength, sceneGuiOrder, paintSegmentOrder)
        {
        }

        override public string GetName()
        {
            return "Height";
        }

        override public string GetDescription()
        {
            return "Left click to raise. Shift and left click to lower the terrain.";
        }


        override public void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext, BrushSettings brushSettings)
        {
            if (editContext.hitValidTerrain)
            {
                Terrain terrain = currentTerrain;

                // the smooth brush size is relative to the main brush size
                float brushSize = brushSettings.brushSize * this.brushSize / 100f;

                BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord, brushSize, brushSettings.brushRotationDegrees);
                PaintContext ctx = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
                Material brushPreviewMat = BrushUtilities.GetDefaultBrushPreviewMaterial();
                brushPreviewMat.color = heightBrushColor;
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
            EditorGUILayout.LabelField("Height", EditorStyles.boldLabel);

            brushSize = EditorGUILayout.Slider(new GUIContent("Brush Size [% of Main Brush]", ""), brushSize, 0.0f, 200.0f);
            brushStrength = EditorGUILayout.Slider(new GUIContent("Brush Strength", ""), brushStrength, 0, 200);
            */
        }

        override public void PaintSegments(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            for (int i = 0; i < segments.Length; i++)
            {

                StrokeSegment segment = segments[i];

                Height(segment.currTerrain, editContext, segment.currUV, segment.prevUV, brushSettings);
            }
        }


        private bool Height(Terrain terrain, IOnPaint editContext, Vector2 currUV, Vector2 prevUV, BrushSettings brushSettings)
        {
            // the brush size is relative to the main brush size
            float brushSize = brushSettings.brushSize * this.brushSize / 100f;

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, currUV, brushSize, brushSettings.brushRotationDegrees);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

            paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;

            Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();

            float brushStrength = this.brushStrength / 100f;

            brushStrength = Event.current.shift ? -brushStrength : brushStrength;

            brushStrength *= 0.001f; // magic number ...

            Vector4 brushParams = new Vector4( brushStrength, 0.0f, 0.0f, 0.0f);

            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);

            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

            // custom Undo. otherwise undo of mixing texture paint and terrain modification won't work
            BrushUndo.RegisterUndo(terrain, paintContext, "MixerTool");

            // no undo, we have our own
            TerrainPaintUtility.EndPaintHeightmap(paintContext, null);

            return true;
        }
    }
}
