using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.TerrainTools.Mixer
{
    /// <summary>
    /// Container for the modules.
    /// This will be used in the array view
    /// </summary>
    [Serializable]
    public class ModulesRegistry : ScriptableObject
    {
        /// <summary>
        /// The name of the modules variable. 
        /// It's being used as property, we don't want magic strings.
        /// </summary>
        public static readonly string MODULES_VARIABLE = "modules";

        /// <summary>
        /// Collection of the available modules.
        /// Note: if the variable name changes, you also need to change <see cref="MODULES_VARIABLE"/>
        /// Important: The object of the List must be Serializable
        /// </summary>
        [SerializeReference]
        public List<ModuleEditor> modules = new List<ModuleEditor>();

    }
}
