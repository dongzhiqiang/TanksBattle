using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteEventGroupEditor : QteEventEditor
{
    QteEventGroup qteEvent;
    public override void DrawCustom()
    {
        if (qteEvent == null)
            qteEvent = m_event as QteEventGroup;

        using (new AutoBeginVertical())
        {
            qteEvent.eventGroupId = EditorGUILayout.TextField("事件组id:", qteEvent.eventGroupId);
        }
    }
}