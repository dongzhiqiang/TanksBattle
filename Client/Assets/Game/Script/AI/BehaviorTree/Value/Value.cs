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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Simple.BehaviorTree
{
    public enum enValueRegion
    {
        constant,//常量
        tree,//树变量
        global,
    }

    [JsonCanInherit]
    public abstract class ValueBase
    {
        public static string[] RegionTypeName = new string[] { "常量", "树变量", "全局变量" };

        public string name = "";//作为共享变量时的变量名
        public enValueRegion region = enValueRegion.constant;
        public enValueType type = enValueType.max;

        protected Node curNode;//当前操作的节点
        public Node CurNode { get { return curNode; } set { curNode = value; } }


#if UNITY_EDITOR
        public bool DrawShare(ValueMgr mgr)
        {
            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    return true;
                }

                //类型
                EditorGUILayout.LabelField(ValueMgrCfg.ValueTypeName[(int)this.type], GUILayout.Width(55));

                //名字
                EditorGUILayout.LabelField(this.name, GUILayout.Width(75));

                OnDrawShare(mgr);


            }
            return false;
        }

        public void Draw(string fieldName, NodeCfg nodeCfg, Node n)
        {
            using (new AutoBeginHorizontal())
            {
                //类型
                EditorGUILayout.LabelField(string.Format("{0}({1})", fieldName, ValueMgrCfg.ValueTypeName[(int)type]), GUILayout.Width(100));
                //是变量还是常量
                region = (enValueRegion)EditorGUILayout.Popup((int)region, ValueBase.RegionTypeName, GUILayout.Width(60));

                OnDraw(fieldName, nodeCfg, n);

            }
        }

        
        public abstract void OnDrawShare(ValueMgr mgr);
        public abstract void OnDraw(string fieldName, NodeCfg nodeCfg, Node n);

        protected void DrawShareValueName(NodeCfg nodeCfg)
        {
            if (region == enValueRegion.constant)
                Debuger.LogError("逻辑错误，常量不应该绘制共享变量名");
            else if (region == enValueRegion.global)
            {
                string[] ns = BehaviorTreeMgrCfg.instance.valueMgrCfg.GetNames(this.type);
                int idxOld = Array.IndexOf(ns, this.name);
                int idx = EditorGUILayout.Popup(idxOld, ns);
                if (idx != -1 && idx != idxOld)
                    this.name = ns[idx];
            }
            else if (region == enValueRegion.tree)
            {
                string[] ns = nodeCfg.Tree.valueMgrCfg.GetNames(this.type);
                int idxOld = Array.IndexOf(ns, this.name);
                int idx = EditorGUILayout.Popup(idxOld, ns);
                if (idx != -1 && idx != idxOld)
                    this.name = ns[idx];
            }
            else
                Debuger.LogError("未知的类型:{0}", region);
        }
#endif
    }

    public abstract class ValueBase<T> : ValueBase
    {

        //初始值,对于常量来说，就是常量的真实值，注意这里不是给最上层直接用的，一般用Node的GetValue()接口
        public abstract T Val { get; }

        public T GetVal(Node n)
        {
            curNode = n;
            T t = Val;
            curNode = null;
            return t;
        }
    }

    //原始的值类型，int、float这些
    public class Value<T> : ValueBase<T>
    {
        public T v;


        public override T Val
        {
            get { return v; }

        }

        //之后从json序列化创建
        public Value()
        {
            //字符串要特殊处理下，不然会空指针
            var v = this as Value<string>;
            if (v != null)
                v.v = "";
        }


        //首次创建设置进来初始值
        public Value(T v,enValueRegion region = enValueRegion.constant) : this()
        {
            this.v = v;
            var valueType = ValueMgrCfg.s_valueIdx.Get(this.GetType());
            if (valueType == null)
            {
                Debuger.LogError("未知的类型，不能进行创建初始化:{0}", this.GetType());
                return;
            }
            this.type = valueType.type;
            this.region = region;
        }

#if UNITY_EDITOR

        public override void OnDrawShare(ValueMgr mgr)
        {
            //值
            v = (T)EditorUtil.Field(v);

            if (mgr != null)
            {
                do
                {

                    ShareValueBase<T> share = mgr.GetValue<T>(this.name);
                    if (share != null)
                    {
                        share.ShareVal = (T)EditorUtil.Field(share.ShareVal, GUILayout.Width(90));
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
                v = (T)EditorUtil.Field(v);
            else
                DrawShareValueName(nodeCfg);

            if (n != null&& region != enValueRegion.constant)
            {
                do
                {
                    if (!string.IsNullOrEmpty(this.name))
                    {
                        ShareValueBase<T> share = n.GetShareValue<T>(this);
                        if (share != null)
                        {
                            share.ShareVal = (T)EditorUtil.Field(share.ShareVal, GUILayout.Width(90));
                            break;
                        }
                    }
                    EditorGUILayout.LabelField("找不到", GUILayout.Width(90));
                } while (false);
            }
        }

#endif
   }

    public abstract class ShareValueBase
    {
        public abstract void Reset();
    }

    public abstract class ShareValueBase<T> : ShareValueBase
    {
        public string name = "";//作为共享变量时的变量名

        ValueBase<T> initVal;//用于取初始值
        public ValueBase<T> InitVal { get { return initVal; } set { initVal = value; } }
        public abstract T ShareVal { get; set; }


        public override void Reset()
        {
            ShareVal = InitVal.GetVal(null);
        }

    }

        //原始的值类型，int、float这些
    public class ShareValue<T> : ShareValueBase<T>
    {
        public T share;

        public override T ShareVal
        {
            get { return share; }
            set { share = value; }
        }

        
    }
    
}