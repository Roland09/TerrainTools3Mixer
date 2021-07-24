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
    public class UnderlayModule : ModuleEditor
    {
        #region Fields

        [HideInInspector]
        [SerializeField]
        TerrainLayer m_SelectedInnerTerrainLayer = null;

        [HideInInspector]
        [SerializeField]
        int innerLayerIndex = -1;

        #endregion Fields

        private Color paintBrushColor = new Color(1.0f, 0.6f, 0, 0.6f);

        public UnderlayModule(Type type, bool active, float brushSize, float brushStrength, int sceneGuiOrder, int paintSegmentOrder) : base(type, active, brushSize, brushStrength, sceneGuiOrder, paintSegmentOrder)
        {
        }

        override public string GetName()
        {
            return "Underlay";
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

                // the smooth brush size is relative to the main brush size
                float brushSize = brushSettings.brushSize * this.brushSize / 100f;

                BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord, brushSize, brushSettings.brushRotationDegrees);
                PaintContext ctx = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
                Material brushPreviewMat = BrushUtilities.GetDefaultBrushPreviewMaterial();
                brushPreviewMat.color = paintBrushColor;
                BrushUtilities.DrawBrushPreview(ctx, BrushUtilities.BrushPreview.SourceRenderTexture, editContext.brushTexture, brushXform, brushPreviewMat, 0);
                TerrainPaintUtility.ReleaseContextResources(ctx);
            }
        }

        override public bool hasAdditionalInspector()
        {
            return true;
        }

        override public void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext, BrushSettings brushSettings)
        {
            if (innerLayerIndex == -1)
            {
                EditorGUILayout.HelpBox(MixerStyles.noTextureSelectedContent.text, MessageType.Warning);
            }

            EditorGUILayout.Space();

            innerLayerIndex = LayerUtilities.ShowTerrainLayersSelection("Underlay", terrain, innerLayerIndex);
            m_SelectedInnerTerrainLayer = LayerUtilities.FindTerrainLayer(terrain, innerLayerIndex);

            /*
            brushSize = EditorGUILayout.Slider(new GUIContent("Brush Size [% of Main Brush]", ""), brushSize, 0.0f, 200.0f);
            brushStrength = EditorGUILayout.Slider(new GUIContent("Brush Strength", ""), brushStrength, 0.0f, 100.0f);
            */   
        }

        override public void PaintSegments(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                StrokeSegment segment = segments[i];

                PaintTexture(segment.currTerrain, editContext, segment.currUV, brushSettings);
            }
        }

        private bool PaintTexture(Terrain terrain, IOnPaint editContext, Vector2 currUV, BrushSettings brushSettings)
        {
            // the brush size is relative to the main brush size
            float brushSize = brushSettings.brushSize * this.brushSize / 100f;

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, currUV, brushSize, brushSettings.brushRotationDegrees);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintTexture(terrain, brushXform.GetBrushXYBounds(), m_SelectedInnerTerrainLayer);

            if (paintContext == null)
                return false;


            Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();

            float targetAlpha = 1.0f;       // always 1.0 now -- no subtractive painting (we assume this in the ScatterAlphaMap)
            float brushStrength = this.brushStrength / 100f; // editContext.brushStrength

            // apply brush
            Vector4 brushParams = new Vector4(brushStrength, targetAlpha, 0.0f, 0.0f);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);

            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);

            #if UNITY_2021_2_OR_NEWER
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainBuiltinPaintMaterialPasses.PaintTexture);
            #else
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.PaintTexture);
            #endif

            // custom Undo. otherwise undo of mixing texture paint and terrain modification won't work
            BrushUndo.RegisterUndo(terrain, paintContext, "MixerTool");

            // no undo, we have our own
            TerrainPaintUtility.EndPaintTexture(paintContext, null);

            return true;
        }
    }
}
