using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
public class SceneEditorArea : SceneEditorBase
{
    public SceneCfg.AreaCfg mArea;
    public bool bOpen = false;

    public bool bSceneShow = true;
    public string areaFlag { get { return mArea.areaFlag; } set { mArea.areaFlag = value; } }

    GameObject goBox = null;
    public void Init(SceneCfg.AreaType areaType)
    {
        mArea = new SceneCfg.AreaCfg();
        mArea.areaType = areaType;
        
        if (string.IsNullOrEmpty(mArea.areaFlag))
        {
            if(mArea.areaType == SceneCfg.AreaType.Normal)
                mArea.areaFlag = string.Format("Area{0}", SceneEditor.mAreaViewList.Count + 1);
            else if (mArea.areaType == SceneCfg.AreaType.DangBan)
                mArea.areaFlag = string.Format("Dangban{0}", SceneEditor.mAreaViewList.Count + 1);
        }

        if (RoleMgr.instance.Hero != null && RoleMgr.instance.Hero.transform != null)
        {
            mArea.pos = RoleMgr.instance.Hero.transform.position;
            mArea.dir = RoleMgr.instance.Hero.transform.eulerAngles;
        }
        else
        {
            mArea.pos = Vector3.zero;
            mArea.dir = Vector3.zero;
        }

        if (mArea.areaType == SceneCfg.AreaType.Normal)
        {
            goBox = GetObjectBox();
            SceneMgr.instance.mDangbanDict[mArea.areaFlag] = goBox;
        }
    }

    public void Init(SceneCfg.AreaCfg area)
    {
        mArea = new SceneCfg.AreaCfg();


        if (area.areaType == SceneCfg.AreaType.Normal)
        {

            AreaTrigger areaTrigger = SceneEventMgr.instance.GetAreaTriggerByFlag(area.areaFlag);
            if (areaTrigger != null)
            {
                goBox = areaTrigger.gameObject;
            }
        }
        else if (area.areaType == SceneCfg.AreaType.DangBan)
        {

            GameObject go = SceneMgr.instance.GetDangban(area.areaFlag);
            if (go != null)
            {
                goBox = go.gameObject;
            }
        }

        mArea.Init(area.areaFlag, goBox.transform.position, goBox.transform.rotation.eulerAngles, goBox.transform.localScale, area.dangbanType, area.areaType, area.bActivate);
        mArea.areaType = area.areaType;
    }

    public void Copy(SceneCfg.AreaCfg area)
    {

        mArea = new SceneCfg.AreaCfg();
        mArea.areaType = area.areaType;
        string flag = string.Format("Area{0}", SceneEditor.mAreaViewList.Count + 1);
        if (mArea.areaType == SceneCfg.AreaType.DangBan)
            flag = string.Format("Dangban{0}", SceneEditor.mAreaViewList.Count + 1);

        mArea.Init(flag, area.pos, area.dir, area.size, area.dangbanType, SceneCfg.AreaType.Normal, area.bActivate);

        if (mArea.areaType == SceneCfg.AreaType.Normal)
        {
            goBox = GetObjectBox();
            SceneMgr.instance.mDangbanDict[flag] = goBox;
        }

    }

    GameObject GetObjectBox()
    {
        GameObject go = null;

        if (mArea.areaType == SceneCfg.AreaType.DangBan)
        {
            go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(SceneCfg.DangbanName[(int)mArea.dangbanType], false);
            go.gameObject.SetActive(true);
        }
        else if (mArea.areaType == SceneCfg.AreaType.Normal)
        {
            go = new GameObject(mArea.areaFlag, typeof(BoxCollider));
        }

        go.transform.SetParent(Room.instance.mAreaGroup.transform, false);

        go.transform.position = mArea.pos;
        go.transform.localScale = mArea.size;
        go.transform.rotation = Quaternion.Euler(mArea.dir);

        BoxCollider collider = go.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.enabled = true;

        return go;
    }

    public override string Save()
    {
        string err = base.Save();
        if (goBox == null)
            return err += "无挡板";
        mArea.pos = goBox.transform.position;
        mArea.size = goBox.transform.localScale;
        mArea.dir = goBox.transform.rotation.eulerAngles;
        if (mArea.areaFlag == "")
            err += "有区域标记为空";
        return err;
    }

    public void OnInspectorGUI()
    {
        mArea.bActivate = EditorGUILayout.Toggle("初始激活", mArea.bActivate);

        mArea.dir = EditorGUILayout.Vector3Field("方向", mArea.dir);
        mArea.size = EditorGUILayout.Vector3Field("大小", mArea.size);
        mArea.pos = EditorGUILayout.Vector3Field("位置", mArea.pos);

        GUILayout.Space(2);
        if (mArea.areaType == SceneCfg.AreaType.DangBan)
        {
            mArea.dangbanType = (SceneCfg.DangbanType)EditorGUILayout.Popup("挡板类型", (int)mArea.dangbanType, SceneCfg.DangbanName);

            if (mArea.dangbanType != SceneCfg.DangbanType.None && goBox == null)
            {
                goBox = GetObjectBox();
                SceneMgr.instance.mDangbanDict[mArea.areaFlag] = goBox;
            }
        }

        GUILayout.Space(2);    
        mArea.pos = EditorGUILayout.Vector3Field("位置", mArea.pos);

        if (goBox == null)
            return;

        goBox.transform.position = mArea.pos;
        goBox.transform.localScale = mArea.size;
        goBox.transform.rotation = Quaternion.Euler(mArea.dir);

        if (GUILayout.Button("移动到主角位置"))
        {
            Role hero = RoleMgr.instance.Hero;
            if (hero == null || hero.State != Role.enState.alive)
            {
                Debug.Log("先创建主角");
                return;
            }
            else
            {
                mArea.pos = hero.transform.position;
                goBox.transform.position = mArea.pos;
            }
        }

        if (GUILayout.Button("Copy", EditorStyleEx.GraphBoxStyle, GUILayout.Width(40)))
        {
            SceneEditorArea areaView = new SceneEditorArea();
            areaView.Copy(mArea);
            SceneEditor.mAreaViewList.Add(areaView);
            return;
        }
        
    }

    public override void OnDrawGizmos()
    {
        if (bSceneShow && goBox != null)
        {
            Handles.Label(goBox.transform.position, new GUIContent(mArea.areaFlag), EditorStyleEx.LabelStyle);


            mArea.pos = goBox.transform.position;
            mArea.size = goBox.transform.localScale;
            mArea.dir = goBox.transform.rotation.eulerAngles;
        }
    }
}
