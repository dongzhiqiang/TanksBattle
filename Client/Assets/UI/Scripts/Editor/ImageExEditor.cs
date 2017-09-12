using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.UI;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(ImageEx), false)]
public class ImageExEditor : ImageEditor
{
    static Sprite s_sprite;

    public override void OnInspectorGUI()
    {
        DrawAtlasSel();
        base.OnInspectorGUI();
    }

    void DrawAtlasSel()
    {
        ImageEx imageEx = target as ImageEx;

        bool grey = EditorGUILayout.Toggle("变灰", imageEx.m_grey);
        if (grey != imageEx.m_grey)
            imageEx.SetGrey(grey);

        string[] ns = UnityEditor.Sprites.Packer.atlasNames;
        if (ns.Length == 0)
            return;
        List<string> l = new List<string>(ns);
        l.Add("baoxiang");
        l.Add("equip");
        l.Add("guanqia");
        l.Add("icon");
        l.Add("item");
        l.Add("pet");
        l.Add("tongyong");
        l.Add("zhucheng");
        ns = l.ToArray();

        //图集选择
        string curAtlas = EditorPrefs.GetString("cur_atlas");
        using (new AutoBeginHorizontal())
        {
            EditorGUILayout.PrefixLabel("图片选择:");

            int i = Array.IndexOf(ns, curAtlas);
            i = EditorGUILayout.Popup(i, ns);
            if (i != -1 && ns[i] != curAtlas)
            {
                EditorPrefs.SetString("cur_atlas", ns[i]);
                curAtlas = ns[i];
            }
            else if(i==-1)
            {
                curAtlas = string.Empty;
            }

            //图片选择
            if (string.IsNullOrEmpty(curAtlas))
                return;
            if (GUILayout.Button("图片", EditorStyles.popup, GUILayout.Width(50)))
            {
                //ImageSelector.Show(curAtlas, OnSel);
                EditorGUIUtility.ShowObjectPicker<Sprite>(imageEx.sprite, false, "ui_"+curAtlas, 0); ;
                UnityEditor.EditorPrefs.SetString("cur_sprite_picker", "imageex");
            }
            if (Event.current.GetTypeForControl(0) == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated" && UnityEditor.EditorPrefs.GetString("cur_sprite_picker") == "imageex")
            {
                Sprite s = EditorGUIUtility.GetObjectPickerObject() as Sprite;
                if (s != null && s != imageEx.sprite)
                {
                    UnityEditor.Undo.RecordObject(imageEx, "imageEx select");
                    imageEx.sprite = s;
                    if (imageEx.type == Image.Type.Simple)
                    {
                        RectTransform t = imageEx.GetComponent<RectTransform>();
                        if (t.sizeDelta.x == 100 && t.sizeDelta.y == 100)//没有设置过大小的情况下设置成图片大小
                        {
                            t.sizeDelta = s.rect.size;
                        }
                    }
                    UnityEditor.EditorUtility.SetDirty(imageEx);
                }

            }
        }        
    }

    public static void ShowSelectImage(Action<string> onSel)
    {
        string[] ns = UnityEditor.Sprites.Packer.atlasNames;
        if (ns.Length == 0)
            return;
        //图集选择
        string curAtlas = EditorPrefs.GetString("cur_atlas");
        using (new AutoBeginHorizontal())
        {
            int i = Array.IndexOf(ns, curAtlas);
            i = EditorGUILayout.Popup(i, ns);
            if (i != -1 && ns[i] != curAtlas)
            {
                EditorPrefs.SetString("cur_atlas", ns[i]);
                curAtlas = ns[i];
            }
            else if (i == -1)
            {
                curAtlas = string.Empty;
            }

            //图片选择
            if (string.IsNullOrEmpty(curAtlas))
                return;
            if (GUILayout.Button("图片", EditorStyles.popup, GUILayout.Width(50)))
            {
                EditorGUIUtility.ShowObjectPicker<Sprite>(s_sprite, false, "ui_" + curAtlas, 0); ;
                UnityEditor.EditorPrefs.SetString("cur_sprite_picker", "SelectImage");
            }
            if (Event.current.GetTypeForControl(0) == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated" && UnityEditor.EditorPrefs.GetString("cur_sprite_picker") == "SelectImage")
            {
                Sprite s = EditorGUIUtility.GetObjectPickerObject() as Sprite;
                if (s != null && s != s_sprite)
                {
                    s_sprite = s;
                    if (onSel!=null)
                        onSel(s_sprite.name);
                }

            }
        }


    }

}

