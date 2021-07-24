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
    public class RidgeErodeModule : ModuleEditor
    {

        #region Materials
        Material material = null;
        Material GetMaterial()
        {
            if (material == null)
                material = new Material(Shader.Find("Hidden/TerrainTools/MixerTool/RidgeErode"));
            return material;
        }
        #endregion Materials

        #region Fields

        [SerializeField]
        float strength = 16f;

        [SerializeField]
        float sharpness = 0.7f;

        #endregion Fields

        private static Color ridgeErodeBrushColor = new Color(1.0f, 0.7f, 0.5f, 0.2f);

        public RidgeErodeModule(Type type, bool active, float brushSize, float brushStrength, int sceneGuiOrder, int paintSegmentOrder) : base( type, active, brushSize, brushStrength, sceneGuiOrder, paintSegmentOrder)
        {
        }

        override public string GetName()
        {
            return "Erosion";
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
                brushPreviewMat.color = ridgeErodeBrushColor;
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
            EditorGUILayout.LabelField("Ridge Erode", EditorStyles.boldLabel);

            brushSize = EditorGUILayout.Slider(new GUIContent("Brush Size [% of Main Brush]", ""), brushSize, 0.0f, 300.0f);
            brushStrength = EditorGUILayout.Slider(new GUIContent("Brush Strength", ""), brushStrength, 0.0f, 100.0f);

            erosionStrength = EditorGUILayout.Slider(new GUIContent("Erosion Strength", ""), erosionStrength, 0.0f, 128.0f);
            mixStrength = EditorGUILayout.Slider(new GUIContent("Sharpness", ""), mixStrength, 0.0f, 1.0f);
            */

        }

        override public void PaintSegments(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            for (int i = 0; i < segments.Length; i++)
            {

                StrokeSegment segment = segments[i];

                Smudge(segment.currTerrain, editContext, segment.currUV, segment.prevUV, brushSettings);
            }
        }


        private bool Smudge(Terrain terrain, IOnPaint editContext, Vector2 currUV, Vector2 prevUV, BrushSettings brushSettings)
        {
            // the brush size is relative to the main brush size
            float brushSize = brushSettings.brushSize * this.brushSize / 100f;

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, currUV, brushSize, brushSettings.brushRotationDegrees);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

            Vector2 smudgeDir = editContext.uv - prevUV;

            paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;

            Material mat = GetMaterial();

            float brushStrength = this.brushStrength / 100f; // editContext.brushStrength;

            Vector4 brushParams = new Vector4(brushStrength, strength, sharpness, 0);
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

        [CustomPropertyDrawer(typeof(RidgeErodeModule))]
        public class RidgeErodeModulePropertyDrawer : ModuleEditorPropertyDrawer
        {
            private GUIContent strengthLabel = EditorGUIUtility.TrTextContent("Strength", "");
            private GUIContent sharpnessLabel = EditorGUIUtility.TrTextContent("Sharpness", "");

            public override void AddDetailProperties(Rect position, SerializedProperty property)
            {
                SerializedProperty strength = property.FindPropertyRelative("strength");
                SerializedProperty sharpness = property.FindPropertyRelative("sharpness");

                float labelWidth = Mathf.Max(
                    GUI.skin.label.CalcSize(strengthLabel).x,
                    GUI.skin.label.CalcSize(sharpnessLabel).x
                    );

                AddSlider(position, strength, 0.0f, 128.0f, labelWidth, strengthLabel);
                AddSlider(position, sharpness, 0.0f, 1.0f, labelWidth, sharpnessLabel);

            }

            /* background panel (disabled for now)
            override public float GetDetailHeight()
            {
                return EditorGUIUtility.singleLineHeight * 2;

            }
            */
        }
    }
}
