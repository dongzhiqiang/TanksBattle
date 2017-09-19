using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class SceneEditorBorn : SceneEditorBase
{

    public string name = "";
    public SceneCfg.BornInfo mBornInfo = new SceneCfg.BornInfo();


    public bool bOpen { get; set; }
    public bool bSceneView { get; set; }
    public string mBornType { get { return mBornInfo.mBornTypeId; } set { mBornInfo.mBornTypeId = value; } }

    public void Init(SceneCfg.BornInfo bornInfo)
    {
        bOpen = true;
        mBornInfo.mBornTypeId = bornInfo.mBornTypeId;
        mBornInfo.mDeadTypeId = bornInfo.mDeadTypeId;
        mBornInfo.mGroundDeadTypeId = bornInfo.mGroundDeadTypeId;
        mBornInfo.mPosition = bornInfo.mPosition;
        mBornInfo.mEulerAngles = bornInfo.mEulerAngles;
        mBornInfo.hate.CopyFrom(bornInfo.hate);
        
    }

    public void OnInspectorGUI ()
    {
        Color color1 = GUI.color;
        GUI.color = Color.green;
        //主角
        using (new AutoBeginHorizontal())
        {
            mBornInfo.mBornTypeId = EditorGUILayout.TextField("出场方式", mBornInfo.mBornTypeId);
            if (GUILayout.Button("选择"))
            {
                List<BornCfg> cfgList = new List<BornCfg>();
                foreach (BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                {
                    if (bc.type == SceneCfg.BornDeadType.Born)
                        cfgList.Add(bc);
                }

                cfgList.Sort((x, y) =>
                {
                    if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                        return 0;
                    int a = (short)(Convert.ToChar(x.typeName[0]));
                    int b = (short)(Convert.ToChar(y.typeName[0]));
                    return a == b ? 0 : ((a > b) ? 1 : -1);
                });

                GenericMenu selectMenu = new GenericMenu();
                for (int i = 0; i < cfgList.Count; i++)
                {
                    int type = i;

                    selectMenu.AddItem(new GUIContent(cfgList[i].typeName), false, () =>
                    {
                        mBornInfo.mBornTypeId = cfgList[type].typeName;
                    });
                }
                selectMenu.ShowAsContext();
            }
        }

        using (new AutoBeginHorizontal())
        {
            mBornInfo.mDeadTypeId = EditorGUILayout.TextField("站立死亡方式", mBornInfo.mDeadTypeId);
            if (GUILayout.Button("选择"))
            {
                List<BornCfg> cfgList = new List<BornCfg>();
                foreach (BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                {
                    if (bc.type == SceneCfg.BornDeadType.Dead)
                        cfgList.Add(bc);
                }

                cfgList.Sort((x, y) =>
                {
                    if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                        return 0;
                    int a = (short)(Convert.ToChar(x.typeName[0]));
                    int b = (short)(Convert.ToChar(y.typeName[0]));
                    return a == b ? 0 : ((a > b) ? 1 : -1);
                });

                GenericMenu selectMenu = new GenericMenu();
                for (int i = 0; i < cfgList.Count; i++)
                {
                    int type = i;

                    selectMenu.AddItem(new GUIContent(cfgList[i].typeName), false, () =>
                    {
                        mBornInfo.mDeadTypeId = cfgList[type].typeName;
                    });
                }
                selectMenu.ShowAsContext();
            }
        }

        using (new AutoBeginHorizontal())
        {
            mBornInfo.mDeadTypeId = EditorGUILayout.TextField("倒地死亡方式", mBornInfo.mDeadTypeId);
            if (GUILayout.Button("选择"))
            {
                List<BornCfg> cfgList = new List<BornCfg>();
                foreach (BornCfg bc in BornCfg.mBornCfg.mBornCfgList)
                {
                    if (bc.type == SceneCfg.BornDeadType.GroundDead)
                        cfgList.Add(bc);
                }

                cfgList.Sort((x, y) =>
                {
                    if (string.IsNullOrEmpty(x.typeName) || string.IsNullOrEmpty(y.typeName))
                        return 0;
                    int a = (short)(Convert.ToChar(x.typeName[0]));
                    int b = (short)(Convert.ToChar(y.typeName[0]));
                    return a == b ? 0 : ((a > b) ? 1 : -1);
                });

                GenericMenu selectMenu = new GenericMenu();
                for (int i = 0; i < cfgList.Count; i++)
                {
                    int type = i;

                    selectMenu.AddItem(new GUIContent(cfgList[i].typeName), false, () =>
                    {
                        mBornInfo.mDeadTypeId = cfgList[type].typeName;
                    });
                }
                selectMenu.ShowAsContext();
            }
        }

        Separator();

        mBornInfo.mPosition = EditorGUILayout.Vector3Field("位置", mBornInfo.mPosition);
        Separator();

        mBornInfo.mEulerAngles = EditorGUILayout.Vector3Field("方向", mBornInfo.mEulerAngles);
        Separator();

        mBornInfo.hate.Draw();
        Separator();
    }

    public void OnSceneGUI() 
    {
        if (mBornInfo.mEulerAngles != Vector3.zero)
        {
            mBornInfo.mPosition = Handles.PositionHandle(mBornInfo.mPosition, Quaternion.LookRotation(mBornInfo.mEulerAngles));
            Handles.Label(mBornInfo.mPosition, new GUIContent(name), EditorStyleEx.LabelStyle);
        }
    }

    public override void OnDrawGizmos()
    {
        if (mBornInfo.mEulerAngles != Vector3.zero && mBornInfo.mPosition != Vector3.zero)
        {

            mBornInfo.mPosition = Handles.PositionHandle(mBornInfo.mPosition, Quaternion.LookRotation(mBornInfo.mEulerAngles));
            Handles.Label(mBornInfo.mPosition, new GUIContent(name), EditorStyleEx.LabelStyle);
        }
    }

    public override void OnDrawGizmosSelected()
    {

    }
}
