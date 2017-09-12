#region Header
/**
 * 名称：角色管理器接口
 
 * 日期：2016.5.20
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IRoleMgr 
{
    Role Hero { get; }
    ICollection<Role> Roles { get; }
    
    //创建主角(注意不会创建模型)
    void CreateHero(FullRoleInfoVo vo, enCamp camp = enCamp.camp1);

    //从网络数据创建一个角色，注意这里不创建模型
    Role CreateNetRole(FullRoleInfoVo vo, bool dontLoadModel, RoleBornCxt cxt);

    //创建非网络角色(小怪等)
    Role CreateRole(RoleBornCxt cxt, bool dontLoadModel = false);

    //杀死一个角色 isGround 是否处于倒地状态
    void DeadRole(Role role, bool isGround = false, bool checkLocking = true, bool bHeroKill = true);

    //销毁角色,notDestroyNet表明是不是不销毁网络角色只是删除它的模型
    void DestroyRole(Role role, bool notDestroyNet = true, bool checkLocking = true);

    //销毁所有角色
    void DestroyAllRole(bool isIncludeHero = true, bool checkLocking = true);

    //显示或者隐藏所有角色
    void ShowAllRole(bool show, bool isIncludeHero = true);

    //获取角色，根据池id
    Role GetRole(int id);

    //获取某角色id的角色
    Role GetRoleByRoleId(string roleId);

    //获取某角色id的所有角色
    List<Role> GetRolesByRoleId(string roleId);

    //获取有这个标记的角色，如果n=-1,那么有标记就行了，否则标记要==n
    Role GetRoleByFlag(string flag, int n = -1, Role except = null, Role source = null, enOrderRole orderType = enOrderRole.normal);


    //找到当前角色id的下一个角色
    Role FindNextRole(string roleId, Role curRole);

    //获取离某位置最近的角色
    Role GetClosestTarget(Role source, Vector3 srcPos, enSkillEventTargetType targetType, bool canBeTrap = false, bool canBeBox = false, Role except = null);

    //获取离某角色最近的角色
    Role GetClosestTarget(Role source, enSkillEventTargetType targetType, bool canBeTrap = false, bool canBeBox = false, Role except = null);

    //获取某类型的对象，获取之后按照由近到远的顺序排列,注意SortedList创建的时候要用这个比较器new RoleMgr.CloseComparer()，否则add键值一样的话会报错
    void GetCloseTargets(Role source, enSkillEventTargetType targetType, ref SortedList<float, Role> targets, bool canBeTrap = false, bool canBeBox = false);

    //获取根据范围内的角色
    Role GetTarget(Role source, RangeCfg c);

    //阵营匹配
    bool MatchTargetType(enSkillEventTargetType targetType, Role source, Role target);

    //判断是不是敌人
    bool IsEnemy(Role a, Role b);

  
  

}
