using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandleImage : IHandle
{
    //不在运行中时，Handle的类型或者m_go改变的时候会刷新值
    public override void OnReset(Handle h, bool resetBegin = true, bool resetEnd = false)
    {
        if (h.m_go)
        {
            Image image=h.m_go.GetComponent<Image>();
            if (image != null)
            {
                h.m_sprite1 = image.sprite;
            }
        }
    }

    public override void OnEnd(Handle h)
    {
        if (h.m_go == null  )
            return;

        Image image = h.m_go.GetComponent<Image>();
        if (image == null || image.sprite == h.m_sprite1)
            return;
        image.sprite = h.m_sprite1;
    }
#if UNITY_EDITOR
    public override bool OnDrawGo(Component comp, Handle h, string title = null)
    {
        return DrawGoField<Image>(comp, h, title);
    }

    public void SelectSprite(Component comp, Handle h)
    {
        
        string[] ns = UnityEditor.Sprites.Packer.atlasNames;
        if (ns.Length == 0)
            return;
        string curAtlas = UnityEditor.EditorPrefs.GetString("cur_atlas");
        using (new AutoBeginHorizontal())
        {
            int i = System.Array.IndexOf(ns, curAtlas);
            i = UnityEditor.EditorGUILayout.Popup(i, ns);
            if (i != -1 && ns[i] != curAtlas)
            {
                UnityEditor.EditorPrefs.SetString("cur_atlas", ns[i]);
                curAtlas = ns[i];
            }
            else if (i == -1)
            {
                curAtlas = string.Empty;
            }

            //图片选择
            if (string.IsNullOrEmpty(curAtlas))
                return;
            if (GUILayout.Button("图片", GUILayout.Width(35)))
            {
                //ImageSelector.Show(curAtlas, OnSel);
                UnityEditor.EditorGUIUtility.ShowObjectPicker<Sprite>(h.m_sprite1, false, "ui_" + curAtlas, 1); ;
                UnityEditor.EditorPrefs.SetString("cur_sprite_picker", "handle_image_" + h.GetHashCode());
            }
            if (Event.current.GetTypeForControl(1) == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated" && UnityEditor.EditorPrefs.GetString("cur_sprite_picker") == ("handle_image_" + h.GetHashCode()))
            {
                Sprite s = UnityEditor.EditorGUIUtility.GetObjectPickerObject() as Sprite;
                if (s != null && s != h.m_sprite1)
                {
                    h.m_sprite1 = s;
                    End(h);
                    EditorUtil.SetDirty(h.m_go);
                }

            }
        }
    }

    //框架，绘制属性(不包含游戏对象),syncGo的话结束值变化会同步到m_go
    public override void OnDraw(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        GUI.changed = false;
        Sprite s;
        using (new AutoBeginHorizontal())
        {
            s = (Sprite)UnityEditor.EditorGUILayout.ObjectField("图片", h.m_sprite1, typeof(Sprite), true, GUILayout.Height(16f));
            SelectSprite(comp, h);
            //这里加一个可以同步的按钮
            if (h.m_go && GUILayout.Button("同步", GUILayout.Width(35)))
            {
                Image image = h.m_go.GetComponent<Image>();
                if (image != null)
                {
                    h.m_sprite1 = image.sprite;
                }
                EditorUtil.SetDirty(h.m_go);
            }
        }


        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_sprite1 = s;
            EditorUtil.SetDirty(comp);
            if (syncGo && h.m_go)
            {
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }
        }
        DrawCommonImmediate(comp, h);
    }

    
    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMin(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        GUI.changed = false;
        Sprite s;
        using (new AutoBeginHorizontal())
        {
            s = (Sprite)UnityEditor.EditorGUILayout.ObjectField("", h.m_sprite1, typeof(Sprite), true, GUILayout.Height(16f));
            SelectSprite(comp, h);
        }

        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            h.m_sprite1 = s;
            EditorUtil.SetDirty(comp);
            if (syncGo && h.m_go)
            {
                End(h);
                EditorUtil.SetDirty(h.m_go);
            }
        }
    }

    //框架，绘制属性(不包含游戏对象)，syncGo的话结束值变化会同步到m_go。这里是最小化绘制，只绘制最需要的属性
    public override void OnDrawMid(Component comp, Handle h, System.Action<Handle.WndType, object> onOpenWnd, bool syncGo = false)
    {
        OnDrawMin(comp,h,onOpenWnd,syncGo);
    }
#endif
}
