using UnityEngine;
using System.Collections;

public class QteEventTimeScaleEditor : QteEventEditor
{
    public override void DrawCustom()
    {
        using (new AutoBeginVertical())
        {
            GUILayout.Label("缩放度");
            GUILayout.Label("缩放时间");
        }
    }
}
