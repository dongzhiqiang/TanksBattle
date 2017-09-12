using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class AutoFontSize: IDisposable
{
    GUIStyle[] m_ps ;
    int[] m_sizes;
    
//#if UNITY_EDITOR
//    int oldEditorLabelFontSize;
//    int oldEditorButtonFontSize;
//    int oldEditorToggleFontSize;
//    int oldEditorTextFieldFontSize;
//#endif

    public AutoFontSize(float size,params GUIStyle[] ps)
    {
        if (ps == null || ps.Length == 0)
        {
            m_ps = new GUIStyle[]{GUI.skin.label,GUI.skin.button,GUI.skin.toggle};
        }
        else
        {
            m_ps = new GUIStyle[ps.Length];
            Array.Copy(ps, this.m_ps, ps.Length);
        }
            
        m_sizes = new int[m_ps.Length];
        for(int i=0;i<m_ps.Length;++i){
            m_sizes[i] =m_ps[i].fontSize;
            m_ps[i].fontSize=(int)size;
        }
        
//#if UNITY_EDITOR
//        GUISkin editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
//        oldEditorLabelFontSize = editorSkin.label.fontSize;
//        oldEditorLabelFontSize = editorSkin.button.fontSize;
//        oldEditorLabelFontSize = editorSkin.toggle.fontSize;
//        oldEditorLabelFontSize = editorSkin.textField.fontSize;
//        editorSkin.label.fontSize = (int)size;
//        editorSkin.button.fontSize = (int)size;
//        editorSkin.toggle.fontSize = (int)size;
//        editorSkin.textArea.fontSize = (int)size;
//        editorSkin.textField.fontSize = (int)size;

//#endif
    }



    public void Dispose()
    {
        for (int i = 0; i < m_ps.Length; ++i)
        {
            m_ps[i].fontSize = m_sizes[i];
        }
//#if UNITY_EDITOR
//        GUISkin editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
//        editorSkin.label.fontSize = oldEditorLabelFontSize;
//        editorSkin.button.fontSize = oldEditorButtonFontSize;
//        editorSkin.toggle.fontSize = oldEditorToggleFontSize;
//        editorSkin.textArea.fontSize = oldEditorTextFieldFontSize;
//        editorSkin.textField.fontSize = oldEditorTextFieldFontSize;
//#endif
    }
}
