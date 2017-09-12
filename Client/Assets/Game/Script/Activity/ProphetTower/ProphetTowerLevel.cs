using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ProphetTowerType
{
    Challenge = 1,
    Random = 2,
}

public class ProphetTowerLevel : LevelBase
{
    #region Fields
    float m_startTime = 0;
    public static bool prevIsWin = false;
    #endregion
    #region Frame
    public override IEnumerator OnLoad() { yield return 0; }
    //全部加载完成
    public override void OnLoadFinish()
    {
        UILevel uiLevel = UIMgr.instance.Open<UILevel>();
        uiLevel.Open<UILevelAreaGizmos>();

        if (Room.instance.roomCfg.time > 0)     //大于0 倒计时
        {
            var area = uiLevel.Open<UILevelAreaTime>();
            area.SetTime(Room.instance.roomCfg.time);
        }
        else if (Room.instance.roomCfg.time == 0)   //等于0 正计时
        {
            uiLevel.Open<UILevelAreaTime>();
        }

        m_startTime = TimeMgr.instance.logicTime;

    }
    //切换场景时再次进入关卡
    public override void OnEnterAgain() { }
    //是否开始关卡逻辑
    public override bool IsCanStart() { return true; }
    //主角进入场景
    public override void OnHeroEnter(Role hero) { }
    //角色进入场景
    public override void OnRoleEnter(Role role) { }
    //角色死亡 isNow:是否立即销毁
    public override void OnRoleDead(Role role, bool isNow) { }
    //角色死亡状态结束  //有些怪是直接爆开 没有死亡状态
    public override void OnRoleDeadEnd(Role role) { }

    //倒计时结束回调
    public override void OnTimeout(int time) { }
    //离开场景时
    public override void OnLeave() { }
    //退出场景时
    public override void OnExit() { }

    public override void OnUpdate() { }

    public override void SendResult(bool isWin)
    {
        //单机直接回城
        if (Main.instance.isSingle)
        {
            LevelMgr.instance.GotoMaincity();
            return;
        }

        EndTowerReq req = new EndTowerReq();
        req.isWin = isWin;
        req.roomId = roomCfg.id;
        req.towerType = (int)mParam;
        req.useTime = TimeMgr.instance.logicTime - m_startTime;
        NetMgr.instance.ActivityHandler.SendTowerEnd(req);
        prevIsWin = isWin;
    }

    #endregion
    public void OnWin(EndTowerRes resData)
    {
        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = true;
        long totalTime = (long)(TimeMgr.instance.logicTime - m_startTime);
        cxt.desc.Add(new KeyValuePair<string, string>("通关时间：" , StringUtil.FormatTimeSpan(totalTime)));
        if ((ProphetTowerType)resData.towerType == ProphetTowerType.Challenge )
            foreach (var reward in resData.rewards)
            {
                cxt.items.Add(new KeyValuePair<int, int>(int.Parse(reward.Key), reward.Value));
            }
        else
        {
            cxt.desc.Add(new KeyValuePair<string, string>("挑战成功可以开启宝箱", ""));
        }

        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }
    public void OnLose()
    {
        Room.instance.StartCoroutine(CoLose());
    }

    IEnumerator CoLose()
    {
        //yield return new WaitForSeconds(2f);
        UIMgr.instance.Open<UILevelFail>();
        yield return 0;
    }
}
