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
    

    public class BehaviorTreeMgrCfg
    {
        public delegate void OnReset();
        //public delegate void onFileChange(BehaviorTreeFileCfg cfg);

        

        static BehaviorTreeMgrCfg s_instance = null;
        public static BehaviorTreeMgrCfg instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Util.LoadJsonFile<BehaviorTreeMgrCfg>("ai/behaviorTreeMgrCfg");
                    if (s_instance == null)
                    {
                        s_instance = new BehaviorTreeMgrCfg();
                    }
                    s_instance.Reset();


                }

                return s_instance;
            }

        }
        static HashSet<string> s_preLoads = new HashSet<string>();

        public ValueMgrCfg valueMgrCfg = new ValueMgrCfg();
        public List<string> files = new List<string>();
        

        public event OnReset onReset;
        //public event onFileChange onFileChange;

        HashSet<string> fileIdx = new HashSet<string>();
        Dictionary<string, BehaviorTreeFileCfg> fileCache= new Dictionary<string, BehaviorTreeFileCfg>();
        
        void Save()
        {
            Util.SaveJsonFile("ai/behaviorTreeMgrCfg", this);
        }

        public void Reset()
        {
            valueMgrCfg.Reset();
            fileIdx.Clear();
            foreach (var file in files)
                fileIdx.Add(file);

            if(onReset!= null)
                onReset();
        }
        
        public void AddValue(enValueType type, string name){valueMgrCfg.Add(type, name);}

        public void RemoveValue(string name){valueMgrCfg.Remove(name);}

        public BehaviorTreeFileCfg AddFile(string file)
        {
            if(string.IsNullOrEmpty(file))
            {
                Debuger.LogError("行为树文件名不能为空");
                return null;
            }
            if(fileIdx.Contains(file))
            {
                Debuger.LogError("行为树文件已经存在不能新建");
                return null;
            }
            var cfg = new BehaviorTreeFileCfg();
            cfg.File = file;
            files.Add(file);
            Reset();
            return cfg;
        }

        public void RemoveFile(BehaviorTreeFileCfg fileCfg)
        {
            files.Remove(fileCfg.File);
            Util.RemoveJsonFile("ai/" + fileCfg.File);
            fileCache.Remove(fileCfg.File);
            Reset();
        }

        public void RemoveFileCache(BehaviorTreeFileCfg fileCfg)
        {
            var cacheCfg = fileCache.Get(fileCfg.File);
            if (cacheCfg == fileCfg)
                fileCache.Remove(fileCfg.File);
        }

        public BehaviorTreeFileCfg GetFile(string file )
        {
            if (string.IsNullOrEmpty(file))
            {
                Debuger.LogError("行为树文件创建失败，传进来的id为空");
                return null;
            }
            BehaviorTreeFileCfg cfg;
            cfg = fileCache.Get(file);
            if (cfg != null)
                return cfg;

            cfg = Util.LoadJsonFile<BehaviorTreeFileCfg>("ai/" + file);
            if (cfg == null)
            {
                if (fileIdx.Contains(file))
                {
                    cfg = new BehaviorTreeFileCfg();
                }
                else
                    cfg = AddFile(file);

            }
            cfg.File = file;
            cfg.Reset();
            fileCache[file] = cfg;
            return cfg;
        }

        public void SaveFile(BehaviorTreeFileCfg fileCfg)
        {
            Util.SaveJsonFile("ai/" + fileCfg.File, fileCfg);
            Save();
        }
        
        public static void PreLoad(string behavior)
        {
            if (string.IsNullOrEmpty(behavior) || behavior=="-1")
                return;

            if (s_preLoads.Contains(behavior))
                return;
            s_preLoads.Add(behavior);

            string[] ps = behavior.Split(':');
            if (ps.Length < 2 || string.IsNullOrEmpty(ps[0]) || string.IsNullOrEmpty(ps[1]))
            {
                Debuger.LogError("不能执行预加载，找不到行为:{0}", behavior);
                return;
            }

            BehaviorTreeFileCfg fileCfg = BehaviorTreeMgrCfg.instance.GetFile(ps[0]);
            var cfg = fileCfg.GetTree(ps[1]);
            if (cfg == null)
                return;
            cfg.PreLoad();
        }
    }
}