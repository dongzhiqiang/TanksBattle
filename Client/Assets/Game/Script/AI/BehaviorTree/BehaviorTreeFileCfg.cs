#region Header
/**
 * 名称：BehaviorTreeCfg
 
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
    
    public class BehaviorTreeFileCfg
    {
        //public int counter = 1;
        public int nodeCounter = 1;
        public List<BehaviorTreeCfg> trees = new List<BehaviorTreeCfg>();
        
        string file = "";
        //Dictionary<int, BehaviorTreeCfg> idIdx = new Dictionary<int, BehaviorTreeCfg>();
        Dictionary<string, BehaviorTreeCfg> nameIdx = new Dictionary<string, BehaviorTreeCfg>();
        public string[] names;

        public string File { get { return file; }set { file = value; } }

        public void Reset( )
        {
         //   idIdx.Clear();
            nameIdx.Clear();
            
            foreach (var tree in trees)
            {
           //     idIdx[tree.id] = tree;
                nameIdx[tree.name] = tree;
                tree.File = this;
                tree.Reset( );
            }
            List<string> l = new List<string>(nameIdx.Keys);
            l.Sort();
            names = l.ToArray();
        }

        public void ResetNameIdx()
        {
            nameIdx.Clear();
            foreach (var tree in trees)
                nameIdx[tree.name] = tree;
        }


        public BehaviorTreeCfg GetTree(string name)
        {
            var tree = nameIdx.Get(name);
            if(tree == null)
            {
                Debuger.LogError("{0}找不到对应的行为树:{1}",file, name);
                return null;
            }
            return tree;
        }

        public void DoAll(Action<NodeCfg> a)
        {
            foreach (var t in trees)
            {
                t.DoAll(a);
            }
        }

        

       

#if UNITY_EDITOR
        public BehaviorTreeCfg AddTree()
        {
            BehaviorTreeCfg treeCfg = new BehaviorTreeCfg();
            //treeCfg.id = ++counter;
            treeCfg.name = GetUniqueName();
            trees.Add(treeCfg);
            //Reset();外部会重新计算
            return treeCfg;
        }

        
        

        //获取唯一名字
        string GetUniqueName()
        {
            string name = "behavior_" + trees.Count;
            while (nameIdx.ContainsKey(name))
                name += "_" + trees.Count;
            return name;
        }
        public void RemoveTree(BehaviorTreeCfg treeCfg)
        {
            if (!trees.Remove(treeCfg))
            {
                Debuger.LogError("从配置上删除一个行为树失败,id:{0}:{1}", treeCfg.File.File, treeCfg.name);
                return;
            }
            treeCfg.Clear();
        }
        
#endif
    }
}