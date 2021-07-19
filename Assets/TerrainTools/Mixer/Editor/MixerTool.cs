using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    public class MixerTool : TerrainPaintTool<MixerTool>
    {
        [SerializeField]
        private ModulesManager m_modulesManager;
        private ModulesManager modulesManager
        {
            get
            {
                // initialize first time usage or if the reference to the ModulesRegistry SerializedObject has been lost,
                // like when entering and exiting Play Mode
                if( m_modulesManager == null  || m_modulesManager.ModulesSerializedObject.targetObject == null )
                {
                    m_modulesManager = new ModulesManager();
                }

                return m_modulesManager;
            }
        }



        private List<IPaintMode> paintModes = null;


        private IPaintMode paintMode;
        private int paintModeIndex;

        private List<AssetIntegration> assetIntegrations = null;

        private AssetIntegration vegetationStudioProIntegration = new VegetationStudioProIntegration();
        private AssetIntegration vegetationStudioIntegration = new VegetationStudioIntegration();

        int s_TerrainEditorHash = "TerrainEditor".GetHashCode();

        private DelayedActionHandler delayedActionHandler;

        private PathRecorder pathRecorder;

        private bool pathRecorderEnabled = true;

        #region BrushSettings

        private const float kHeightmapBrushScale = 0.01F;
        private const float kMinBrushStrength = (1.1F / ushort.MaxValue) / kHeightmapBrushScale;

        private bool showBrushSize = true;
        private bool showBrushStrength = true;
        private bool showBrushRotation = true;

        BrushSettings brushSettings = new BrushSettings();

        // flag if the brush is currently being resized and not moved
        bool isBrushResizing = false;

        #endregion BrushSettings

        private bool onPaintFinishedRequired = false;

        override public void OnEnable()
        {

        }

        public MixerTool()
        {
            #region Paint Mode

            // register available paint modes
            paintModes = new List<IPaintMode>();

            this.paintModes.Add(new PaintBrushPaintMode());
            this.paintModes.Add(new StrokePaintMode());
            this.paintModes.Add(new WaypointPaintMode());
            this.paintModes.Add(new SplinePaintMode());

            // set initial paint mode
            this.paintModeIndex = 0; // select the first item
            this.paintMode = this.paintModes[this.paintModeIndex];

            #endregion Paint Mode

            // register asset integrations
            assetIntegrations = new List<AssetIntegration>();

            if (vegetationStudioProIntegration.Enabled)
            {
                assetIntegrations.Add(vegetationStudioProIntegration);
            }

            if (vegetationStudioIntegration.Enabled)
            {
                assetIntegrations.Add(vegetationStudioIntegration);
            }

            #region Delayed Action

            this.delayedActionHandler = new DelayedActionHandler();
            this.delayedActionHandler.AddDelayedAction(new DelayedAction(this));

            #endregion Delayed Action

            #region PathRecorder

            pathRecorder = new PathRecorder();

            #endregion PathRecorder

        }

        public override string GetName()
        {
            return "Mixer";
        }

        string toolDescription = "Use combinations of Terrain Tools for e. g. painting a path, a river bed, details, etc with a single stroke";
        #if UNITY_2021_2_OR_NEWER
        public override string GetDescription()
        {
            return toolDescription;
        }
        #else
        public override string GetDesc()
        {
            return toolDescription;
        }
        #endif


        public override void OnSceneGUI(Terrain currentTerrain, IOnSceneGUI editContext)
        {
            if (currentTerrain == null || editContext == null)
                return;

            ///
            /// Handle Input
            ///
            Event evt = Event.current;
            int controlId = GUIUtility.GetControlID(s_TerrainEditorHash, FocusType.Passive);
            switch (evt.GetTypeForControl(controlId))
            {
                case EventType.Layout:
                    // nothing to do
                    break;

                case EventType.MouseDown:

                    if (evt.button == 0 && evt.isMouse)
                    {
                        #region Delayed Action

                        delayedActionHandler.StartDelayedActions();

                        #endregion Delayed Action


                        #region PathRecorder
                        if (pathRecorderEnabled)
                        {
                            // reset path
                            pathRecorder.StartRecording();

                        }
                        #endregion PathRecorder
                    }

                    break;

                case EventType.MouseUp:

                    if (evt.button == 0 && evt.isMouse)
                    {
                        #region Delayed Action

                        delayedActionHandler.ApplyAllDelayedActions();

                        #endregion Delayed Action

                    }
                    break;

                case EventType.MouseDrag:
                    if (evt.button == 0 && evt.isMouse)
                    {
                        #region PathRecorder
                        if (pathRecorderEnabled)
                        {
                            // record path while dragging
                            pathRecorder.AddPosition(editContext.raycastHit.point);

                        }
                        #endregion PathRecorder
                    }
                    break;
            }

            // handle brush rotation
            if (evt.control && evt.type == EventType.ScrollWheel)
            {
                brushSettings.brushRotationDegrees += Event.current.delta.y;

                if (brushSettings.brushRotationDegrees >= 360)
                {
                    brushSettings.brushRotationDegrees -= 360;
                }

                if (brushSettings.brushRotationDegrees < 0)
                {
                    brushSettings.brushRotationDegrees += 360;
                }

                brushSettings.brushRotationDegrees %= 360;

                evt.Use();
                editContext.Repaint();
            }

            // handle brush resize
            if (evt.control && (evt.type == EventType.MouseDrag))
            {

                brushSettings.brushSize += Event.current.delta.x;

                isBrushResizing = true;

                evt.Use();
                editContext.Repaint();
            }
            else if (evt.type == EventType.MouseUp)
            {
                isBrushResizing = false;
            }

            // handle onPaintFinishedRequired flag depending on whether we were painting or not
            switch (evt.GetTypeForControl(controlId))
            {
                // flag that onPaintFinished is required
                case EventType.MouseDown:

                    if (evt.button == 0 && evt.isMouse)
                    {
                        onPaintFinishedRequired = true;
                    }
                    break;

                // perform onPaintFinished if required and invalidate the flag for that information
                case EventType.MouseUp:

                    if (evt.button == 0 && evt.isMouse)
                    {
                        // Perform OnPaintFinished on the integrations which implement it
                        if (onPaintFinishedRequired)
                        {
                            PerformIntegrationOnPaintFinished();
                        }

                        onPaintFinishedRequired = false;
                    }
                    break;

            }

            // We're only doing painting operations, early out if it's not a repaint
            if (Event.current.type != EventType.Repaint)
                return;

            #region PaintMode

            paintMode.OnSceneGUI(currentTerrain, editContext, brushSettings);

            #endregion PaintMode

            #region Modules

            foreach (ModuleEditor module in modulesManager.SceneGuiOrderList)
            {
                if (!module.active)
                    continue;

                module.OnSceneGUI(currentTerrain, editContext, brushSettings);
            }

            #endregion Modules

            #region PathRecorder

            if (pathRecorderEnabled)
            {
                // paint recorded path
                List<Vector3> positions = pathRecorder.GetPositions();
                Handles.DrawAAPolyLine(4, positions.ToArray());
            }

            #endregion PathRecorder


        }


        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {

            // ensure the button styles are initialized
            // there seems to be a problem which first got noticed in 2019.3. OnInspectorGUI is invoked, the MixerStyles.GetButtonToggleStyle invoked, but the styles
            // in there weren't initialized in the static constructor. this is just a quick workaround, will examine this later
            // see also https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors
            // easily reproducable: happened every time the scene got loaded new
            MixerStyles.Init();

            #region General Notification

            EditorGUILayout.HelpBox("Please backup your terrain before you perform any modifications.", MessageType.Info);

            #endregion General Notification

            EditorGUI.BeginChangeCheck();
            {



                #region Paint Mode

                GUILayout.BeginVertical("box");
                {
                    List<GUIContent> paintModeContents = new List<GUIContent>();
                    foreach (IPaintMode editor in paintModes)
                    {
                        paintModeContents.Add(new GUIContent(editor.GetName(), editor.GetDescription()));
                    }

                    EditorGUILayout.LabelField(MixerStyles.paintModesContent, EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();

                    paintModeIndex = GUILayout.Toolbar(paintModeIndex, paintModeContents.ToArray());
                    paintMode = paintModes[paintModeIndex];

                    EditorGUILayout.EndHorizontal();

                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    paintMode.OnInspectorGUI(terrain, editContext, brushSettings);
                }
                GUILayout.EndVertical();

                #endregion Paint Mode



                #region Modules Stack

                EditorGUI.BeginChangeCheck();
                {
                    modulesManager.ModulesSerializedObject.Update();

                    EditorGUILayout.PropertyField(modulesManager.ModulesSerializedProperty, true);

                    modulesManager.ModulesSerializedObject.ApplyModifiedProperties();
                }
                if( EditorGUI.EndChangeCheck())
                {
                    modulesManager.UpdateModulesOrder();
                }

                #endregion Modules Stack

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset"))
                    {
                        modulesManager.Reset();
                    }
                    else if (GUILayout.Button("Load"))
                    {
                        ModulesRegistry modulesRegistry = Persistence.LoadModulesRegistry();
                        modulesManager.ApplyRegistry(modulesRegistry);

                    }
                    else if (GUILayout.Button("Save"))
                    {
                        Persistence.SaveModulesRegistry( modulesManager.ModulesRegistry);
                    }
                }
                EditorGUILayout.EndHorizontal();

                /* replaced by stack
                 * 
                #region Module Editor

                GUILayout.BeginVertical("box");
                {

                    GUILayout.Label(MixerStyles.activeTerrainToolsContent, EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();

                    foreach (ModuleEditor module in modulesRegistry.modules)
                    {
                        // toggle active state
                        if (GUILayout.Button(module.GetName(), MixerStyles.GetButtonToggleStyle(module.active)))
                        {
                            module.active = !module.active;
                        }
                    }


                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                #endregion Module Editor
                */


                #region Module Help

                foreach (ModuleEditor module in modulesManager.Modules)
                {

                    if (!module.active)
                        continue;

                    string helpText = module.GetDescription();

                    if (helpText.Length == 0)
                        continue;

                    GUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.LabelField( module.GetName(), EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox(helpText, MessageType.Info);
                    }
                    GUILayout.EndVertical();
                }

                #endregion Module Help

                #region Module Editor Settings

                foreach (ModuleEditor module in modulesManager.Modules)
                {

                    if (!module.active)
                        continue;

                    if (!module.hasAdditionalInspector())
                        continue;

                    GUILayout.BeginVertical("box");
                    {
                        if (module.GetDescription().Length > 0)
                        {
                            EditorGUILayout.HelpBox(module.GetDescription(), MessageType.Info);
                        }

                        module.OnInspectorGUI(terrain, editContext, brushSettings);
                    }
                    GUILayout.EndVertical();
                }

                #endregion Module Editor Settings


                #region Brush

                GUILayout.BeginVertical("box");
                {
                    // editContext.ShowBrushesGUI(0);

                    #region BrushSettings

                    // show the brush templates
                    editContext.ShowBrushesGUI(0, BrushGUIEditFlags.Select);

                    // info
                    EditorGUILayout.HelpBox(MixerStyles.brushSettingsHelp.text, MessageType.None);

                    if (showBrushSize)
                    {
                        float safetyFactorHack = 0.9375f;
                        brushSettings.brushSize = EditorGUILayout.Slider(MixerStyles.brushSizeStyle, brushSettings.brushSize, 0.1f, Mathf.Round(Mathf.Min(terrain.terrainData.size.x, terrain.terrainData.size.z) * safetyFactorHack));
                    }

                    if (showBrushStrength)
                    {
                        brushSettings.brushStrength = PercentSlider(MixerStyles.brushOpacityStyle, brushSettings.brushStrength, kMinBrushStrength, 1); // former string formatting: "0.0%"
                    }

                    if (showBrushRotation)
                    {
                        brushSettings.brushRotationDegrees = EditorGUILayout.Slider(MixerStyles.brushRotationStyle, brushSettings.brushRotationDegrees, 0, 359);
                        brushSettings.brushRotationDegrees = brushSettings.brushRotationDegrees % 360;
                    }

                    #endregion BrushSettings

                }
                GUILayout.EndVertical();

                #endregion Brush

                #region Integrations
                foreach (AssetIntegration asset in assetIntegrations)
                {
                    GUILayout.BeginVertical("box");
                    {
                        asset.OnInspectorGUI();
                    }
                    GUILayout.EndVertical();
                }
                #endregion Integrations

                #region Debug

                GUILayout.BeginVertical("box");
                {

                    EditorGUILayout.LabelField(MixerStyles.debugContent, EditorStyles.boldLabel);

                    pathRecorderEnabled = EditorGUILayout.Toggle("Show Path", pathRecorderEnabled);

                }
                GUILayout.EndVertical();
                #endregion Debug

            }
            if (EditorGUI.EndChangeCheck())
            {
                Save(true);

                // update scene view, otherwise eg changing the "show path" option wouldn't be visualized immediately
                SceneView.RepaintAll();

            }

            base.OnInspectorGUI(terrain, editContext);
        }



        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {

            StrokeSegment[] segments = paintMode.OnPaint(terrain, editContext, brushSettings);

            // abort if we have nothing to paint
            if (segments == null)
                return true;

            // perform paint operation
            PaintSegments(segments, editContext);

            // perform operatons on integrations
            OnPaintIntegrations(segments, terrain, editContext);

            return false;
        }


        #region Integrations
        private void OnPaintIntegrations(StrokeSegment[] segments, Terrain terrain, IOnPaint editContext)
        {

            // integreation calls are redundant if the system isn't active.
            // but this way we can move the #if-endif clauses to a dedicated class and don't
            // have to clutter the main class. another option might be to have a dedicated
            // integraction checker class and return a list of what's active and what not.
            // we'll see how it develops

            foreach (AssetIntegration asset in assetIntegrations)
            {
                if (!asset.Active)
                    continue;

                vegetationStudioProIntegration.Update(segments, editContext);
            }
        }

        /// <summary>
        /// Perform OnPaintFinished on the integrations which implement it
        /// Example: Vegetation System Refresh for VS Pro
        /// </summary>
        private void PerformIntegrationOnPaintFinished()
        {

            // integreation calls are redundant if the system isn't active.
            // but this way we can move the #if-endif clauses to a dedicated class and don't
            // have to clutter the main class. another option might be to have a dedicated
            // integraction checker class and return a list of what's active and what not.
            // we'll see how it develops

            foreach (AssetIntegration asset in assetIntegrations)
            {
                if (!asset.Active)
                    continue;

                vegetationStudioProIntegration.OnPaintFinished();
            }

        }

        #endregion Integrations

        /// <summary>
        /// Perform paint operations on registered and active modules
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="editContext"></param>
        private void PaintSegments(StrokeSegment[] segments, IOnPaint editContext)
        {
            foreach (ModuleEditor module in modulesManager.PaintSegmentOrderList)
            {
                if (!module.active)
                    continue;

                #region Delayed Actions

                // skip paintmodule if delayed actions is active, it has to be painted over all other modules
                if (UseDelayedActions())
                {
                    if (module == modulesManager.PaintModule)
                        continue;
                }

                #endregion Delayed Actions

                module.PaintSegments(segments, editContext, brushSettings);
            }

            #region Delayed Actions

            if (UseDelayedActions())
            {
                // add and apply delayed actions in batches
                delayedActionHandler.AddDelayedAction(segments, editContext, brushSettings);
                delayedActionHandler.ApplyDelayedActions();
            }

            #endregion Delayed Actions
        }

        #region Delayed Actions

        private bool UseDelayedActions()
        {
            return modulesManager.PaintModule.active && modulesManager.UnderlayModule.active;

        }

        private class DelayedAction : IDelayedAction
        {
            private MixerTool parent;

            public DelayedAction(MixerTool parent)
            {
                this.parent = parent;
            }

            public void OnActionPerformed(DelayedActionContext actionContext)
            {
                foreach (ModuleEditor module in parent.modulesManager.PaintSegmentOrderList)
                {
                    if (!module.active)
                        continue;

                    // apply paintmodule if delayed actions are active, it has to be painted over all other modules
                    if (module == parent.modulesManager.PaintModule)
                    {
                        module.PaintSegments(actionContext.segments, actionContext.editContext, actionContext.brushSettings);
                    }

                }
            }
        }

        #endregion Delayed Actions

        #region BrushSettings
        static float PercentSlider(GUIContent content, float valueInPercent, float minVal, float maxVal)
        {
            EditorGUI.BeginChangeCheck();
            float v = EditorGUILayout.Slider(content, Mathf.Round(valueInPercent * 100f), minVal * 100f, maxVal * 100f);

            if (EditorGUI.EndChangeCheck())
            {
                return v / 100f;
            }
            return valueInPercent;
        }
        #endregion BrushSettings


    }
}
