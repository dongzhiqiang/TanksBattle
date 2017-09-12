#region Header
/**
 * 名称：UILevelAreaHead
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaHead : UILevelArea
{
    
    #region Fields
    public UISmoothProgress m_hp;
    public UISmoothProgress m_mp;

    public SequenceHandleEx m_switchWeapon;
    public ImageEx m_curWeapon;
    public ImageEx m_nextWeapon;
    public ImageEx m_nextNextWeapon;

    public UILevelPetHead m_pet1;
    public UILevelPetHead m_pet2;

    int m_observer;
    int m_observer2;
    int m_observer3;
    int m_observer4;

    //切换武器标识
    bool isSwitchNextWeapon = false;
    bool isSwitchLastWeapon = false;

    int m_oldMPVal = 0;
    int m_minNeedMP = 0;

    #endregion

    #region Properties
    public override enLevelArea Type { get{return enLevelArea.head;}}
    public override bool IsOpenOnStart { get{return true;} }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        m_switchWeapon.m_onDrag = OnDragSwitch;
        m_switchWeapon.m_onDragEnd = OnEndSwitch;
        m_switchWeapon.m_onCanDrag = CanSwitch;
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
    }

    protected override void OnUpdateArea()
    {
        //摇杆控制切换武器
        if (Input.GetAxis("UnderJoystickHorizontal") != 0 && !isSwitchNextWeapon)
        {
            if (Input.GetAxis("UnderJoystickHorizontal") == 1)
                OnEndSwitch(1);
            else
                OnEndSwitch(-1);
            isSwitchNextWeapon = true;
        }
        if (Input.GetAxis("UnderJoystickHorizontal") == 0)
        {
            isSwitchNextWeapon = false;
        }        
    }

    //关闭
    protected override void OnCloseArea()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
        if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }
        if (m_observer4 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer4); m_observer4 = EventMgr.Invalid_Id; }
        m_pet1.Close();
        m_pet2.Close();
    }

    protected override void OnRoleBorn()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
        if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
        if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }
        if (m_observer4 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer4); m_observer4 = EventMgr.Invalid_Id; }


        CombatPart c = Role.CombatPart;
        //当前技能
        m_switchWeapon.SetFactor(0);

        OnDragSwitch(0);
        if (c == null || c.Weapons.Count <= 1)
            m_switchWeapon.enabled = false;
        else
            m_switchWeapon.enabled = true;

        //主角的血条和怒气条
        float f = Role.GetInt(enProp.hp) / Role.GetFloat(enProp.hpMax);
        m_hp.SetProgress(f,true);
        f = Role.GetInt(enProp.mp) / Role.GetFloat(enProp.mpMax);
        m_mp.SetProgress(f, true);
        m_observer =Role.AddPropChange(enProp.hp, OnHp);
        m_observer2 =Role.AddPropChange(enProp.hpMax, OnHp);
        m_observer3 =Role.AddPropChange(enProp.mp, OnMp);
        m_observer4 =Role.AddPropChange(enProp.mpMax, OnMp);

        //找到主角的上阵宠物，监听出生
        PetFormation petFormation = Role.PetFormationsPart.GetCurPetFormation();

        m_pet1.gameObject.SetActive(false);
        m_pet2.gameObject.SetActive(false);
        RoomCfg cfg = LevelMgr.instance.CurLevel.roomCfg;
        if (cfg.petNum >= 1)
        {
            m_pet1.gameObject.SetActive(true);
            string guid1 = petFormation.GetPetGuid(enPetPos.pet1Main);
            Role pet1 = string.IsNullOrEmpty(guid1) ? null : Role.PetsPart.GetPet(guid1);
            m_pet1.Init(pet1);
        }
        
        if (cfg.petNum >= 2)
        {
            m_pet2.gameObject.SetActive(true);
            string guid2 = petFormation.GetPetGuid(enPetPos.pet2Main);
            Role pet2 = string.IsNullOrEmpty(guid2) ? null : Role.PetsPart.GetPet(guid2);
            m_pet2.Init(pet2);
        }

        FindMinNeedMP();
    }

    public bool IsInMoniter(Role role)
    {
        return role == Role || m_pet1.Pet == role || m_pet2.Pet == role;
    }
    #endregion

    #region Private Methods
    bool CanSwitch()
    {
        if (Role == null)
            return false;
        return Role.RSM.CurState.CanLeave(Role.RSM.StateSwitchWeapon);
    }
    //左上角切换技能被拖动
    void OnDragSwitch(int idx)
    {
        CombatPart c = Role.CombatPart;
        if (c == null || c.FightWeapon == null || c.Weapons.Count == 0)
        {
            m_curWeapon.Set(null);
            m_nextWeapon.Set(null);
            m_nextNextWeapon.Set(null);
            return;
        }

        WeaponCfg curCfg = c.FightWeapon;
        if (c.Weapons.Count == 1)
        {
            m_curWeapon.Set(curCfg.icon);
            m_nextWeapon.Set(null);
            m_nextNextWeapon.Set(null);
            return;
        }

        List<WeaponCfg> cfgs = c.Weapons;
        int i = cfgs.IndexOf(curCfg) + idx;
        m_curWeapon.Set(cfgs[Util.Clamp(i % cfgs.Count, 0, cfgs.Count)].icon);
        m_nextWeapon.Set(cfgs[Util.Clamp((i + 1) % cfgs.Count, 0, cfgs.Count)].icon);
        m_nextNextWeapon.Set(cfgs[Util.Clamp((i + 2) % cfgs.Count, 0, cfgs.Count)].icon);
    }

    //左上角切换技能被点击或者拖动结束
    void OnEndSwitch(int idx)
    {
        OnDragSwitch(idx);
        CombatPart c = Role.CombatPart;
        int i = c.Weapons.IndexOf(c.FightWeapon) + idx;
        c.FightWeapon = c.Weapons[Util.Clamp(i % c.Weapons.Count, 0, c.Weapons.Count)];

        TeachMgr.instance.OnDirectTeachEvent("combat", "changeweapon");
    }

    void Update()
    {

           
        
        //Debug.Log("水平:" + Input.GetAxis("UnderJoystickHorizontal"));
        //Debug.Log("竖直:" + Input.GetAxis("UnderJoystickVertical"));
        //Debug.Log("水平:" + Input.GetAxis("LeftJoystickHorizontal"));
        //Debug.Log("竖直:" + Input.GetAxis("LeftJoystickVertical"));
    }

    void OnHp()
    {
        float f = Role.GetInt(enProp.hp) / Role.GetFloat(enProp.hpMax);
        m_hp.SetProgress(f, false);
    }

    void OnMp()
    {
        var mp = Role.GetInt(enProp.mp);
        var mpMax = Role.GetFloat(enProp.mpMax);
        float f = mp / mpMax;
        m_mp.SetProgress(f, false);

        var oldMPVal = m_oldMPVal;
        m_oldMPVal = mp;
        //跨边界才触发
        if (mp < m_minNeedMP && oldMPVal >= m_minNeedMP)
            TeachMgr.instance.OnDirectTeachEvent("hero", "lowMP");
        else if (mp >= mpMax && oldMPVal < mpMax)
            TeachMgr.instance.OnDirectTeachEvent("hero", "fullMP");                
    }

    void FindMinNeedMP()
    {
        //先保存旧的耐力值
        m_oldMPVal = Role.GetInt(enProp.mp);

        //扫描当前主角的所以技能，找到释放技能需要最小的耐力值（不需要耐力的技能除外）
        CombatPart part = Role.CombatPart;
        if (part != null)
        {
            m_minNeedMP = int.MaxValue;

            var skillTypes = new enSkillType[] { enSkillType.skill1, enSkillType.skill2, enSkillType.skill3 };
            for (var i = 0; i < skillTypes.Length; ++i)
            {
                var skill = part.GetSkill(skillTypes[i]);

                if (skill == null)
                    continue;

                var cfg = skill.Cfg;
                if (cfg.mpNeed == 0 || (cfg.mpNeed < 0 && cfg.mp == 0))
                    continue;

                var needMP = cfg.mpNeed > 0 ? cfg.mpNeed : cfg.mp;

                m_minNeedMP = Mathf.Min(needMP, m_minNeedMP);
            }

            if (m_minNeedMP == int.MaxValue)
                m_minNeedMP = 0;
        }
    }
    #endregion

    
}
