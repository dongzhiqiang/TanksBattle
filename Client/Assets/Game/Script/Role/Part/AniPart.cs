#region Header
/**
 * 名称：动画部件
 
 * 日期：2015.9.21
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SimpleAniCxt 
{
    public string aniName="";
    public WrapMode wrapMode = WrapMode.ClampForever;
    public float duration = -1;//-1则为动作播放时间
    public float fade = 0.2f;
    public bool expand=true;

    SimpleAniCxt next;

    public static string[] AniTypeName = new string[] { "单次", "循环", "来回" };
    public static Dictionary<string, WrapMode> s_aniTypeNames = new Dictionary<string, WrapMode>();
    public static Dictionary<WrapMode, string> s_aniTypes = new Dictionary<WrapMode, string>();

    public SimpleAniCxt Next {get{return next;}set{next =value;}}

    static SimpleAniCxt()
    {
        s_aniTypeNames["单次"] = WrapMode.ClampForever;
        s_aniTypeNames["循环"] = WrapMode.Loop;
        s_aniTypeNames["来回"] = WrapMode.PingPong;
        foreach(var pair in s_aniTypeNames){
            s_aniTypes[pair.Value] = pair.Key;
        }
    }

    public static SimpleAniCxt Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        SimpleAniCxt cxt = new SimpleAniCxt();
        string[] pp = s.Split(':');
        cxt.aniName = pp[0];

        if (pp.Length < 2 || !s_aniTypeNames.TryGetValue(pp[1], out cxt.wrapMode))
            cxt.wrapMode = WrapMode.ClampForever;

        if (pp.Length < 3 || !float.TryParse(pp[2], out cxt.duration))
            cxt.duration = -1;

        if (pp.Length < 4 || !float.TryParse(pp[3], out cxt.fade))
            cxt.fade = 0.2f;


        return cxt;
    }
    public void CopyFrom(SimpleAniCxt cfg)
    {
        if (cfg == null) return;


        //复制值类型的属性
        Util.Copy(cfg, this);
    }
}

public class SimpleAnimationsCxt //简单的动作序列的上下文
{
    public List<SimpleAniCxt> anis= new List<SimpleAniCxt>();

    Dictionary<string,SimpleAniCxt> anisByName =new Dictionary<string,SimpleAniCxt>();
    bool m_cache = false;

    public Dictionary<string,SimpleAniCxt> AnisByName {get{return anisByName;}}

    public static SimpleAnimationsCxt Parse(string s)
    {
        if(string.IsNullOrEmpty(s))
            return null;

        string[] pp = s.Split('|');
        SimpleAnimationsCxt cxt = new SimpleAnimationsCxt();
        for(int i =0;i<pp.Length;++i){
            SimpleAniCxt c = SimpleAniCxt.Parse(pp[i]);
            if(c == null)
                continue;
            cxt.anis.Add(c);
        }

        if (cxt.anis.Count == 0)
            return null;

        cxt.Cache();
        return cxt;
    }

    public void Cache()
    {
        if (m_cache)return;
        m_cache =true;

        SimpleAniCxt ani;
        for(int i = 0 ;i<anis.Count;++i){
            ani =anis[i];
            ani.Next = (i+1)< anis.Count?anis[i+1]:null;
            anisByName[ani.aniName] =ani;
        }
    }

    public void CopyFrom(SimpleAnimationsCxt cfg)
    {
        if (cfg == null) return;


        //复制值类型的属性
        Util.Copy(cfg, this);

        //复制引用类型的属性
        anis.Clear();
        foreach (SimpleAniCxt c in cfg.anis)
        {
            anis.Add(new SimpleAniCxt());
            anis[anis.Count - 1].CopyFrom(c);
        }
    }
}

public class AniPart:RolePart
{
    #region Fields
    AniFxMgr m_ani;
    float m_moveAniSpeed = 6;
    SimpleAnimationsCxt m_anisCxt = null;
    bool m_isAnisWhenPlay=false;
    string m_curAniNameNoPostfix ="";
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.ani; } }
    public AniFxMgr Ani { get{return m_ani;}}
    public AnimationState CurSt { get { return m_ani == null ? null : m_ani.CurSt; } }
    public float MoveAniSpeed { get{return m_moveAniSpeed;}}
    #endregion


    #region Frame    
    //属于角色的部件在角色第一次创建的时候调用，属于模型的部件在模型第一次创建的时候调用
    public override void OnCreate(RoleModel model) {
        m_ani = model.Model.GetComponent<AniFxMgr>();
        m_moveAniSpeed = model.SimpleRole.m_moveAniSpeed;
    }

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        if (m_anisCxt != null)
        {
            m_anisCxt = null;
        }
        m_isAnisWhenPlay = false;
    }

    //每帧更新
    public override void OnUpdate() {
        CheckSimpleAnis();
    }


    public override void OnDestroy()
    {
        //老的先删掉
        if (m_anisCxt != null)
        {
            m_anisCxt = null;
        }
        m_curAniNameNoPostfix ="";
        if(m_ani!= null)
            m_ani.DestroyFx();
    }

    #endregion


    #region Private Methods
    void CheckSimpleAnis()
    {
        if (m_anisCxt == null)
            return;

        AnimationState st = CurSt;
        if (st == null)
        {

            m_anisCxt = null;
            return;
        }

        SimpleAniCxt cxt = m_anisCxt.AnisByName.Get(m_curAniNameNoPostfix);
        if(cxt == null)
        {
            
            m_anisCxt = null;
            return;
        }

        //是不是结束了
        bool isOver;
        if (cxt.duration == -2)
            isOver = false;
        else if (cxt.duration == -1)
            isOver = st.time >= st.length;
        else
            isOver = st.time > cxt.duration;
        if(!isOver)
            return;

        if(cxt.Next== null)
        {
            m_anisCxt = null;
            return;
        }


        //播放下一段
        Play(cxt.Next);
    }
    #endregion
    public AnimationState GetSt(string name)
    {
        return m_ani == null ? null : m_ani.GetSt(name);
    }

    public bool HasAnis(SimpleAnimationsCxt cxt)
    {
        if (cxt == null || m_ani == null)
            return false;
        
        var sts = m_ani.Sts;
        for(int i=0,j= cxt.anis.Count;i<j;++i)
        {
            var a = cxt.anis[i];
            if (string.IsNullOrEmpty(a.aniName))
                continue;
            if (!sts.ContainsKey(a.aniName))
                return false;
        }
        return true;
    }

    public void Play(string aniName, WrapMode wrapMode, AniRateCfg rateCfg, float fade = 0.2f)
    {
        if (m_ani == null)
        {
            Debuger.LogError("模型为空");
            return;
        }

        m_ani.Play(aniName, wrapMode, rateCfg, fade);
    }

    public void Play(string aniName, WrapMode wrapMode, float fade = 0.2f, float speed = 1f,bool checkWeaponPostfix=false)
    {
        if (m_ani == null)
        {
            Debuger.LogError("模型为空");
            return;
        }
        

        //检查动画序列，如果不是播放动作序列的动作但是正在动画序列播放中，那么销毁动画序列上下文
        if (!m_isAnisWhenPlay)
        {
            if (m_anisCxt != null)
            {
                m_anisCxt = null;
            }
        }

        //武器动作前缀
        m_curAniNameNoPostfix = aniName;//记录下当前在播的动作名
        if (checkWeaponPostfix&&CombatPart.FightWeapon != null && !string.IsNullOrEmpty(CombatPart.FightWeapon.postfix) )
        {
            string name2 = aniName + CombatPart.FightWeapon.postfix;
            if (GetSt(name2) == null)
            {
                Debuger.LogError("找不到动作:{0} 先用默认动作替代", name2);
            }
            else
                aniName = name2;
        }


        m_ani.Play(aniName,wrapMode,fade,speed);
    }

    public void Play(SimpleAniCxt c)
    {
        m_isAnisWhenPlay = true;
        Play(c.aniName, c.wrapMode, c.fade,1f,true);
        m_isAnisWhenPlay = false;
    }

    //支持连续播放多个动作(动作序列)
    public void Play(SimpleAnimationsCxt cxt)
    {
        //老的先删掉
        if (m_anisCxt != null)
        {
            m_anisCxt = null;
        }

        if (cxt == null ||cxt.anis.Count == 0)
        {   
            //if (cxt != null)
            //    cxt.Put();            
            return;
        }

        m_isAnisWhenPlay = true;
        m_anisCxt = cxt;
        m_anisCxt.Cache();
        Play(m_anisCxt.anis[0]);
        m_isAnisWhenPlay = false;
    }

    //动作序列是不是播放完了
    public bool IsAnisOver(SimpleAnimationsCxt anis)
    {
        return m_anisCxt == null || m_anisCxt != anis;
    }


    //卡帧
    public void AddPause(float duration)
    {
        if (m_ani == null)
        {
            Debuger.LogError("模型为空");
            return;
        }
        m_ani.AddPause(duration);
    }
    public void ResetPause()
    {
        if (m_ani == null)
        {
            Debuger.LogError("模型为空");
            return;
        }
        m_ani.ResetPause();
    }
}
