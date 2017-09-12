#region Header
/**
 * 名称：行为树管理器
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Simple.BehaviorTree
{
    public class BehaviorTreeMgr: SingletonMonoBehaviour<BehaviorTreeMgr>
    {
        public List<BehaviorTree> m_trees = new List<BehaviorTree>();//运行中的树
        public ValueMgr m_valueMgr = new ValueMgr();
        public bool m_lock = false;
        public BehaviorTree m_debug1;
        public BehaviorTree m_debug2;

        protected override void  Awake()
        {
            m_valueMgr.SetCfg(BehaviorTreeMgrCfg.instance.valueMgrCfg);
        }

        public ShareValueBase<T> GetValue<T>(string name)
        {
            return m_valueMgr.GetValue<T>(name);
        }

        public T GetValue<T>(string name,T def)
        {
            var v = m_valueMgr.GetValue<T>(name);
            if (v != null)
                return v.ShareVal;
            else
                return def;
        }

        public void Clear()
        {
            StopAll();
            m_valueMgr.Clear();
        }

        public void AddTree(BehaviorTree t)
        {
            if (m_lock)
            {
                Debuger.LogError("逻辑出错，锁定中不能AddTree");
                return;
            }
            m_trees.Add(t);
        }
        
        public void RemoveTree(BehaviorTree t)
        {
            if (m_lock)
            {
                Debuger.LogError("逻辑出错，锁定中不能RemoveTree");
                return;
            }
            m_trees.Remove(t);
        }

        //找下一个，注意如果只有当前的那么返回空
        public BehaviorTree FindNextTree(BehaviorTreeFileCfg cfg,string behavior, BehaviorTree tree)
        {
            int idx = tree == null ? -1 : m_trees.IndexOf(tree);
            idx = idx == -1 ? m_trees.Count:idx;
            //先找之后的
            for (int i = idx + 1; i < m_trees.Count; ++i)
            {
                var t = m_trees[i];
                if (t.IsTreeAcitve && t.Cfg.File == cfg && (string.IsNullOrEmpty(behavior) || t.Cfg.name == behavior))
                    return t;
            }

            //从头找起
            for (int i =  0; i < idx; ++i)
            {
                var t = m_trees[i];
                if (t.IsTreeAcitve && t.Cfg.File == cfg && (string.IsNullOrEmpty(behavior) || t.Cfg.name == behavior))
                    return t;
            }
            return null;
        }

        public void PlayAll() {
            foreach(var t in m_trees)
            {
                if (t.IsTreeAcitve && !t.IsPlaying)
                    t.RePlay(false);
            }
                
        }

        public void PauseAll()
        {
            if(m_lock)
            {
                Debuger.LogError("逻辑出错，锁定中不能PauseAll");
                return;
            }
            foreach (var t in m_trees)
            {
                if (t.IsTreeAcitve && t.IsPlaying)
                    t.Pause();
            }
        }

        public void StopAll()
        {
            if (m_lock)
            {
                Debuger.LogError("逻辑出错，锁定中不能StopAll");
                return;
            }
            for (int i = m_trees.Count - 1; i >= 0; --i)
            {
                m_trees[i].Stop();
            }
        }

        public void ReCreate(BehaviorTreeFileCfg fileCfg)
        {
            for(int i = m_trees.Count-1;i>=0;--i)
            {
                var t = m_trees[i];
                if (t.IsTreeAcitve && t.Cfg.File == fileCfg)
                    t.ReCreate();
            }
        }
        public void Lock()
        {
            if(m_lock)
            {
                Debuger.LogError("重复锁定");
                return;
            }
            m_lock = true;
        }

        public void Unlock()
        {
            if (!m_lock)
            {
                Debuger.LogError("重复解锁");
                return;
            }
            m_lock = false;
        }


    }
}
