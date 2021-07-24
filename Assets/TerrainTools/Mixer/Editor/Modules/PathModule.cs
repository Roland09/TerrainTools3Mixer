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
    public class PathModule : ModuleEditor
    {
        #region Material
        Material material = null;
        Material GetMaterial()
        {
            if (material == null)
                material = new Material(Shader.Find("Hidden/TerrainTools/MixerTool/SetExactHeight"));
            return material;
        }
        #endregion Material

        #region Fields

        [SerializeField]
        AnimationCurve widthProfile = AnimationCurve.Linear(0, 1, 1, 1);

        [SerializeField]
        AnimationCurve heightProfile = AnimationCurve.Linear(0, 0, 1, 0);

        [SerializeField]
        AnimationCurve strengthProfile = AnimationCurve.Linear(0, 1, 1, 1);

        #endregion Fields

        private static Color pathBrushColor = new Color(0.6f, 0.6f, 1.0f, 0.7f);

        public PathModule(Type type, bool active, float brushSize, float brushStrength, int sceneGuiOrder, int paintSegmentOrder) : base(type, active, brushSize, brushStrength, sceneGuiOrder, paintSegmentOrder)
        {
        }

        override public string GetName()
        {
            return "Path";
        }

        override public string GetDescription()
        {
            return "";
        }

        override public void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext, BrushSettings brushSettings)
        {
            
            Terrain terrain = currentTerrain;
            float brushSize = brushSettings.brushSize * this.brushSize / 100f;

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord, brushSize, brushSettings.brushRotationDegrees);
            PaintContext ctx = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
            Material brushPreviewMat = BrushUtilities.GetDefaultBrushPreviewMaterial();
            brushPreviewMat.color = pathBrushColor;
            BrushUtilities.DrawBrushPreview(ctx, BrushUtilities.BrushPreview.SourceRenderTexture, editContext.brushTexture, brushXform, brushPreviewMat, 0);
            TerrainPaintUtility.ReleaseContextResources(ctx);

        }

        override public bool hasAdditionalInspector()
        {
            return false;
        }

        override public void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext, BrushSettings brushSettings)
        {
            /*
            EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);

            //"Controls the width of the path over the length of the stroke"
            // we don't make those editable for now
            widthProfile = EditorGUILayout.CurveField(MixerStyles.widthProfileContent, widthProfile);
            heightProfile = EditorGUILayout.CurveField(MixerStyles.heightProfileContent, heightProfile);
            strengthProfile = EditorGUILayout.CurveField(MixerStyles.strengthProfileContent, strengthProfile);
            */

        }



        override public void PaintSegments(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                StrokeSegment segment = segments[i];

                Path(segment.currTerrain, editContext, segment.currUV, segment.pct, segment.stroke, segment.startPoint, brushSettings);

            }
        }

        private bool Path(Terrain terrain, IOnPaint editContext, Vector2 currUV, float pct, Vector3 stroke, Vector3 startPoint, BrushSettings brushSettings)
        {
            float heightOffset = heightProfile.Evaluate(pct) / terrain.terrainData.size.y;
            float strengthScale = strengthProfile.Evaluate(pct);
            float widthScale = widthProfile.Evaluate(pct);

            float finalHeight = ( startPoint + pct * stroke).z + heightOffset;
            float brushSize = brushSettings.brushSize * this.brushSize / 100f;

            int finalBrushSize = (int)(widthScale * (float)brushSize);

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, currUV, finalBrushSize, brushSettings.brushRotationDegrees);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());

            Material mat = GetMaterial();
            Vector4 brushParams = new Vector4();

            mat.SetTexture("_BrushTex", editContext.brushTexture);

            brushParams.x = brushSettings.brushStrength * /* strengthScale */ (this.brushStrength / 100f);
            brushParams.y = 0.5f * finalHeight;

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
