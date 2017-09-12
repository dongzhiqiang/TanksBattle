using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//主城场景
public class IntroductionScene : LevelBase
{
    public const string ROOM_ID = "xuzhang01";
    Material m_preLoadMat;

    public override void OnLoadFinish()
    {
        UILevel uiLevel = UIMgr.instance.Open<UILevel>();
        uiLevel.Open<UILevelAreaReward>().ResetUI();
        uiLevel.Close<UILevelAreaSetting>();
        uiLevel.Open<UILevelAreaGizmos>();

        //找出等下序章材质球存着，等下切换关卡就不会被卸载了
        GameObject go =GameObject.Find("MapScene/[Building]/001diyimu/shitou/XuZhang_shitou_001 (1)");
        if (go != null)
            m_preLoadMat = go.GetComponent<Renderer>().sharedMaterial;
        else
            Debuger.LogError("不能预加载场景的材质：MapScene/[Building]/001diyimu/shitou/XuZhang_shitou_001 (1)");
            
    }

    public override void OnEnterAgain()
    {
        UILevel uiLevel = UIMgr.instance.Open<UILevel>();
        uiLevel.Open<UILevelAreaReward>().ResetUI();
        uiLevel.Close<UILevelAreaSetting>();
        uiLevel.Open<UILevelAreaGizmos>();
    }

    public override void OnExit()
    {
        var hero = RoleMgr.instance.Hero;
        if (hero != null && PlayerStateMachine.Instance.GetCurStateType() != enPlayerState.playGame)
            RoleMgr.instance.DestroyRole(hero, false);

        m_preLoadMat = null;
    }

    public override void SendResult(bool isWin)
    {
        UIMgr.instance.Close<UILevel>();
        UIMgr.instance.Open<UICreateRole>();

        var hero = RoleMgr.instance.Hero;
        if (hero != null)
            RoleMgr.instance.DestroyRole(hero, false);
    }
}