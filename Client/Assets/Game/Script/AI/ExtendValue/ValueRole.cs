#region Header
/**
 * 名称：GameObjectValue
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Simple.BehaviorTree
{
    public enum enValueRole
    {
        none,           //无
        self,           //释放者
        hate,           //仇恨目标
        parent,         //主人
        hero,           //主角
        groupAttack,    //群体攻击目标
        groupRound,     //群体包围目标
        cloesetEnemy,   //最近的敌人
        hateNew,        //仇恨值目标
        hateNewNotFind, //仇恨值目标(没有不自动找)
        max
    }

    //原始的值类型，int、float这些
    public class ValueRole : ValueBase<Role> 
    {
        public static string[] ValueRoleType = new string[] {"无", "自己","仇恨目标","主人","主角", "群体攻击目标", "群体包围目标","最近的敌人","仇恨值目标", "仇恨值目标(不自动)" };
        
        public enValueRole roleType = enValueRole.self;
        
        public bool IsAlwaysNull { get { return this.region == enValueRegion.constant && roleType == enValueRole.none; } }
        public override Role Val
        {
            get {
                if (curNode == null)
                    return null;

                switch (roleType)
                {
                    case enValueRole.none:return null;
                    case enValueRole.self:return curNode.GetOwner<Role>();
                    case enValueRole.hate:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State!= Role.enState.alive)
                                return null;

                            return r.HatePart.GetTargetLegacy();
                        }
                    case enValueRole.parent:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State != Role.enState.alive)
                                return null;
                            r = r.Parent;
                            if (r == null || r.State != Role.enState.alive)
                                return null;

                            return r;
                        }
                    case enValueRole.hero:
                        {
                            Role r = RoleMgr.instance.Hero;
                            return r!= null && r.State== Role.enState.alive?r:null;
                        }
                    case enValueRole.groupAttack:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State != Role.enState.alive)
                                return null;

                            return RoleGroupMgr.instance.GetAttackTarget(r);
                        }
                    case enValueRole.groupRound:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State != Role.enState.alive)
                                return null;

                            return RoleGroupMgr.instance.GetRoundTarget(r);
                        }
                    case enValueRole.cloesetEnemy:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State != Role.enState.alive)
                                return null;
                            
                            return RoleMgr.instance.GetClosestTarget(r, enSkillEventTargetType.enemy);
                        }
                    case enValueRole.hateNew:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State != Role.enState.alive)
                                return null;

                            return r.HatePart.GetTarget(true);
                        }
                    case enValueRole.hateNewNotFind:
                        {
                            var r = curNode.GetOwner<Role>();
                            if (r == null || r.State != Role.enState.alive)
                                return null;

                            return r.HatePart.GetTarget(true);
                        }
                    default:
                        {
                            Debuger.LogError("未知的类型:{0}", roleType);
                            return null;
                        }
                }
                
            }    
        }

        //之后从json序列化创建
        public ValueRole()
        {
            
        }


        //首次创建设置进来初始值
        public ValueRole(enValueRegion region)
        {
            
            this.type = enValueType.Role;
            this.region = region;
        }

        //首次创建设置进来初始值
        public ValueRole(enValueRole roleType)
        {

            this.type = enValueType.Role;
            this.region = enValueRegion.constant;
            this.roleType = roleType;
        }



#if UNITY_EDITOR
        public override void OnDrawShare(ValueMgr mgr)
        {

            //值
            EditorGUILayout.LabelField("无初始值");

            if (mgr != null)
            {
                do
                {
                    ShareValueBase<Role> share = mgr.GetValue<Role>(this.name);
                    if (share != null)
                    {
                        Role oldRole = share.ShareVal;
                        RoleModel oldModel = oldRole != null ? oldRole.RoleModel : null;
                        RoleModel model = (RoleModel)EditorGUILayout.ObjectField(oldModel, typeof(RoleModel), true, GUILayout.Width(90));
                        if (model != oldModel)
                        {
                            if (model == null)
                                share.ShareVal = null;
                            else
                                share.ShareVal = model.Parent;
                        }

                        break;
                    }
                    EditorGUILayout.LabelField("找不到", GUILayout.Width(90));
                } while (false);
            }

            

        }

        public override void OnDraw(string fieldName, NodeCfg nodeCfg, Node n)
        {
            
            //变量的话要设置变量名，常量的话要设置值
            if (region == enValueRegion.constant)
                roleType = (enValueRole)EditorGUILayout.Popup((int)roleType, ValueRoleType);
            else
                DrawShareValueName(nodeCfg);

            if (n!=null&& region != enValueRegion.constant)
            {
                do
                {
                    if (!string.IsNullOrEmpty(this.name))
                    {

                        ShareValueBase<Role> share = n.GetShareValue<Role>(this);
                        if (share != null)
                        {
                            Role oldRole = share.ShareVal;
                            RoleModel oldModel = oldRole != null ? oldRole.RoleModel : null;
                            RoleModel model = (RoleModel)EditorGUILayout.ObjectField(oldModel, typeof(RoleModel), true, GUILayout.Width(90));
                            if(model != oldModel)
                            {
                                if (model == null)
                                    share.ShareVal = null;
                                else
                                    share.ShareVal = model.Parent;
                            }
                            
                            break;
                        }
                    }
                    EditorGUILayout.LabelField("找不到", GUILayout.Width(90));
                } while (false);
            }
        }
#endif
    }
    
    public class ShareValueRole: ShareValueBase<Role>
    {
        Role r;
        int poolId;

        public override Role ShareVal
        {
            get { return r!= null&& !r.IsUnAlive(poolId)?r:null; }
            set {
                r = value;
                poolId = r != null ? r.Id : -1;
            }
        }
    }
}