#region Header
/**
 * 名称：value
 
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
    public class ValueMgr
    {
        //共享变量
        public Dictionary<string, ShareValueBase> m_values = new Dictionary<string, ShareValueBase>();

        ValueMgrCfg m_cfg;
           
        public ShareValueBase<T> GetValue<T>(string name)
        {
            //先找下
            ShareValueBase obj = m_values.Get(name);
            ShareValueBase<T> shareValue = obj == null ? null : obj as ShareValueBase<T>;

            //检错下
            if (obj != null && shareValue == null)
            {
                Debuger.LogError("行为树变量类型异常，实际类型:{0},目标类型:{1}", obj == null? "空指针": obj.GetType().Name, typeof(ShareValueBase<T>));
                return null;
            }
#if UNITY_EDITOR
            if (shareValue != null)//编辑器下先检查下,可能编辑器已经删除或者修改了
            {
                ValueBase<T> v2 = m_cfg.Get<T>(name);
                if (v2 == null || v2 != shareValue.InitVal)
                {
                    Debuger.LogError("行为树找不到变量，是不是名字或者类型不正确:{0}", name);
                    return null;
                }
            }
#endif

            //找到了就返回
            if (shareValue != null)
                return shareValue;

            //没有找到则创建
            ValueBase<T> v = m_cfg.Get<T>(name);
            if (v == null)
            {
                Debuger.LogError("行为树找不到变量，是不是名字或者类型不正确:{0}", name);
                return null;
            }
            shareValue = ValueMgrCfg.CreateShareValue(v);
            if (shareValue == null)
                return null;
            m_values[shareValue.name] = shareValue;
            return shareValue;

        }

        public void Reset()
        {
            foreach(var v in m_values.Values)
            {
                v.Reset();
            }
        }

        public void SetCfg(ValueMgrCfg cfg)
        {
            m_cfg = cfg;
            m_values.Clear();
        }


        public void Clear()
        {
            m_values.Clear();
        }
    }

    
}