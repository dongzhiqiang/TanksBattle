using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelState
{
    Loading,
    Runing,
    End,
}
//关卡基类
public class LevelBase
{
    LevelState m_state;
    //切换时传的参数
    public object mParam;
    public RoomCfg roomCfg;
    public LevelState State { get { return m_state; } set { m_state = value; } }

    public Dictionary<int, Role> mRoleDic = new Dictionary<int, Role>();

    #region Role Interface
    public bool RemoveRole(Role role)
    {
        if (role != null)
        {
            mRoleDic.Remove(role.Id);
            return true;
        }
        return false;
    }

    public void RemoveRoleByFlag(string flag)
    {
        List<Role> removeList = new List<Role>();
        foreach (Role role in mRoleDic.Values)
        {
            if (role.GetFlag(flag) > 0 && role.State == Role.enState.alive)
                removeList.Add(role);
        }

        foreach (Role role in removeList)
            role.DeadPart.Handle(true);
    }

    public void RemoveRoleById(string npcId)
    {
        List<Role> removeList = new List<Role>();
        foreach (Role role in mRoleDic.Values)
        {
            if (role.GetString(enProp.roleId) == npcId)
                removeList.Add(role);
        }

        foreach(Role role in removeList)
            role.DeadPart.Handle(true);

    }

    public Role GetRoleById(int npcId)
    {
        Role role;
        if (!mRoleDic.TryGetValue(npcId, out role))
            Debug.Log(string.Format("没找到Npc {0}", npcId));
        return role;
    }

    public List<Role> GetRoleById(string npcId)
    {
        List<Role> roleList = new List<Role>();
        foreach (Role role in mRoleDic.Values)
        {
            if (role.GetString(enProp.roleId) == npcId)
                roleList.Add(role);
        }
        return roleList;
    }

    public List<Role> GetRoleByFlag(string flag)
    {
        List<Role> roleList = new List<Role>();
        foreach (Role role in mRoleDic.Values)
        {
            if (role.GetFlag(flag) > 0)
                roleList.Add(role);
        }
        return roleList;
    }

    public bool IsHaveRole(Role role)
    {
        foreach(Role r in mRoleDic.Values)
        {
            if (r.Id == role.Id)
                return true;
        }
        return false;
    }

    #endregion

    //能不能加非战斗状态
    public virtual bool CanAddUnaliveBuff { get{return false;} }

    //主要用于做预加载
    public virtual IEnumerator OnLoad() { yield return 0; }
    //全部加载完成
    public virtual void OnLoadFinish() {}
    //切换场景时再次进入关卡
    public virtual void OnEnterAgain() {}
    //是否开始关卡逻辑
    public virtual bool IsCanStart() { return true; }
    //主角进入场景
    public virtual void OnHeroEnter(Role hero) { }
    //角色进入场景
    public virtual void OnRoleEnter(Role role) { }
    //角色死亡 isNow:是否立即销毁
    public virtual void OnRoleDead(Role role, bool isNow) { }
    //角色死亡状态结束  //有些怪是直接爆开 没有死亡状态
    public virtual void OnRoleDeadEnd(Role role) { }
    
    //倒计时结束回调
    public virtual void OnTimeout(int time) { }
    //离开场景时
    public virtual void OnLeave() { }
    //退出关卡时
    public virtual void OnExit() { }

    public virtual void OnUpdate() { }

    public virtual void SendResult(bool isWin) { }

    //创建全局敌人的时候，返回全局敌人的阵营，如果不希望创建可以返回enCamp.max
    public virtual enCamp OnCreateGlobalEnemy() {return enCamp.camp2;}

    //用于修正英雄和宠物血量的等级值，一般取主角等级，竞技场等活动中应该取双方角色的均值
    public virtual int GetHpRateLv() { return RoleMgr.instance.Hero.GetInt(enProp.level); }
    

}
