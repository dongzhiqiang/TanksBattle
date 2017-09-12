using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteEventBlurEditor : QteEventEditor
{
    public QteEventCamera qteEvent;
    public override void DrawCustom()
    {
        if (qteEvent == null)
            qteEvent = m_event as QteEventCamera;
        using (new AutoBeginVertical())
        {
            qteEvent.beginSmooth = EditorGUILayout.FloatField("开始模糊时间:", qteEvent.beginSmooth);
            qteEvent.endSmooth = EditorGUILayout.FloatField("结束模糊时间:", qteEvent.endSmooth);
            qteEvent.offset = EditorGUILayout.Vector3Field("偏移", qteEvent.offset);
        }
    }
}
