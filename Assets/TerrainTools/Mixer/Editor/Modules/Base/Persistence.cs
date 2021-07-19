using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.TerrainTools.Mixer
{
    public class Persistence
    {
        private static string MODULES_REGISTRY_DEFAULT_FILE_PATH_PART = "Default";

        /// <summary>
        /// Load a modules registry
        /// </summary>
        /// <returns></returns>
        public static ModulesRegistry LoadModulesRegistry() 
        {
            string prefix = MODULES_REGISTRY_DEFAULT_FILE_PATH_PART;
            string filePath = getFilterStackFilePath(prefix);

            Debug.Log( $"Loading from {filePath}");

            UnityEngine.Object[] obs = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(filePath);
            ModulesRegistry modulesRegistry = obs[0] as ModulesRegistry;

            return modulesRegistry;

        }

        /// <summary>
        /// Save the modules registry
        /// </summary>
        /// <param name="modulesRegistry"></param>
        public static void SaveModulesRegistry( ModulesRegistry modulesRegistry) 
        {
            List< UnityEngine.Object > objList = new List< UnityEngine.Object >();

            objList.Add( modulesRegistry );

            string prefix = MODULES_REGISTRY_DEFAULT_FILE_PATH_PART;
            string filePath = getFilterStackFilePath(prefix);

            Debug.Log( $"Saving to {filePath}");

            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(objList.ToArray(), filePath, true );
        }

        private static string getFilterStackFilePath( string pathPart)
        {
            return Application.persistentDataPath + "/TerrainTools_Mixer_" + pathPart + ".modulesstack"; 
            
        }
    }
}
