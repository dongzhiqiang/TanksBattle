#region Header
/**
 * 名称：属性定义表
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public enum enBuffTargetType
{
    self,      //0默认自己
    source,    //1释放者
    another,   //2别人
    hate, //3仇恨目标
    hateNew,//4仇恨值目标
    hateNewNotFind,//5仇恨值目标(不自动查找)
    closestSame,//6最近的友方
    closestEnemy,//7最近的敌方
    closestNeutral,//8最近的中立阵营
    parent,//9主人
    hero,//10主角
    max
}


//进一步解析参数的类
public class BuffExCfg
{
    //支持作为非战斗状态，如果不支持那么会延缓到角色出生的时候加这个状态
    public virtual bool SupportUnalive { get { return false; } }
    public virtual bool Init(string[] pp) { return true; }
    public virtual void PreLoad() { }

}

public class BuffCfg
{
    public int id=0;//状态id
    public string name="";//状态名
    public string desc="";//状态说明
    public string type="";//状态类型
    public string[] param=null;//参数
    public float time = -2;//-1,非战斗状态，角色模型没有创建出来也可以加在角色上;-2,永久战斗状态，创建后直到角色模型销毁才会消失;否则，为持续时间
    public float interval = -1;//-1，没有间隔，状态创建的时候执行一次，之后不执行；否则，一旦达到这个间隔就会执行一次
    public int roleFxId = -1;//角色特效id
    public int soundId = -1;
    public bool stopIfEnd = true;
    public string endEventGroupId = "";//结束事件组
    public int[] endBuffId =null;//结束状态ID,-1则表示没有
    public int overlayLimit = -1;//同id叠加限制，-1没上限，否则作用对象身上有超过这个id数量的状态时不能叠加上去
    public bool useOldIfSame = false;//同id叠加或组优先级相同时使用老的(默认是使用新的)
    public int priority = 0;// 免疫优先级，当角色身上有免疫这个状态的状态a时，如果这个状态的优先级大于a的优先级，那么a无效
    public int useful = 1;    //是否有益状态

    //默认组，某个类型的状态在默认组内是叠加还是替换见状态说明表
    //如果填了组名，那么组内部的状态任意时间只能存在一个，存在哪个取组优先级大的
    //组与组之间是叠加还是替换见状态说明表
    public string group = "";//组名
    public int groupPriority=0;//决定组内部替换的时候的状态

    public float cd = -1;//-1无cd，如果有填，下一个同id的状态在cd时间内是加不上的

    public BuffExCfg exCfg;

    enBuff _type = enBuff.min;

    public static Dictionary<int, BuffCfg> s_cfgs = null;
    static HashSet<int> s_preLoads = new HashSet<int>();

    public enBuff BuffType { get{return _type;}}

    //是不是战斗状态，不是的话这个状态可以在角色不是alive的时候加上去且角色退出alive状态的时候不会被销毁
    public bool IsAliveBuff { get{return time != -1;}}

    

    public static void Init()
    {
        s_preLoads.Clear();
        BuffType typeCfg;
        //加入战斗状态配置
        s_cfgs = Csv.CsvUtil.Load<int, BuffCfg>("systemSkill/combatBuff", "id");
        foreach (BuffCfg cfg in s_cfgs.Values)
        {
            if (cfg.id >=10000)
                Debuger.LogError("combat中的状态表不能id不能填超过10000");
        }

        //加入系统状态配置
        Dictionary<int, BuffCfg> systemBuffCfgs =Csv.CsvUtil.Load<int, BuffCfg>("systemSkill/systemBuff", "id");
        foreach (BuffCfg cfg in systemBuffCfgs.Values)
        {
            if(s_cfgs.ContainsKey(cfg.id))
            {
                Debuger.LogError("id重复了，战斗状态表和系统状态表里同时包含了状态:{0}", cfg.id);
                continue;
            }
            if (cfg.id < 10000)
                Debuger.LogError("combat中的状态表不能id不能填小于10000");
            s_cfgs[cfg.id] = cfg;
        }

        //进一步解析参数
        foreach (BuffCfg cfg in s_cfgs.Values)
        {
            typeCfg = global::BuffType.Get(cfg.type);
            if(typeCfg== null)
            {
                Debuger.LogError("状态类型不存在，请检查combatSkill表的状态说明表:{0} 状态id:{1}", cfg.type, cfg.id);
                continue;
            }
                
                    

            cfg._type = typeCfg != null ? typeCfg.id : enBuff.min;

            if (cfg.time > 0) cfg.time /= 1000f;
            if (cfg.interval > 0) cfg.interval /= 1000f;
            if (cfg.cd > 0) cfg.cd /= 1000f;
            cfg.exCfg = BuffFactory.CreateExCfg(cfg);
            cfg.param = null;
        }

            
    }
    public static BuffCfg Get(int id)
    {
        CheckInit();
        BuffCfg cfg = s_cfgs.Get(id);
        if (cfg == null)
            Debuger.LogError("配置表找不到状态:{0}",id);
        return cfg;
    }
    static void CheckInit()
    {
        if (s_cfgs == null)
            Init();
    }
    public static void ProLoad(int id)
    {
        CheckInit();
        if (s_preLoads.Contains(id))
            return;
        s_preLoads.Add(id);

        BuffCfg cfg = Get(id);
        if (cfg == null)
            return;

        if (cfg.roleFxId != -1)
        {
            //Debuger.Log("预加载角色特效:{0}", cfg.roleFxId);
            RoleFxCfg.ProLoad(cfg.roleFxId);
        }
        if (cfg.soundId!= -1)
        {
            SoundMgr.instance.PreLoad(cfg.soundId);
        }

        if (cfg.endBuffId != null )
        {
            for(int i =0 ;i<cfg.endBuffId.Length;++i){
                ProLoad(cfg.endBuffId[i]);    
            }
        }
        SkillEventGroupCfg.PreLoad(cfg.endEventGroupId);

        //加载参数里的配置的资源
        if (cfg.exCfg != null)
            cfg.exCfg.PreLoad(); 
    }
}
