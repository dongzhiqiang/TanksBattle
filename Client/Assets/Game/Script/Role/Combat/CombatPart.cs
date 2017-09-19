#region Header
/**
 * 名称：战斗部件
 
 * 日期：2015.9.21
 * 描述：释放技能
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CombatPart:RolePart
{
   

    public enum enPlaySkill{
        fail,//cd或者有技能正在播放中
        buff,//有连击或者取消技能加到帧缓冲中，达到连击帧或者缓冲后帧之后会播放
        normal,//正常播放
    }
    #region Fields
    RoleSkillCfg m_roleSkillCfg;
    Dictionary<string,Skill> m_skills = new Dictionary<string,Skill>();
    RoleStateCombat m_combatState;
    WeaponCfg m_fightWeapon;//战斗的时候的当前武器，创建角色模型的时候为m_currentWeapon
    List<WeaponCfg> m_weapons = new List<WeaponCfg>();
    public SortedList<float, Role> m_autoFaceTem = new SortedList<float, Role>(new RoleMgr.CloseComparer());//用于自动朝向的临时列表
    int m_treasureSkillCounter = 0;//使用了多少个神器技能了
    bool m_elementOpen = false;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.combat; } }
    //最后一次使用的技能,如果是内部的，那么这里会返回它的所属技能
    public Skill LastSkill { get{return  m_combatState.LastSkill;}}
    //最后一次使用的技能
    public Skill LastSkillSelf{ get { return m_combatState.LastSkillSelf; } }
    //当前技能，如果是内部的，那么这里会返回它的所属技能
    public Skill CurSkill { get { return m_combatState.CurSkill; } }
    //当前技能
    public Skill CurSkillSelf { get { return m_combatState.CurSkillSelf; } }
    public bool IsPlaying { get { return RSM.CurStateType == enRoleState.combat; } }
    public RoleSkillCfg RoleSkillCfg { get{return m_roleSkillCfg;}}
    public WeaponCfg FightWeapon
    {
        get { return m_fightWeapon; }
        set
        {
            if (m_fightWeapon == value) return;
           
            //强制切换动作
            if(RSM.GotoState(enRoleState.switchWeapon))
            {
                m_fightWeapon = value;
                m_parent.Fire(MSG_ROLE.WEAPON_CHANGE, m_fightWeapon);

                //动作设置当前元素属性
                AniFxMgr aniFxMgr = AniPart.Ani;
                if (aniFxMgr != null)
                {
                    Weapon weapon = WeaponPart.GetWeaponByWeaponId(m_fightWeapon.id);
                    aniFxMgr.RuntimeElement = (enAniFxElement)weapon.CurElementType;
                }
            }
        }
    }
    public List<WeaponCfg> Weapons { get { return m_weapons; } }
    
    //使用了多少个神器技能了
    public int TreasureCounter { get { return m_treasureSkillCounter; } }
    public int TreasureCount { get { return TreasurePart == null ? 0 : TreasurePart.BattleTreasureInfos.Count; } }
    public bool IsElementOpen { get { return m_elementOpen; } }
    #endregion


    #region Frame    

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        m_weapons.Clear();
        m_fightWeapon = null;
        m_treasureSkillCounter = 0;
        if (RSM.IsModelEmpty)
            return true;

        if (string.IsNullOrEmpty(m_parent.Cfg.skillFile))
            Debuger.LogError("role表没有填角色技能配置文件:{0}",m_parent.Cfg.id);

        m_roleSkillCfg = RoleSkillCfg.Get(m_parent.Cfg.skillFile);

        enElement elem = enElement.none;
        if (WeaponPart != null)
        {
            Weapon w = WeaponPart.CurWeapon;

            //元素属性
            string s;
            m_elementOpen = SystemMgr.instance.IsActive(enSystem.element, out s);
            if (m_elementOpen)
            {
                elem = w.CurElementType;
            }
            else
                elem =  enElement.none;


            //每次创建模型都重置下
            m_fightWeapon = w.Cfg;

            

            //设置当前的武器模型
            if(m_fightWeapon == null)
                RenderPart.ChangeWeapon(null);
            else
                RenderPart.ChangeWeapon(m_fightWeapon.modRight, m_fightWeapon.modLeft);

            if (m_weapons.Count == 0)
            {
                for (int i = 0, len = (int)enEquipPos.maxWeapon - (int)enEquipPos.minWeapon + 1; i < len; ++i)
                {
                    WeaponCfg c =WeaponPart.GetWeapon(i).Cfg;
                    if (c != null && !Equip.IsWeaponLocked(i))
                        m_weapons.Add(c);
                }
            }
        }
        else
        {
            elem = m_parent.Cfg.element;
            m_elementOpen = true;
        }
            

        //动作设置当前元素属性
        AniFxMgr aniFxMgr = AniPart.Ani;
        if (aniFxMgr != null)
            aniFxMgr.RuntimeElement = (enAniFxElement)elem;
        return true;
    }
    
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_combatState = RSM.StateCombat;
        
    }

    public override void OnDestroy()
    {
        if (CurSkill!= null)
            CurSkill.Stop(enSkillStop.force);
        //把动态创建出来的技能放回对象池
        foreach(Skill s in m_skills.Values){
            IdTypePool<Skill>.Put(s);
        }
        m_skills.Clear();
    }
    #endregion


    #region Private Methods
    //从技能的自动朝向计算目标
    Role GetAutoTarget(Skill s)
    {
        //自动朝向
        Vector3 lastDir = RSM.StateMove.CurDir;
        SkillCfg cfg = s.Cfg;
        RoleMgr.instance.GetCloseTargets(Parent, enSkillEventTargetType.enemy, ref m_autoFaceTem,false,true);
        if (m_autoFaceTem.Count == 0)
            return null;

        Vector3 srcPos = Parent.TranPart.Pos;
        Vector3 srcDir = Parent.transform.forward;

        Role target;
        //摇杆的朝向,第一范围
        cfg.firstRange.IngoreRadius = true;//不计算敌人的半径
        if (lastDir != Vector3.zero)
        {
            target = GetTarget(srcPos, lastDir, cfg.firstRange, m_autoFaceTem.Values);
            if (target != null)
                return target;
        }

        //如果最后打的敌人在第一范围内，那么选中这个敌人
        Role lastHit = HatePart.GetLastHit();
        if (lastHit != null && CollideUtil.Hit(srcPos, srcDir, lastHit.TranPart.Pos, lastHit.RoleModel.Radius, cfg.firstRange))
        {
            return lastHit;
        }

        //玩家的朝向，第一范围 
        target = GetTarget(srcPos, srcDir, cfg.firstRange, m_autoFaceTem.Values);
        if (target != null)
            return target;

        //玩家的朝向，第二范围
        cfg.secondRange.IngoreRadius = true;//不计算敌人的半径
        target = GetTarget(srcPos, srcDir, cfg.secondRange, m_autoFaceTem.Values);
        if (target != null)
            return target;

        //玩家的朝向，第三范围
        cfg.thirdRange.IngoreRadius = true;//不计算敌人的半径
        target = GetTarget(srcPos, srcDir, cfg.thirdRange, m_autoFaceTem.Values);
        if (target != null)
            return target;

        return null;
    }

    Role GetTarget(Vector3 pos, Vector3 dir, RangeCfg c, IList<Role> l)
    {
        Role r;
        //遍历进行碰撞检测
        for (int i = 0; i < l.Count; ++i)
        {
            r = l[i];
            //碰撞检测
            if (!CollideUtil.Hit(pos, dir, r.TranPart.Pos, r.RoleModel.Radius, c))
                continue;

            return r;
        }
        return null;
    }
    #endregion
    public Skill GetSkill(enSkillType skillType)
    {
        if (skillType == enSkillType.none || skillType == enSkillType.unique || skillType == enSkillType.treasure || skillType == enSkillType.commonMax)
            return null;

        
        string skillId=null;
        if (m_fightWeapon != null)
            skillId = m_fightWeapon.GetSkillId(skillType, RSM.IsAir);
        
        if(!string.IsNullOrEmpty(skillId))
            return GetSkill(skillId);

        //如果不是通用技能，而是武器才可以有的技能，那么返回
        if(skillType >= enSkillType.commonMax)
            return null;

        //如果没有武器模型，那么可以从角色身上取，否则返回
        if (m_fightWeapon != null && !string.IsNullOrEmpty(m_fightWeapon.modRight))
            return null;

        
        RoleCfg roleCfg = m_parent.Cfg;
        skillId = roleCfg.GetSkillId(skillType, RSM.IsAir);
        if (!string.IsNullOrEmpty(skillId))
            return GetSkill(skillId);

        return null;
    }

    public Skill GetSkill(string skillId)
    {
        Skill s = m_skills.Get(skillId);
        if (s!= null)
            return s;
        SkillCfg skillCfg = m_roleSkillCfg.GetBySkillId(skillId);
        if (skillCfg == null)
        {
            //Debuger.LogError("找不到这个技能id，roleId:{0} 技能文件:{1}.json 技能id:{2}", m_parent.GetString(enProp.roleId), m_parent.Cfg.skillFile, skillId);
            return null;
        }

        s =IdTypePool<Skill>.Get();
        s.Init(skillCfg,m_parent);
        m_skills[skillId] = s;
        return s;
    }


    //播放一个技能如果有连击技能的话，播放它的连击技能
    public enPlaySkill PlayCombo(Skill s)
    {
#if UNITY_EDITOR
        //调试技能
        string debugSkillId = m_parent.RoleModel.m_debugSkillId;//技能id|连击技能id|返回值类型，默认都可以填-1，那么就是全部调试
        string debugComboId = "-1";
        int resultType  = -1;

        string[] ss = debugSkillId.Split('|');
        debugSkillId = ss[0];
        if (ss.Length >=2) 
            debugComboId = ss[1];
        if(ss.Length >=3) 
            if (!int.TryParse(ss[2], out resultType)) resultType = -1;
        
        string skillId = s.Cfg.skillId;
        bool needDebug =debugSkillId == "-1" || debugSkillId == skillId;
        if (needDebug)
        {
            Skill comboSkill = s.ComboSkill;
            string comboSkillId = comboSkill == null?"":comboSkill.Cfg.skillId;
            needDebug =   debugComboId == "-1" || debugComboId ==  comboSkillId;
            if(needDebug){
                enPlaySkill result = comboSkill == null ? enPlaySkill.fail : CanPlay(comboSkill, true);
                needDebug = resultType == -1||resultType == (int)result;
                if (needDebug)
                    Debuger.Log("{4}.{0}_{1}使用{2}的连击技能:{3}", m_parent.Cfg.id, m_parent.Id, skillId, comboSkillId, result);
            }
            
            
        }
        
#endif 
        s = s.ComboSkill;
        if (s == null) return enPlaySkill.fail;
 
        return Play(s,null,true,false);
    }

    public enPlaySkill Play(string skillId, Role target = null, bool cancelAndCombo = true, bool force = false)
    {
        Skill s = GetSkill(skillId);
        if(s == null)return enPlaySkill.fail;
        return Play(s, target,cancelAndCombo, force);
    }

    public static bool m_debugSkillPlay = false;

    public enPlaySkill Play(Skill s,Role target=null,bool cancelAndCombo=true,bool force=false)
    {
        if (TimeMgr.instance.IsPause) return enPlaySkill.fail;
        if (m_parent.State != Role.enState.alive)
        {
            Debuger.LogError("没有在战斗态就使用技能roleId:{0} 技能id:{1}", m_parent.Cfg.id, s.Cfg.skillId);
            return enPlaySkill.fail;
        }
        
        
        //检查连击和取消
        if (!force )
        {
            enPlaySkill result =CanPlay(s, cancelAndCombo );
            if(result== enPlaySkill.fail || result== enPlaySkill.buff)
            {
                if (m_debugSkillPlay && result == enPlaySkill.fail)
                    Debuger.Log("调试技能失败1起作用");
                return result;
            }
                
        }

        //技能目标，如果没有的话，战斗状态里会自动计算
        if (target!= null &&!RoleMgr.instance.MatchTargetType(enSkillEventTargetType.enemy, m_parent, target))//检错下，暂时只支持敌人
        {
            Debuger.LogError("逻辑错误，技能目标暂时只支持敌人.roleId:{0} 技能id:{1}", m_parent.Cfg.id, s.Cfg.skillId);
            target = null;
        }
        if (target == null)
        {
            target = GetAutoTarget(s);
            m_autoFaceTem.Clear();
        }
        s.Target = target;

        bool ret = m_combatState.GotoState(s, force);
        if (m_debugSkillPlay && !ret)
            Debuger.Log("调试技能失败2起作用");
        return ret ? enPlaySkill.normal : enPlaySkill.fail;
    }

    //如果有技能的话，暂停
    public void Stop()
    {
        if (!this.IsPlaying)
            return;

        RSM.CheckFree(true);        
    }

    public enPlaySkill CanPlay(string skillId, bool cancelAndCombo = true)
    {
        return CanPlay(GetSkill(skillId), cancelAndCombo);
    }
    public enPlaySkill CanPlay(Skill s, bool cancelAndCombo = true)
    {
        if(s == null)return enPlaySkill.fail;

        if (RSM.IsSilent) return enPlaySkill.fail;

        if (RSM.IsAir && !s.Cfg.isAirSkill) return enPlaySkill.fail;//在空中，但是不是空中技能

        
        if (s.CDNeed > 0)
            return enPlaySkill.fail;//cd中
        else if (!s.IsMpEnough)//先检查下mp
            return enPlaySkill.fail;
        else if(!IsPlaying)//当前没有技能的话就可以直接播放
            return  enPlaySkill.normal ;
        else if(!cancelAndCombo)//如果不能连击和取消，那么不能播放
            return enPlaySkill.fail;
        else //连击和取消
        {
            SkillCfg cfg = s.Cfg;
            Skill curSkill = this.CurSkill;//这里如果是内部技能的话会返回它的父技能
            enSkillState curSt = curSkill.State;
            SkillCfg curCfg= curSkill.Cfg;
            //连击，不按内部技能算
            if (curCfg.comboSkillId == cfg.skillId)
            {
                if (curSt == enSkillState.postFrame)//连击后帧之后
                    return enPlaySkill.normal;
                else if (curSt == enSkillState.buffFrame)//连击前帧之后，后帧之前
                {
                    curSkill.SetComboBuff(cfg.skillId);
                    return enPlaySkill.buff;
                }
                else
                    return enPlaySkill.fail;
            }

            //取消，按内部技能算
            curSkill = this.CurSkillSelf;
            curCfg = curSkill.Cfg;
            if (!curCfg.canBeCanel || !cfg.canCanel||
               cfg.cancelPriority <curCfg.cancelPriority|| //取消优先级要高于当前技能才能取消
               curCfg.isAirSkill!= cfg.isAirSkill)//同样是空中或者同样是地上才能取消
                return enPlaySkill.fail;
            
            if (curCfg.cancelPostFrame!=-1 && curSkill.CurFrame >= curCfg.cancelPostFrame)//打击后帧之后
                return enPlaySkill.normal;
            else if(curCfg.cancelPreFrame!=-1 && curSkill.CurFrame < curCfg.cancelPreFrame)//打击前帧之前
                return enPlaySkill.normal;
            else
            {
                curSkill.SetCancelBuff(cfg.skillId);
                return enPlaySkill.buff;
            }
                
        }

    }

    //是不是所有技能cd中
    public bool IsAllSkillCD()
    {
        RoleCfg cfg = Parent.Cfg;
        
        Skill s = null;
        //技能
        for (int i = cfg.skills.Count - 1; i >= 0; --i)
        {
            s = GetSkill(cfg.skills[i]);
            if (s != null && !s.IsPlayOrCD)
                return false;
        }

        //普通攻击
        s = string.IsNullOrEmpty(cfg.atkUpSkill) ? null : GetSkill(cfg.atkUpSkill);
        if (s != null && !s.IsPlayOrCD)
            return false;

        return true;
    }

    
    //范围内没有技能的情况下，范围外的技能随机取；范围内有技能，范围内的技能随机取
    public Skill AutoFindSkill(float dis=-1)
    {
        //沉默状态下不能使用任何技能
        if (RSM.IsSilent)
            return null;

        RoleCfg cfg = Parent.Cfg;
       
        //技能
        Skill s = null;
        Skill inside = null;
        int insideCount = 0;
        Skill outside = null;
        int outsideCount = 0;

        for (int i = cfg.skills.Count - 1; i >= 0; --i)
        {
            s = m_fightWeapon !=null?GetSkill((enSkillType)i+ (int)enSkillType.skill1) :GetSkill(cfg.skills[i]);//如果有武器的话从武器取技能，没有的话从角色取
            if (s == null || s.IsPlayOrCD|| !s.IsMpEnough)
                continue;
            if (!IsSkillEnabled(cfg.skills[i]))
                continue;

            //范围内
            if (dis != -1 && dis <= s.Cfg.attackRange.distance)
            {
                ++insideCount;
                if (insideCount <=1|| UnityEngine.Random.Range(0, insideCount) == 0)
                    inside = s;
            }
            //范围外,范围内已经有就不用找了
            else if (inside == null)
            {
                ++outsideCount;
                //如果范围内已经有了就不用找了
                if (inside != null)
                    continue;
                if (outsideCount <= 1 || UnityEngine.Random.Range(0, outsideCount) == 0)
                    outside = s;
            }
        }
        //普通攻击
        s = GetSkill(enSkillType.atkUp);
        if (s != null && !s.IsPlayOrCD && s.IsMpEnough)
        {
            //范围内
            if (dis != -1 && dis <= s.Cfg.attackRange.distance)
            {
                ++insideCount;
                if (insideCount <= 1 || UnityEngine.Random.Range(0, insideCount) == 0)
                    inside = s;
            }
            //范围外
            else if(inside == null)
            {
                ++outsideCount;
                if (outsideCount <= 1 || UnityEngine.Random.Range(0, outsideCount) == 0)
                    outside = s;
            }
        }


        return inside!= null? inside:outside;
    }

    bool IsSkillEnabled(string skillId)
    {
        return true;
    }
    
    // 前一个神器，这里不能直接返回技能，因为图标要从神器取
    public TreasureInfo GetPreTreasure() { return TreasurePart == null ? null : TreasurePart.GetBattleTreasure(m_treasureSkillCounter - 1); }
    
    // 当前的神器
    public TreasureInfo GetCurTreasure() { return TreasurePart == null ? null : TreasurePart.GetBattleTreasure(m_treasureSkillCounter); }
    
    public enPlaySkill PlayTreasureSkill()
    {
        var cur = GetCurTreasure();
        if (cur == null|| string.IsNullOrEmpty(cur.skillId))
            return enPlaySkill.fail;

        //如果前一个神器在播放中或者cd中都不让使用
        var pre = GetPreTreasure();
        if(pre!=null )
        {
            var preSkill =  GetSkill(pre.skillId);
            if (preSkill != null && preSkill.State != enSkillState.normal)
                return enPlaySkill.fail;
        }

        var ret =Play(cur.skillId);
        if(ret == enPlaySkill.normal)
            ++m_treasureSkillCounter;
        return ret;

    }

}
