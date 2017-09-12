using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SceneEditorBase
{

    public bool m_bShow;
    public SceneEditorBase()
    {
        m_bShow = false;
    }

    public virtual void Init() { }

    public virtual void Show()
    {
    }

    public virtual void Load()
    {
    }

    public virtual string Save()
    {
        return "";
    }

    public void Separator()
    {
        GUIStyle separator = EditorStyleEx.PixelBox3SeparatorStyle;
        if (separator == null)
        {
            separator = new GUIStyle();
        }

        Rect r = GUILayoutUtility.GetRect(new GUIContent(), separator);

        if (Event.current.type == EventType.Repaint)
        {
            separator.Draw(r, false, false, false, false);
        }
    }
    public virtual void OnDrawGizmos()
    {
    }
    public virtual void OnDrawGizmosSelected()
    {
    }

}
