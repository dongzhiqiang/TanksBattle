using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteEventFxEditor : QteEventEditor
{
    QteEventFx qteEvent;
    public override void DrawCustom()
    {
        if (qteEvent == null)
            qteEvent = m_event as QteEventFx;

        using (new AutoBeginVertical())
        {
            qteEvent.fxName = EditorGUILayout.TextField("特效名:", qteEvent.fxName);
        }
    }
}
