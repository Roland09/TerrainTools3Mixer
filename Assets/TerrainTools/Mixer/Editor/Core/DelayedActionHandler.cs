using System.Collections.Generic;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.TerrainTools;
#else
using UnityEditor.Experimental.TerrainAPI;
#endif

namespace Rowlan.TerrainTools.Mixer
{

    public class DelayedActionHandler
    {

        /// <summary>
        /// How many actions will be in the buffer
        /// </summary>
        int delayedActionBufferCount = 10;

        private List<DelayedActionContext> actionContextList = new List<DelayedActionContext>();
        private List<IDelayedAction> actionList = new List<IDelayedAction>();

        public void AddDelayedAction(IDelayedAction action)
        {
            actionList.Add(action);
        }

        public void AddDelayedAction(StrokeSegment[] segments, IOnPaint editContext, BrushSettings brushSettings)
        {
            actionContextList.Add(new DelayedActionContext(segments, editContext, brushSettings));
        }

        public void StartDelayedActions()
        {
            actionContextList.Clear();
        }

        public void ApplyDelayedActions()
        {
            while (actionContextList.Count > delayedActionBufferCount)
            {
                DelayedActionContext actionContext = actionContextList[0];
                actionContextList.RemoveAt(0);

                ApplyDelayedAction(actionContext);

            }

        }

        public void ApplyAllDelayedActions()
        {
            foreach (DelayedActionContext actionContext in actionContextList)
            {
                ApplyDelayedAction(actionContext);
            }

            actionContextList.Clear();

        }

        private void ApplyDelayedAction(DelayedActionContext actionContext)
        {
            foreach(IDelayedAction action in actionList)
            {
                action.OnActionPerformed(actionContext);
            }

        }

    }

}