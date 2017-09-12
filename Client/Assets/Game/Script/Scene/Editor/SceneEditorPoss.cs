using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJson;

public class SceneEditorPoss : SceneEditorBase
{

    public SceneCfg.PossCfg mCheck = new SceneCfg.PossCfg();

    public bool bOpen { get; set; }

    public void Init(SceneCfg.PossCfg check)
    {
        mCheck.name = check.name;
        mCheck.ps.AddRange(check.ps);
    }

    

    public void OnInspectorGUI()
    {
        Transform source =null;
        if (Application.isPlaying && RoleMgr.instance != null && RoleMgr.instance.Hero != null && RoleMgr.instance.Hero.State == Role.enState.alive)
            source = RoleMgr.instance.Hero.transform;

        for (int i=0;i<mCheck.ps.Count;++i)
        {
            using(new AutoBeginHorizontal())
            {
                GUILayout.Label(string.Format("Point {0}", i));
                if (GUILayout.Button("选中"))
                {
                    EditorUtil.SelectScenePos(mCheck.ps[i]);
                }

                //if (GUILayout.Button("贴地表"))
                //{
                //    mCheck.ps[i] = PosUtil.CaleByTerrains(mCheck.ps[i]);
                //}

                if (GUILayout.Button("删除"))
                {
                    mCheck.ps.RemoveAt(i);
                }

                if (source!=null&&GUILayout.Button("插入"))
                {
                    mCheck.ps.Insert(i, source.position);
                }
            }
        }
        
    }

    public override void OnDrawGizmos()
    {
        if (!bOpen || mCheck.ps.Count<1)
            return;

        Color old = Handles.color;
        Handles.color = Color.green;
        Vector3[] poss = new Vector3[mCheck.ps.Count];
        for (int i = 0; i < mCheck.ps.Count; ++i)
        {
            Handles.Label(mCheck.ps[i], string.Format("Point {0}", i));
            mCheck.ps[i] = Handles.PositionHandle(mCheck.ps[i], Quaternion.identity);
            poss[i] = mCheck.ps[i];
        }

        if(mCheck.ps.Count >1)
            Handles.DrawPolyLine(poss);

        Handles.color = old;
        
        
        
    }
}
