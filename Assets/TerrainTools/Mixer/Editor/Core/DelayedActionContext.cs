#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{
    /// <summary>
    /// Container for terrain paint data which will be applied delayed in a postprocess step
    /// </summary>
    public class DelayedActionContext
    {
        public StrokeSegment[] segments;
        public IOnPaint editContext;
        public BrushSettings brushSettings;

        public DelayedActionContext(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            this.segments = segments;
            this.editContext = editContext;
            this.brushSettings = brushSettings;
        }
    }
}
