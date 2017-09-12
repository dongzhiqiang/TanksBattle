using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public delegate void DrawRemoveBtn(SceneCfg.CheckCfg check, CheckCfgBase actionCfg);

[JsonCanInherit]

public class CheckCfgBase
{
#if UNITY_EDITOR
    public static string AreaFlagDesc = "区域ID";
    public static string RoleFlagDesc = "标记ID(主角空)";
    public static string RefreshFlagDesc = "刷新点ID";
    public static string DangbanDesc = "挡板ID";
    public static string FindPosition = "寻路点";
    public static string StoryDesc = "剧情ID";
    public static string SceneFileDesc = "配置文件";
    public static string CameraFlagDesc = "镜头名";
    public static string EventFlagDesc = "事件id";
#endif

    protected string TypeDesc { get; set; }
    protected string[] ParamDesc { get; set; }



    public CheckCfgBase()
    {

    }

#if UNITY_EDITOR
    public void OnShow()
    {
        
        FieldInfo[] fields = this.GetType().GetFields();

        object fieldValue;
        for (int i = 0; i < fields.Length; i++)
        {
            fieldValue = fields[i].GetValue(this);

            if (fields[i].Name == "_idx")   //所有action都有个触发顺序的记录
            {
                int value = EditorGUILayout.IntField("触发顺序", (int)fieldValue);
                fields[i].SetValue(this, value);
            }
            else if (fields[i].Name == "_delay")
            {
                float value = EditorGUILayout.FloatField("延时时间", (float)fieldValue);
                fields[i].SetValue(this, value);
            }
            else if (fields[i].FieldType == typeof(int))
            {
                int value = EditorGUILayout.IntField(ParamDesc[i], (int)fieldValue);
                fields[i].SetValue(this, value);
            }
            else if (fields[i].FieldType == typeof(float))
            {
                float value = EditorGUILayout.FloatField(ParamDesc[i], (float)fieldValue);
                fields[i].SetValue(this, value);
            }
            else if (fields[i].FieldType == typeof(string))
            {
                GetValueByType(ParamDesc[i], (string)fieldValue, fields[i]);
            }
            else if (fields[i].FieldType == typeof(bool))
            {
                bool value = EditorGUILayout.Toggle(ParamDesc[i], (bool)fieldValue);
                fields[i].SetValue(this, value);
            }
            else if (fields[i].FieldType == typeof(Vector3))
            {
                Vector3 value = EditorGUILayout.Vector3Field(ParamDesc[i], (Vector3)fieldValue);
                fields[i].SetValue(this, value);

                if (ParamDesc[i] == FindPosition)
                {
                    if (GUILayout.Button("获取"))
                    {
                        if (RoleMgr.instance.Hero != null)
                        {
                            fields[i].SetValue(this, RoleMgr.instance.Hero.TranPart.GetRoot());
                        }
                    }
                }
            }
        }

        OnDraw();
    }

    public void GetValueByType(string desc, string fieldValue, FieldInfo fieldInfo)
    {
        SceneCfg.SceneData sceneData = SceneMgr.instance.SceneData;

        using (new AutoBeginHorizontal())
        {

            fieldValue = EditorGUILayout.TextField(desc, fieldValue);

            if (desc == AreaFlagDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mAreaList.Count; i++)
                    {

                        if (sceneData.mAreaList[i].areaType == SceneCfg.AreaType.Normal)
                        {
                            int type = i;

                            selectMenu.AddItem(new GUIContent(sceneData.mAreaList[i].areaFlag), false, () =>
                            {
                                fieldInfo.SetValue(this, sceneData.mAreaList[type].areaFlag);
                            });
                        }
                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == RoleFlagDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mRefGroupList.Count; i++)
                    {
                        int type1 = i;
                        for (int j = 0; j < sceneData.mRefGroupList[i].Points.Count; j++)
                        {
                            int type2 = j;
                            SceneCfg.RefPointCfg refPoint = sceneData.mRefGroupList[i].Points[j];
                            selectMenu.AddItem(new GUIContent(refPoint.pointFlag), false, () =>
                            {
                                fieldInfo.SetValue(this, sceneData.mRefGroupList[type1].Points[type2].pointFlag);
                            });
                        }

                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == RefreshFlagDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mRefGroupList.Count; i++)
                    {
                        int type1 = i;
                        selectMenu.AddItem(new GUIContent(sceneData.mRefGroupList[i].groupFlag), false, () =>
                        {
                            fieldInfo.SetValue(this, sceneData.mRefGroupList[type1].groupFlag);
                        });
                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == DangbanDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mAreaList.Count; i++)
                    {
                        if (sceneData.mAreaList[i].areaType == SceneCfg.AreaType.DangBan)
                        {
                            int type = i;
                            selectMenu.AddItem(new GUIContent(sceneData.mAreaList[i].areaFlag), false, () =>
                            {
                                fieldInfo.SetValue(this, sceneData.mAreaList[type].areaFlag);
                            });
                        }
                        
                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == SceneFileDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    List<string> fileList = LevelMgr.instance.CurLevel.roomCfg.sceneFileName;
                    for (int i = 0; i < fileList.Count;i++)
                    {
                        int type = i;
                        selectMenu.AddItem(new GUIContent(fileList[i]), false, () =>
                        {
                            fieldInfo.SetValue(this, fileList[type]);
                        });
                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == CameraFlagDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mCameraList.Count; i++)
                    {
                        int type = i;
                        selectMenu.AddItem(new GUIContent(sceneData.mCameraList[i].cameraFlag), false, () =>
                        {
                            fieldInfo.SetValue(this, sceneData.mCameraList[type].cameraFlag);
                        });

                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == AreaFlagDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mAreaList.Count; i++)
                    {
                        if (sceneData.mAreaList[i].areaType == SceneCfg.AreaType.Normal)
                        {
                            int type = i;
                            selectMenu.AddItem(new GUIContent(sceneData.mAreaList[i].areaFlag), false, () =>
                            {
                                fieldInfo.SetValue(this, sceneData.mAreaList[type].areaFlag);
                            });
                        }

                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else if (desc == EventFlagDesc)
            {
                if (GUILayout.Button("选择"))
                {
                    GenericMenu selectMenu = new GenericMenu();
                    for (int i = 0; i < sceneData.mCheckList.Count; i++)
                    {
                        int type = i;
                        selectMenu.AddItem(new GUIContent(sceneData.mCheckList[i].checkFlag), false, () =>
                        {
                            fieldInfo.SetValue(this, sceneData.mCheckList[type].checkFlag);
                        });

                    }
                    selectMenu.ShowAsContext();
                    return;
                }
            }
            else
            {
                fieldInfo.SetValue(this, fieldValue);
            }
        }

    }
    
    public virtual void OnDraw() {  }
#endif
    public string GetTypeDesc()
    {
        return TypeDesc;
    }

    public string[] GetParamDesc()
    {
        return ParamDesc;
    }

    

}
