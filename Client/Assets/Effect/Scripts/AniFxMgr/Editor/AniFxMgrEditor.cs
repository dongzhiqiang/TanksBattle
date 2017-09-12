using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AniFxMgr), false)]
public class AniFxMgrEditor : Editor
{
    List<float> m_percents= new List<float>(){0.1f,0.2f,0.3f,0.4f};
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        AniFxMgr aniFxMgr = target as AniFxMgr;
        Animation ani=aniFxMgr.GetComponent<Animation>();
        if(ani== null){
            EditorGUILayout.LabelField("找不到Animation，不能添加动作绑定的特效");
            return;
        }
        string[] aniNames = ani.GetNames();    
        if(aniNames== null || aniNames.Length == 0){
            EditorGUILayout.LabelField("Animation没有动作");
            return;
        }
        if(!string.IsNullOrEmpty(aniFxMgr.m_search))
        {
            List<string> l = new List<string>();
            foreach (var aniName in aniNames)
            {
                if (aniName.Contains(aniFxMgr.m_search))
                    l.Add(aniName);
            }
            aniNames = l.ToArray();
        }

        EditorGUI.BeginChangeCheck();
        using (new AutoBeginHorizontal())
        {
            aniFxMgr.m_search = EditorGUILayout.TextField("筛选", aniFxMgr.m_search, "ToolbarSeachTextField", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("", string.IsNullOrEmpty(aniFxMgr.m_search) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton"))
                aniFxMgr.m_search = "";
        }
        //测试元素属性
        aniFxMgr.m_testElement = (enAniFxElement)EditorGUILayout.Popup("测试元素属性", (int)aniFxMgr.m_testElement, AniFxMgr.Element_Names);
        
        List<AniFxGroup> removes = new List<AniFxGroup>();
        //绘制
        foreach (AniFxGroup g in aniFxMgr.m_groups)
        {
            if (!string.IsNullOrEmpty(aniFxMgr.m_search) && !g.name.Contains(aniFxMgr.m_search))
                continue;

            if (DrawGroup(g,aniFxMgr,ani,aniNames))
                removes.Add(g);
        }
        //删除
        foreach(AniFxGroup g in removes){
            aniFxMgr.m_groups.Remove(g);
        }
        
        //添加
        int idx = UnityEditor.EditorGUILayout.Popup("添加", -1, aniNames);
        if (idx != -1)
        {
            if (aniFxMgr.GetGroup(aniNames[idx]) == null)
            {
                AniFxGroup g = new AniFxGroup();
                g.name = aniNames[idx];
                aniFxMgr.m_groups.Add(g);
            }
            else
            {
                Debuger.LogError("{0}已经添加过了，不能重复添加", aniNames[idx]);
            }
        }
            

        if (EditorGUI.EndChangeCheck())
        {
            //Debuger.Log("修改");
            EditorUtil.SetDirty(aniFxMgr);
        }
    }
    public void OnSceneGUI()
    {
        AniFxMgr aniFxMgr = target as AniFxMgr;
        Transform root = aniFxMgr.transform.parent;
        
        foreach (AniFxGroup g in aniFxMgr.m_groups)
        {
            foreach (AniFx fx in g.fxs)
            {
                if (!fx.IsDrawGizmos) continue;


                if (fx.type == AniFx.enCreateType.bone)//绑定骨骼
                {
                    Transform t = fx.GetTarget(root);
                    

                    Vector3 worldPosOld = t.TransformPoint(fx.offset);
                    Handles.Label(worldPosOld, fx.prefab == null ? "" : fx.prefab.name);
                    Vector3 worldPosNew = Handles.PositionHandle(worldPosOld,t.rotation * Quaternion.Euler(fx.euler));
                    if (worldPosNew != worldPosOld)
                    {
                        fx.offset = t.InverseTransformPoint(worldPosNew);
                        EditorUtility.SetDirty (target);
                    }
                }   
            }
            
		}
        

    }

    bool DrawGroup(AniFxGroup g, AniFxMgr aniFxMgr, Animation ani, string[] aniNames)
    {
        //绘制
        bool isShow;
        bool isClick;
        int idxOld = Array.IndexOf(aniNames, g.name);
        if(idxOld==-1){
            using(new AutoChangeColor(Color.red))
                EditorUtil.DrawHeaderBtn(g.name+"(动作找不到)", "删除", out isShow, out isClick);
        }
        else
        {
            AnimationState st = ani[g.name];
            int maxFrame = (int)(st.length / Util.One_Frame);
            EditorUtil.DrawHeaderBtn(string.Format("{0}(共{1}帧)", g.name, maxFrame), "删除", out isShow, out isClick);
        }
            

        
        List<AniFx> removes = new List<AniFx>();
        if (isShow)
        {
            using(new AutoContent()){
                
                foreach (AniFx fx in g.fxs)
                {
                    if (DrawFx(fx, g, aniFxMgr, ani, aniNames))
                        removes.Add(fx);
                }
                //添加
                if (GUILayout.Button("添加"))
                {
                    AniFx fx = new AniFx();
                    fx.follow = true;
                    fx.destroyIfAniEnd = true;
                    fx.endFrame = -1;
                    g.fxs.Add(fx);
                }
#if ART_DEBUG
                if (Application.isPlaying && GUILayout.Button("播放"))
                {
                    SimpleRole.AttackCxt atk = new SimpleRole.AttackCxt();
                    atk.aniName = g.name;
                    aniFxMgr.transform.parent.GetComponent<SimpleRole>().GotoState(SimpleRole.enState.attack, atk);
                }
#endif
            }
        }
        else
        {
            foreach(AniFx fx in g.fxs){
                if (fx.IsDrawGizmos)
                    fx.IsDrawGizmos = false;
            }
        }
       
       
        //删除
        foreach (AniFx fx in removes)
        {
            g.fxs.Remove(fx);
        }

        return isClick;
    }

    bool DrawFx(AniFx fx,AniFxGroup g, AniFxMgr aniFxMgr, Animation ani, string[] aniNames)
    {
        using(new AutoEditorIndentLevel(2))
        {
            using (new AutoBeginHorizontal())
            {
                fx.IsDrawGizmos = EditorGUILayout.Foldout(fx.IsDrawGizmos, fx.prefab == null ? "空" : fx.prefab.name);
                if (GUILayout.Button("删除", GUILayout.Width(45)))
                    return true;
            }
            if (fx.IsDrawGizmos)
            {
                using(new AutoEditorIndentLevel())
                {
                    fx.canHide = EditorGUILayout.Toggle("可以隐藏", fx.canHide);
                    fx.prefab = (GameObject)EditorGUILayout.ObjectField("特效", fx.prefab, typeof(GameObject), false);
                    fx.prefabFire = (GameObject)EditorGUILayout.ObjectField("特效(火)", fx.prefabFire, typeof(GameObject), false);
                    fx.prefabIce = (GameObject)EditorGUILayout.ObjectField("特效(冰)", fx.prefabIce, typeof(GameObject), false);
                    fx.prefabThunder = (GameObject)EditorGUILayout.ObjectField("特效(雷)", fx.prefabThunder, typeof(GameObject), false);
                    fx.prefabDark = (GameObject)EditorGUILayout.ObjectField("特效(冥)", fx.prefabDark, typeof(GameObject), false);
                    fx.type = (AniFx.enCreateType)EditorGUILayout.Popup("类型", (int)fx.type, AniFx.TypeName);
                    if (fx.type== AniFx.enCreateType.bone)
                    {
                        fx.bone = (Transform)EditorGUILayout.ObjectField("骨骼", fx.bone, typeof(Transform));
                        fx.follow = EditorGUILayout.Toggle("跟随", fx.follow);
                        fx.offset =EditorGUILayout.Vector3Field("位移", fx.offset);
                        fx.euler = EditorGUILayout.Vector3Field("角度", fx.euler);
                    }

                    fx.destroyIfAniEnd = EditorGUILayout.Toggle("动作结束销毁", fx.destroyIfAniEnd);
                    fx.beginFrame = EditorGUILayout.IntField("开始帧", fx.beginFrame);
                    if (!fx.destroyIfAniEnd)
                        fx.endFrame = EditorGUILayout.IntField("结束帧数", fx.endFrame);
                    else if (fx.endFrame!=-1)
                        fx.endFrame = -1;

                    fx.loopCreate = EditorGUILayout.Toggle("循环创建多次", fx.loopCreate);
                }
            }
        }
        
      

        return false;
    }

    private readonly int m_LODSliderId = "slider".GetHashCode();
    
    private int m_SelectedLODSlider = -1;
    private static float CalculatePercentageFromBar(Rect totalRect, Vector2 clickPosition)
    {
        clickPosition.x -= totalRect.x;
        totalRect.x = 0f;
        return (totalRect.width <= 0f) ? 0f : (1f - clickPosition.x / totalRect.width);
    }
   
}

