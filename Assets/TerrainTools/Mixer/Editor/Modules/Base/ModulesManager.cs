using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Rowlan.TerrainTools.Mixer
{
    public class ModulesManager 
    {
        private ModulesRegistry m_modulesRegistry;
        public ModulesRegistry modulesRegistry
        {
            get {

                if( m_modulesRegistry == null) 
                {
                    m_modulesRegistry = ScriptableObject.CreateInstance<ModulesRegistry>();
                }
                return m_modulesRegistry;
            }
        }

        private SerializedObject modulesSerializedObject;
        private SerializedProperty modulesSerializedProperty;

        // the modules of the stack
        private ModuleEditor pathModule;
        private ModuleEditor smoothModule;
        private ModuleEditor paintModule;
        private ModuleEditor smudgeModule;
        private ModuleEditor ridgeErodeModule;
        private ModuleEditor heightModule;
        private ModuleEditor underlayModule;

        private List<ModuleEditor> onSceneGuiOrderList = null;
        private List<ModuleEditor> paintSegmentOrderList = null;

        public ModulesManager() {

            Reset();
            CreateSerializedData();

        }

        private void CreateSerializedData()
        { 
            modulesSerializedObject = new SerializedObject(modulesRegistry);
            modulesSerializedProperty = modulesSerializedObject.FindProperty(ModulesRegistry.MODULES_VARIABLE);
            modulesSerializedObject.ApplyModifiedProperties();
        }

        private void CreateDefaultModules()
        {

            // add modules: order in the inspector
            // initialize the modules
            //                                       active,  brush size, brush strength, sceneGuiOrder, paintSegmentOrder
            pathModule = new PathModule(               true,        100f,           100f,             3,                 1);
            smoothModule = new SmoothModule(           true,        150f,            20f,             1,                 2);
            paintModule = new PaintModule(             true,         80f,           100f,             7,                 7);
            smudgeModule = new SmudgeModule(          false,        150f,            20f,             2,                 3);
            ridgeErodeModule = new RidgeErodeModule(  false,        150f,            16f,             5,                 5);
            heightModule = new HeightModule(          false,        100f,            20f,             4,                 4);
            underlayModule = new UnderlayModule(      false,        140f,           100f,             6,                 6);

        }

        public void Reset()
        {
            // need to create new instances, the inspector changes the object data
            CreateDefaultModules();

            // register available modules
            modulesRegistry.modules.Clear();

            modulesRegistry.modules.Add(paintModule);
            modulesRegistry.modules.Add(pathModule);
            modulesRegistry.modules.Add(smoothModule);
            modulesRegistry.modules.Add(heightModule);
            modulesRegistry.modules.Add(ridgeErodeModule);
            modulesRegistry.modules.Add(smudgeModule);
            modulesRegistry.modules.Add(underlayModule);

            UpdateModulesOrder();

        }

        public void ApplyRegistry( ModulesRegistry modulesRegistry)
        {
            this.m_modulesRegistry = modulesRegistry;

            UpdateModulesOrder();

            CreateSerializedData();

        }

        /// <summary>
        /// Set the modules order for the brush rendering and the paint logic
        /// </summary>
        public void UpdateModulesOrder()
        {
            // sort order in OnSceneGui: order of the brush
            this.onSceneGuiOrderList = new List<ModuleEditor>();
            this.onSceneGuiOrderList.AddRange( modulesRegistry.modules);
            this.onSceneGuiOrderList =  modulesRegistry.modules.OrderBy(x => x.sceneGuiOrder).ToList();

            // sort order in paintSegment execution: order of paint execution
            this.paintSegmentOrderList = new List<ModuleEditor>();
            this.paintSegmentOrderList.AddRange(Modules);
            this.paintSegmentOrderList = Modules.OrderBy(x => x.paintSegmentOrder).ToList();

        }

        public List<ModuleEditor> Modules {  get { return modulesRegistry.modules; } }
        public SerializedObject ModulesSerializedObject { get { return modulesSerializedObject; } }
        public SerializedProperty ModulesSerializedProperty { get { return modulesSerializedProperty; } }
        public ModulesRegistry ModulesRegistry {  get { return modulesRegistry; } }
        public PaintModule PaintModule { get { return paintModule as PaintModule; } }
        public UnderlayModule UnderlayModule { get { return underlayModule as UnderlayModule; } }

        public List<ModuleEditor> SceneGuiOrderList {  get { return onSceneGuiOrderList; } }
        public List<ModuleEditor> PaintSegmentOrderList {  get { return paintSegmentOrderList; } }

    }
}
