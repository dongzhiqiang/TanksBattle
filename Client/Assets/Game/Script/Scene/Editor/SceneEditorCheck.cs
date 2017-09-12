using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJson;

public class SceneEditorCheck : SceneEditorBase
{

    public SceneCfg.CheckCfg mCheck;
    public bool bOpen = false;

    bool bOpenAddTrigger = false;
    bool bOpenAddAction = false;
    public bool bSceneShow = true;

    List<EventCfg> mEventList = new List<EventCfg>();
    List<ActionCfg> mActionList = new List<ActionCfg>();
    public string checkFlag { get { return mCheck.checkFlag; } set { mCheck.checkFlag = value; } }

    public override void Init()
    {
        mCheck = new SceneCfg.CheckCfg();
        mEventList.Clear();
        mActionList.Clear();
    }

    public void Init(SceneCfg.CheckCfg check)
    {
        mCheck = check;

        for (int i = 0; i < mCheck.eventCfgList.Count; i++)
        {
            EventCfg cfg = EventCfgFactory.instance.GetEventCfg((SceneCfg.EventType)mCheck.eventCfgList[i].eveType, mCheck.eventCfgList[i].content);
            mEventList.Add(cfg);
        }

        for (int i = 0; i < mCheck.actionCfgList.Count; i++)
        {
            ActionCfg cfg = ActionCfgFactory.instance.GetActionCfg((SceneCfg.ActionType)mCheck.actionCfgList[i].eveType, mCheck.actionCfgList[i].content);
            mActionList.Add(cfg);
        }
    }


    public override string Save()
    {
        string err = base.Save();
        mCheck.actionCfgList.Clear();
        mCheck.eventCfgList.Clear();

        for (int i = 0; i < mActionList.Count; i++)
        {

            if (mActionList[i].mType == SceneCfg.ActionType.Story)
            {
                ActionCfg_Story cfg = mActionList[i] as ActionCfg_Story;
                if (!SceneMgr.instance.SceneData.mStoryIdList.Contains(cfg.storyId))
                {
                    SceneMgr.instance.SceneData.mStoryIdList.Add(cfg.storyId);
                    StorySaveCfg.GetCfg(cfg.storyId);
                }
            }

            SceneCfg.CheckSaveCfg saveCfg = new SceneCfg.CheckSaveCfg();
            saveCfg.eveType = (int)mActionList[i].mType;
            saveCfg.content = JsonMapper.ToJson(mActionList[i]);
            mCheck.actionCfgList.Add(saveCfg);
        }

        for (int i = 0; i < mEventList.Count; i++)
        {
            SceneCfg.CheckSaveCfg saveCfg = new SceneCfg.CheckSaveCfg();
            saveCfg.eveType = (int)mEventList[i].mType;
            saveCfg.content = JsonMapper.ToJson(mEventList[i]);
            mCheck.eventCfgList.Add(saveCfg);
        }

        if (mCheck.triggerNum == 0)
            mCheck.triggerNum = mCheck.eventCfgList.Count;

        return err;
    }

    public void OnInspectorGUI()
    {
        Color c2 = GUI.color;
        GUI.color = Color.cyan;
        mCheck.triggerType = (SceneCfg.TriggerType)EditorGUILayout.Popup("触发类型", (int)mCheck.triggerType, SceneCfg.TriggerTypeName);
        if (mCheck.triggerType == SceneCfg.TriggerType.Random)
        {
            mCheck.triggerNum = EditorGUILayout.IntField("", mCheck.triggerNum);
        }
        else
        {
            mCheck.triggerNum = mCheck.eventCfgList.Count;
        }

        mCheck.triggerTimes = EditorGUILayout.IntField("触发次数", mCheck.triggerTimes);

        for (int i = 0; i < mEventList.Count; i++)
        {

            using (new AutoBeginHorizontal())
            {
                Color c = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label(mEventList[i].GetTypeDesc());
                if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                {
                    mEventList.Remove(mEventList[i]);
                    return;
                }
                GUI.color = c;
            }
            mEventList[i].OnShow();
            Separator();

        }

        Color c3 = GUI.color;
        GUI.color = Color.red;
        Separator();
        Separator();
        GUI.color = c3;

        for (int i = 0; i < mActionList.Count; i++)
        {

            using (new AutoBeginHorizontal())
            {
                Color c = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label(mActionList[i].GetTypeDesc());
                if (GUILayout.Button("Delete", EditorStyleEx.GraphDeleteButtonStyle))
                {
                    mActionList.Remove(mActionList[i]);
                    return;
                }
                GUI.color = c;
            }
            
             mActionList[i].OnShow();
            
            Separator();

        }

        using (new AutoBeginHorizontal())
        {

            Color color1 = GUI.color;
            GUI.color = Color.green;

            if (GUILayout.Button("AddTrigger"))
            {
                GenericMenu eventMenu = new GenericMenu();

                for (int i = 0; i < SceneEditor.mEveTypeDesc.Length; ++i)
                {
                    int type = i;
                    eventMenu.AddItem(new GUIContent(SceneEditor.mEveTypeDesc[i]), false, () =>
                    {
                        mEventList.Add(EventCfgFactory.instance.GetEventCfg((SceneCfg.EventType)type));
                    });
                }
                eventMenu.ShowAsContext();
            }

            if (GUILayout.Button("AddAction"))
            {
                GenericMenu actionMenu = new GenericMenu();

                for (int i = 0; i < SceneEditor.mActTypeDesc.Length; ++i)
                {
                    int type = i;
                    actionMenu.AddItem(new GUIContent(SceneEditor.mActTypeDesc[i]), false, () =>
                    {
                        mActionList.Add(ActionCfgFactory.instance.GetActionCfg((SceneCfg.ActionType)type));
                    });
                }
                actionMenu.ShowAsContext();
            }


            GUI.color = color1;
        }
        GUI.color = c2;
    }

    public void OnSceneGUI()
    {
        if (bSceneShow)
        {
        }

    }

    public void SortAction()
    {
        mActionList.Sort((ActionCfg x, ActionCfg y) =>
        {
            return x._idx == y._idx ? 0 : (x._idx > y._idx ? 1 : -1);
        });
    }

    public override void OnDrawGizmos()
    {
        if (bSceneShow)
        {
        }
    }
}
