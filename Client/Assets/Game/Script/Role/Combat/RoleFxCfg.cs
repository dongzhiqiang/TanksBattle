#region Header
/**
 * 名称：角色特效配置
 
 * 日期：2015.12.8
 * 描述：
 *      用于状态特效和被击特效
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class RoleFxCfg
{
    static Dictionary<int, RoleFxCfg> s_cfgs;
    static Dictionary<string, RoleFxCfg> s_cfgsByName = new Dictionary<string, RoleFxCfg>();
    static string[] s_hitFxNames;
    static string[] s_fxNames;
    static HashSet<int> s_preLoads = new HashSet<int>();

    public static string[] HitFxNames
    {
        get
        {
            if (s_hitFxNames == null)
            {
                CheckInit();
                List<string> l = new List<string>();

                foreach (RoleFxCfg c in s_cfgsByName.Values)
                {
                    if (c.posType != enFxCreatePos.hitPos)
                        continue;
                    l.Add(c.name);
                }

                l.Sort();
                l.Add("空");
                s_hitFxNames = l.ToArray();
            }
            return s_hitFxNames;
        }
    }

    public static string[] FxNames
    {
        get
        {
            if (s_fxNames == null)
            {
                CheckInit();
                List<string> l = new List<string>();

                foreach (RoleFxCfg c in s_cfgsByName.Values)
                {
                    l.Add(c.name);
                }

                l.Sort();
                s_fxNames = l.ToArray();
            }
            return s_fxNames;
        }
    }



    public int id;
    public string name="";
    public enFxCreatePos posType = enFxCreatePos.source;
    public enFxCreateDir dirType = enFxCreateDir.forward;
    public string bone = "";
    public bool follow = true;
    public string fx = "";
    public float percent = 1f;
    public string fireFx;
    public string iceFx;
    public string thunderFx;
    public string darkFx;
        

    FxCreateCfg createCfg =null;

    public FxCreateCfg CreateCfg {
        get {
            if (createCfg == null)
            {
                createCfg = new FxCreateCfg();
                createCfg.name =fx;
                createCfg.posType = posType;
                createCfg.dirType = dirType;
                createCfg.bone = bone;
                createCfg.follow = follow;
                createCfg.fireFx = this.fireFx;
                createCfg.iceFx = this.iceFx;
                createCfg.thunderFx = this.thunderFx;
                createCfg.darkFx = this.darkFx;
            }
            return createCfg;
        }
    }

    static void CheckInit()
    {
        if(s_cfgs==null)
            Init();
    }

    public static void Init()
    {
        s_cfgs = Csv.CsvUtil.Load<int, RoleFxCfg>("role/roleFx", "id");
        s_cfgsByName.Clear();
        foreach (RoleFxCfg c in s_cfgs.Values)
        {
            if(string.IsNullOrEmpty(c.name))
                continue;
            s_cfgsByName[c.name] = c;
        }
        s_hitFxNames = null;
        s_fxNames = null;
        s_preLoads.Clear();
    }

     //预加载下特效
    public static void ProLoad(int id)
    {
        CheckInit();
        if (id<=0)
            return;

        if (s_preLoads.Contains(id))
            return;
        s_preLoads.Add(id);

        RoleFxCfg cfg = Get(id);
        if (cfg == null)
            return;

        cfg.CreateCfg.PreLoad();
    }
    //预加载下特效
    public static void ProLoad(string name)
    {
        RoleFxCfg fx = Get(name);
        if (fx == null) return;
        ProLoad(fx.id);
    }
    public static RoleFxCfg Get(int id)
    {
        CheckInit();
        RoleFxCfg cfg = s_cfgs.Get(id);
        if (cfg == null)
            Debuger.LogError("角色特效找不到.id:{0}", id);
        return cfg;
    }
    public static RoleFxCfg Get(string name)
    {
        CheckInit();
        RoleFxCfg cfg = s_cfgsByName.Get(name);
        if(cfg == null)
            Debuger.LogError("角色特效找不到.name:{0}", name);
        return cfg;
    }

    public static GameObject Play(string name,Role source, Role target)
    {
        return Play(name, source, target, source.TranPart.GetYOff(0.5f));
    }

    public static GameObject Play(string name, Role source, Role target, Vector3 pos, enElement elem= enElement.none)
    {
        RoleFxCfg cfg =Get(name);
        if(cfg == null) return null;

        return Play(cfg, source, target, pos, elem, true);
    }

    public static GameObject Play(int id, Role source, Role target, bool checkDelayDestroy = true)
    {
        RoleFxCfg cfg = Get(id);
        if (cfg == null) return null;
        return Play(cfg, source, target, source.TranPart.GetYOff(0.5f), enElement.none, checkDelayDestroy);
    }

    public static GameObject Play(RoleFxCfg cfg, Role source, Role target, Vector3 pos, enElement elem, bool checkDelayDestroy )
    {
        if (cfg==null)return null;
        if (source.transform == null)
        {
            Debuger.LogError("角色可能被销毁了，不能创建角色特效.{0}", cfg.name);
            return null;
        }
        //有概率创建
        bool b = cfg.percent == 1f || (Random.value < cfg.percent);
        if (!b) return null;

        //转换为FxCreate并创建
        var fxCreateCfg =cfg.CreateCfg;
        GameObject go = fxCreateCfg.Create(source, target, pos, elem);
        if (go == null)
            return null;

        //如果飞出物上没有任何销毁的脚本，那么提示下
        if (checkDelayDestroy &&!FxDestroy.HasDelay(go))
        {
            Debuger.LogError("角色特效上没有绑销毁脚本.特效名:{0}", go.name);
        }

        return go;
    }
}
