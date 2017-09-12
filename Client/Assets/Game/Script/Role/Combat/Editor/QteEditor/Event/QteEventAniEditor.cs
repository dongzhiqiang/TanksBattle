using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteEventAniEditor : QteEventEditor
{
    QteEventAni qteEvent;
    public override void DrawCustom()
    {
        if (qteEvent == null)
            qteEvent = m_event as QteEventAni;
        
        using (new AutoBeginVertical())
        {
            qteEvent.m_objType = (enQteEventObjType)EditorGUILayout.Popup("对象", (int)qteEvent.m_objType, QteEventAni.ObjTypeName);
            qteEvent.aniName = EditorGUILayout.TextField("动作:", qteEvent.aniName);
            qteEvent.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("循环:", qteEvent.wrapMode);
        }
    }
}
