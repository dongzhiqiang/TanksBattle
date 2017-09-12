#region Header
/**
 * 名称：UILevelAreaJoystick
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaJoystick : UILevelArea
{

    #region Fields
    public UIJoystick m_joystick;

    public UILevelSkillItem m_atk;
    public UILevelSkillItem[] m_skills;
    public UILevelSkillItem m_block;
    public UILevelSkillItem m_qte;//提起是qte，按紧是冲撞
    public UILevelTreasureItem m_treasure;

    public StateHandle m_combosState;
    public List<StateHandle> m_combos;

    public SimpleHandle m_hitsHandle;
    public UIArtFont m_hitValue;

    public float m_downTimeLimit = 0.25f;

    float m_timeLastHitTime;
    int m_hitCounter;
    int m_showHitCounter;
    int m_showMaxHitNum;

    int m_observer2;
    int m_observer3;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.joystick; } }
    public override bool IsOpenOnStart { get { return true; } }
    public UIJoystick UIJoystick { get { return m_joystick; } }

    public int MaxHitNum { get { return m_showMaxHitNum; } }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        //摇杆触发技能
        m_atk.Init(this, KeyCode.Keypad5, KeyCode.Joystick1Button1, enSkillType.atkUp, enSkillType.atkPress);
        m_block.Init(this, KeyCode.Keypad4, KeyCode.Joystick1Button6, enSkillType.none, enSkillType.none, enSkillType.block, enSkillType.slider);
        m_qte.Init(this, KeyCode.Keypad6, KeyCode.Joystick1Button7, enSkillType.qte, enSkillType.qtePress);
        m_skills[0].Init(this, KeyCode.Keypad1, KeyCode.Joystick1Button0, enSkillType.none, enSkillType.none, (enSkillType)((int)enSkillType.skill1 + 0));
        m_skills[1].Init(this, KeyCode.Keypad2, KeyCode.Joystick1Button3, enSkillType.none, enSkillType.none, (enSkillType)((int)enSkillType.skill1 + 1));
        m_skills[2].Init(this, KeyCode.Keypad3, KeyCode.Joystick1Button4, enSkillType.none, enSkillType.none, (enSkillType)((int)enSkillType.skill1 + 2));

        //神器
        m_treasure.Init(this, KeyCode.Keypad7, KeyCode.Joystick1Button8);

        //摇杆
        m_joystick.m_onJoystickDown = OnJoystickDown;
        m_joystick.m_onJoystickDrag = OnJoystickDrag;
        m_joystick.m_onJoystickUp = OnJoystickUp;
        m_joystick.m_onJoystickSlider = OnJoystickSlider;
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        m_timeLastHitTime = 0;
        m_hitCounter = 0;
        m_showHitCounter = 0;
        m_showMaxHitNum = 0;
        m_hitsHandle.gameObject.SetActive(false);
        m_combosState.SetState(0);

        var hero = RoleMgr.instance.Hero;
        if (hero != null && !reopen)
        {
            var part = hero.SystemsPart;

            var atkOp = part.GetTeachVal("atk_op");
            m_atk.gameObject.SetActive(atkOp == TeachMgr.TEACH_PLAYED_FLAG);

            var skillOp = part.GetTeachVal("skill_op");
            var skillOpOK = skillOp == TeachMgr.TEACH_PLAYED_FLAG;
            for (var i = 0; i < m_skills.Length; ++i)
            {
                m_skills[i].gameObject.SetActive(skillOpOK);
            }

            var qteOp = part.GetTeachVal("qte_op");
            m_qte.gameObject.SetActive(qteOp == TeachMgr.TEACH_PLAYED_FLAG);

            var blockOp = part.GetTeachVal("block_op");
            m_block.gameObject.SetActive(blockOp == TeachMgr.TEACH_PLAYED_FLAG);

            var errMsg = "";
            m_treasure.gameObject.SetActive(SystemMgr.instance.IsEnabled(enSystem.treasure, out errMsg));
        }

        m_atk.Open(reopen);
        m_block.Open(reopen);
        m_treasure.Open(reopen);
        m_qte.Open(reopen);
        m_skills[0].Open(reopen);
        m_skills[1].Open(reopen);
        m_skills[2].Open(reopen);
    }

    protected override void OnUpdateArea()
    {
#if UNITY_EDITOR
        //调试技能
        if (Input.GetKeyDown(KeyCode.PageDown) && CombatMgr.instance.m_debugRoleId != -1 &&
        !string.IsNullOrEmpty(CombatMgr.instance.m_debugSkillId))
        {
            Role r = RoleMgr.instance.GetRole(CombatMgr.instance.m_debugRoleId);
            if (r != null && r.State == Role.enState.alive)
            {
                r.CombatPart.Play(CombatMgr.instance.m_debugSkillId, null, false, true);
            }
        }

#endif

        //hit数消失
        if (m_timeLastHitTime != 0 && m_hitsHandle.gameObject.activeSelf && Time.time - m_timeLastHitTime > 2)
        {
            m_hitsHandle.gameObject.SetActive(false);
            m_timeLastHitTime = 0;
            m_hitCounter = 0;
            m_showHitCounter = 0;
            m_showMaxHitNum = 0;
        }
        if (m_hitCounter > m_showHitCounter && Time.time - m_timeLastHitTime > 0.1f)
            ShowHit();


        //连击图标消失
        CombatPart c = Role.CombatPart;
        if (m_combosState.CurStateIdx != 0 && (Role == null || Role.State != Role.enState.alive || c.LastSkill == null || !c.LastSkill.CanShowCombo))
            m_combosState.SetState(0);

        //设置操作中
        if (Role != null && Role.State == Role.enState.alive &&m_joystick.IsDraging)
            Role.AIPart.FreshOperation();
    }

    //关闭
    protected override void OnCloseArea()
    {
        if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
        if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }

        UIJoystick.JoystickUp();
    }

    protected override void OnRoleBorn()
    {
        if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
        if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }
        m_observer2 = Role.Add(MSG_ROLE.SKILL, OnSkill);
        m_observer3 = Role.Add(MSG_ROLE.HIT, OnHit);

        m_atk.FreshItem();
        m_block.FreshItem();
        m_treasure.FreshItem(true);
        for (int i = 0; i < m_skills.Length && i < 3; ++i)
        {
            m_skills[i].FreshItem();
        }
    }

    #endregion

    #region Private Methods
    void OnJoystickDown(Vector2 pos)
    {

    }

    void OnJoystickDrag(Vector2 delta)
    {
        if (Role == null || Role.State != Role.enState.alive || !Role.CanMove) return;

        if (delta.sqrMagnitude >= 100)//防止位移过小的情况下出现
        {
            if (Role.MovePart.MoveByCameraDir(delta))
                Role.AIPart.FreshOperation();
        }
    }

    void OnJoystickUp()
    {
        if (Role == null || Role.State != Role.enState.alive) return;

        Role.MovePart.Stop();
    }
    void OnJoystickSlider()
    {

        if (Role == null || Role.State != Role.enState.alive) return;

        CombatPart combat = Role.CombatPart;
        Skill s = combat.GetSkill(enSkillType.slider);
        if (s == null)
            return;
        var ret = combat.Play(s);
        if (ret != CombatPart.enPlaySkill.fail)
            Role.AIPart.FreshOperation();

    }

    void OnSkill(object p)
    {
        //检查连击显示
        Skill curSkill = (Skill)p;
        Skill trueCurSkill = curSkill.InternalParentSkill != null ? curSkill.InternalParentSkill : curSkill;
        Skill firstSkill = trueCurSkill.ComboFirst;
        if (!trueCurSkill.CanShowCombo)
        {
            if (m_combosState.CurStateIdx != 0)
                m_combosState.SetState(0);
            return;
        }

        int totalCount = firstSkill.Combos.Count;
        int idx = firstSkill.Combos.IndexOf(trueCurSkill);
        m_combosState.SetState(totalCount);
        for (int i = 0; i < m_combos.Count; ++i)
        {
            int state;//0未激活，1激活
            if (i <= idx)
                state = 1;
            else
                state = 0;

            m_combos[i].SetState(state);
        }

        //如果使用了对应槽位的技能，要播放个特效
        if (curSkill.ComboFirst == m_skills[0].DownSkill)
            m_skills[0].OnUseSkill();
        else if(curSkill.ComboFirst == m_skills[1].DownSkill)
            m_skills[1].OnUseSkill();
        else if (curSkill.ComboFirst == m_skills[2].DownSkill)
            m_skills[2].OnUseSkill();

    }

    public void OnClickSkill(UILevelSkillItem item, Skill s)
    {
        if (Role == null || Role.State != Role.enState.alive) return;
        if (s == null) return;
        
        CombatPart.enPlaySkill ret = Role.CombatPart.PlayCombo(s);

        //如果cd中或者mp不足提示下
        if (ret == CombatPart.enPlaySkill.fail)
        {
            Skill comboSkill = s.ComboSkill;
            if (comboSkill != null)
            {
                if (!s.IsMpEnough)//先检查下mp
                    UIMessage.Show("耐力不足");
            }


        }
        else
            Role.AIPart.FreshOperation();

        //刷新图标
        item.FreshItem();


    }
    void OnHit()
    {
        ++m_hitCounter;

        if (m_timeLastHitTime == 0 || Time.time - m_timeLastHitTime > 0.1f)
            ShowHit();

    }

    void ShowHit()
    {
        ++m_showHitCounter;
        m_showMaxHitNum = m_showHitCounter;
        m_timeLastHitTime = Time.time;
        m_hitsHandle.gameObject.SetActive(true);
        //if (m_showHitCounter < 10)
        //    m_hitValue.m_prefix = "ui_shuzi_big_bai_";
        //else if (m_showHitCounter < 100)
        //    m_hitValue.m_prefix = "ui_shuzi_big_huang_";
        //else
        //    m_hitValue.m_prefix = "ui_shuzi_big_hong_";

        m_hitValue.SetNum(m_showHitCounter.ToString());
        m_hitsHandle.ResetPlay();
    }
    #endregion
}
