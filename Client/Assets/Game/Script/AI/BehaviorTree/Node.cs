#region Header
/**
 * 名称：Node
 
 * 日期：2016.5.13
 * 描述：一个节点的生命周期：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Simple.BehaviorTree
{

    public enum enNodeState
    {
        inactive,
        success,
        failure,
        running
    }

    public enum enExecute
    {
        normal,//在栈上的时候执行
        interrupt,//条件节点的中断判断逻辑
    }

    //出栈的原因
    public enum enPopReason
    {
        normal,//正常出栈
        interrupt,//被中断了
    }

    public class Node:IdType
    {
        protected NodeCfg m_cfg;
        protected BehaviorTree m_tree;
        protected int m_idx;
        protected int m_parentIdx;
        protected int m_localIdx;//在父节点的第几个位置
        protected enNodeState m_state;//已经执行完的为成功或者失败状态，没执行的为未执行状态，在栈上的为运行中状态
        protected bool m_inStack=false;

        public NodeCfg Cfg { get { return m_cfg; } }
        public int Idx { get { return m_idx; } }
        public int ParentIdx { get { return m_parentIdx; } }
        public int LocalIdx { get { return m_localIdx; } }
        public enNodeState State { get { return m_state; } }
        public bool IsInStack { get { return m_inStack; } }



        #region 用于被继承的属性
        public virtual bool CanRuning{ get { return true; } }

        #endregion


        #region 用于被继承的接口


        //重置临时数据.销毁的时候或者onEnable的时候执行，可以当成数据初始化的地方
        protected virtual void OnResetState() { }

        //行为树被创建的时候
        protected virtual void OnInitNode(){}
        
        //行为树被销毁的时候
        protected virtual void OnDestroyNode(){}

        //行为树被启用。一般和禁用有对应关系，游戏暂停恢复、行为树切换、创建等情况下会调用到
        protected virtual void OnEnable() { }

        //行为树被禁用。一般和启用有对应关系，游戏暂停、行为树切换、销毁等情况下会调用到
        protected virtual void OnDisable() { }

        //入栈。行为树遍历过程中，遍历到一个节点就会入栈它。可以用做是当前次遍历的OnInit
        protected virtual void OnPush() { }

        //执行。遍历到这个节点的时候就会在OnPush()后执行，如果返回running的话就会一直执行，直到返回success或者fail，然后OnPop()
        protected virtual enNodeState OnExecute(enExecute executeType) { return enNodeState.success; }

        //出栈。自己执行完了以及自己的子树执行完成后出栈
        protected virtual void OnPop() { }

        //新的一轮遍历开始
        protected virtual void OnReUpdate() { }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos() { }
#endif
        #endregion

        public T GetOwner<T>()where T:class
        {
            return m_tree.GetOwner<T>();
        }

        public void InitNode(NodeCfg cfg, BehaviorTree tree,int idx,int parentIdx,int localIdx)
        {
            this.m_cfg = cfg;
            this.m_tree = tree;
            this.m_idx = idx;
            this.m_parentIdx = parentIdx;
            this.m_localIdx = localIdx;
            
            OnInitNode();
        }

        public override void OnClear()
        {
            ResetState();
            OnDestroyNode();

            m_cfg = null;
            m_tree = null;
            m_idx = -1;
            m_parentIdx = -1;
            m_localIdx = -1;
        }
        public void Enable(bool reset)
        {

            ClearState();
            if (reset || m_cfg.resetTempValueIfEnable)
            {
                ResetState();
            }

            OnEnable();
        }

        public void Disable()
        {
            OnDisable();
            ClearState();
        }


        public virtual void Push()
        {
            m_state = enNodeState.running;
            m_inStack = true;
            OnPush();
        }

        public void ClearState()
        {
            m_state = enNodeState.inactive;
            m_inStack = false;
        }

        public enNodeState Execute(enExecute executeType)
        {
            var s = OnExecute(executeType);
            if (executeType == enExecute.normal &&s != enNodeState.running)
                m_state = s;
            
            return s;
        }

        public void Pop()
        {
            m_inStack = false;
            OnPop();
        }

        //新的一轮遍历开始
        public void ReUpdate()
        {
            ClearState();
            OnReUpdate();
        }

        public virtual void ResetState() {
            OnResetState();
        }
        
        
        public ShareValueBase<T> GetShareValue<T>(ValueBase<T> v)
        {
            if (v.region == enValueRegion.constant || string.IsNullOrEmpty(v.name))
            {
                Debuger.LogError("常量类型不能获取共享变量");
                return null;
            }
            else if(v.region == enValueRegion.tree)
                return m_tree.GetValue<T>(v.name);
            else if (v.region == enValueRegion.global)
                return BehaviorTreeMgr.instance.GetValue<T>(v.name);
            else
            {
                Debuger.LogError("未知的变量类型：{0}");
                return null;
            }
        }

        //获取值
        public T GetValue<T>(ValueBase<T> v)
        {
            if (v.region == enValueRegion.constant || string.IsNullOrEmpty(v.name))
                return v.GetVal(this);
            else if (v.region == enValueRegion.tree)
            {
                ShareValueBase<T> treeVal = m_tree.GetValue<T>(v.name);
                if (treeVal != null)
                    return treeVal.ShareVal;
                else
                {
                    Debuger.LogError("没有找到树变量，不能获取，是不是变量被删除了，第{0}个节点,变量名:{1}", m_cfg.id, v.name);
                    return v.GetVal(this);
                }
            }
            else if (v.region == enValueRegion.global)
            {
                ShareValueBase<T> globalVal = BehaviorTreeMgr.instance.GetValue<T>(v.name);
                if (globalVal != null)
                    return globalVal.ShareVal;
                else
                {
                    Debuger.LogError("没有找到全局变量，不能获取，是不是变量被删除了，第{0}个节点,变量名:{1}", m_cfg.id, v.name);
                    return v.GetVal(this);
                }
            }
            else
            {
                Debuger.LogError("未知的变量类型：{0}");
            }


            return v.Val;
        }

        //设置值
        public void SetValue<T>(ValueBase<T> v, T val)
        {
            if (v.region == enValueRegion.constant)
            {
                Debuger.LogError("常量不能被设置值");
                return;
            }

            if (string.IsNullOrEmpty(v.name))
            {
                Debuger.LogError("没有指定变量名");
                return;
            }

            if (v.region == enValueRegion.tree)
            {
                ShareValueBase<T> treeVal = m_tree.GetValue<T>(v.name);
                if (treeVal == null)
                {
                    Debuger.LogError("没有找到树变量,不能设置，是不是变量被删除了，第{0}个节点,变量名:{1}", m_cfg.id, v.name);
                    return;
                }
                treeVal.ShareVal = val;
            }
            else if (v.region == enValueRegion.global)
            {
                ShareValueBase<T> globalVal = BehaviorTreeMgr.instance.GetValue<T>(v.name);
                if (globalVal == null)
                {
                    Debuger.LogError("没有找到全局变量,不能设置，是不是变量被删除了，第{0}个节点,变量名:{1}", m_cfg.id, v.name);
                    return;
                }
                globalVal.ShareVal = val;
            }
            else
            {
                Debuger.LogError("未知的变量类型：{0}");
            }
        }

        public void LogError(string format,params object[] ps)
        {
            string s = string.Format("行为树逻辑错误{0}:{1},节点:{2}", this.m_tree.Cfg.File.File,this.m_tree.Cfg.name, m_cfg.id);
            Debuger.LogError(s + format, ps);
        }
    }
}