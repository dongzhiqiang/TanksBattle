using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteEventOperateEditor : QteEventEditor
{
    QteEventOperate qteEvent;
    bool bReach = false;
    public override void DrawCustom()
    {
        if (qteEvent == null)
            qteEvent = m_event as QteEventOperate;
        using (new AutoBeginVertical())
        {
            qteEvent.stageType = (enQteStateType)EditorGUILayout.Popup("操作类型", (int)qteEvent.stageType, QteEvent.OperateName);
            bReach = EditorGUILayout.Toggle("达成：", bReach);
            if (bReach)
                qteEvent.IsReach = true;
        }
    }
}
