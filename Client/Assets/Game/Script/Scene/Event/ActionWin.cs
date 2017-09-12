using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionWin : SceneAction
{

    public ActionCfg_Win mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_Win;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log("设置胜利");

        //boss可能有死亡效果 没播完 因为胜利暂停后会立即停止销毁
        Room.instance.StartCoroutine(CoSetWin());
    }

    IEnumerator CoSetWin()
    {
        yield return new WaitForSeconds(0.5f);
        EventMgr.FireAll(MSG.MSG_SCENE, MSG_SCENE.WIN, true);

        //先关闭所有界面
        UIMgr.instance.CloseAll();

        //为避免怪物死后飞魂之前就发送随机出的奖励 导致少了最后一个怪的奖励 这里直接去取奖励 死后的回调用关卡是否结算来判断不给奖励
        LevelScene level = LevelMgr.instance.CurLevel as LevelScene;
        if (level != null)
        {
            Dictionary<int, Role> roles = level.mRoleDic;
            foreach (Role r in roles.Values)
            {
                if (r.IsMonster && r.GetCamp() != enCamp.neutral)
                {
                    level.GiveReward(r);
                }
            }
        }

        LevelMgr.instance.SetWin();
    }
}

