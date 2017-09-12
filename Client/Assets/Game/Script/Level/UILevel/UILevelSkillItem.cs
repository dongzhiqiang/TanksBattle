#region Header
/**
 * 名称：UILevelSkillItem 
 
 * 日期：2015.12.3
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UILevelSkillItem : MonoBehaviour
{
    public StateHandle btn;
    public ImageEx icon;
    public ImageEx cdMask;
    public Text cdText;
    public StateHandle state;
    public GameObject playFx;
    public GameObject comboingFx;

    UILevelAreaJoystick parent;
    enSkillType upSkill;
    enSkillType pressSkill;
    enSkillType downSkill;
    enSkillType dragingDownSkill;
    KeyCode key;
    KeyCode joystickKey;

    Skill m_runTimeSkill;

    public Skill Skill
    {
        get
        {
            if (m_runTimeSkill != null)
                return m_runTimeSkill;
            if (upSkill == enSkillType.none) return null;
            CombatPart combatPart = parent.Role == null ? null : parent.Role.CombatPart;
            return combatPart == null ? null : combatPart.GetSkill(upSkill);
        }
    }

    public Skill PressSkill
    {
        get
        {
            if (pressSkill == enSkillType.none) return null;
            CombatPart combatPart = parent.Role == null ? null : parent.Role.CombatPart;
            return combatPart == null ? null : combatPart.GetSkill(pressSkill);
        }
    }

    public Skill DownSkill
    {
        get
        {
            CombatPart combatPart = parent.Role == null ? null : parent.Role.CombatPart;
            if (combatPart == null)
                return null;

            if (dragingDownSkill != enSkillType.none && parent.m_joystick.IsDraging)//摇杆拖动中的话用dragingdown
                return combatPart.GetSkill(dragingDownSkill);
            else if (downSkill != enSkillType.none)
                return combatPart.GetSkill(downSkill);
            else
                return null;

        }
    }



    public Skill RuntimeSkill
    {
        get { return m_runTimeSkill; }
        set { m_runTimeSkill = value; }
    }

    public void Init(UILevelAreaJoystick parent, KeyCode key, KeyCode joystickKey, enSkillType upSkill = enSkillType.none, enSkillType pressSkill = enSkillType.none, enSkillType downSkill = enSkillType.none, enSkillType dragingDownSkill = enSkillType.none)
    {
        m_runTimeSkill = null;
        this.key = key;
        this.joystickKey = joystickKey;
        this.upSkill = upSkill;
        this.pressSkill = pressSkill;
        this.downSkill = downSkill;
        this.dragingDownSkill =dragingDownSkill;
        this.parent = parent;

        btn.m_pressHoldTime = parent.m_downTimeLimit;
        btn.AddPressHold(OnHoldPress);
        btn.AddClick(OnClick);
        btn.AddPointDown(OnPointDown);
        btn.AddPointUp(OnPointUp);
    }

    public void Open(bool reOpen)
    {
        if(!reOpen)
            m_runTimeSkill = null;
    }

    public void FreshItem(){
        if(parent == null)return;

        Skill s = Skill;
        if (s == null)s = DownSkill;


        s = s == null?null:s.ShowSkill;//连击中的技能要切换图标

        if (icon != null)
            icon.Set((s ==null ||s.SystemSkillCfg == null )? null : s.SystemSkillCfg.icon);

        enSkillState curState = s == null ? enSkillState.noSkill : s.State;
        

        if (state.CurStateIdx != (int)curState)
            state.SetState((int)curState);

        //cd中的话要刷新时间
        if (curState == enSkillState.cd)
        {
            float cd = s.CDNeed;
            if (cdMask != null)
                cdMask.fillAmount = cd / s.CD;
            if (cdText != null)
            {
                if (cd >= 1)
                    cdText.text = Mathf.FloorToInt(cd).ToString();
                else
                    cdText.text = cd.ToString("0.#");
            }

        }

        if(comboingFx != null)
        {
            bool show = curState == enSkillState.postFrame || curState == enSkillState.buffFrame;
            if(show!= comboingFx.activeSelf)
                comboingFx.SetActive(show);
        }
    }
     void Update()
    {
        FreshItem();

#if UNITY_EDITOR || UNITY_STANDALONE
        if (btn.m_curState == 0 && Input.GetKeyDown(key))
        {
            btn.OnPointerDown(null);
        }
        if (btn.m_curState == 1 && Input.GetKeyUp(key))
        {
            btn.OnPointerUp(null);
            btn.OnPointerClick(null);
        }
#endif

        if (btn.m_curState == 0 && Input.GetKeyDown(joystickKey))
        {
            btn.OnPointerDown(null);
        }
        if (btn.m_curState == 1 && Input.GetKeyUp(joystickKey))
        {
            btn.OnPointerUp(null);
            btn.OnPointerClick(null);
        }
    }

    public void OnHoldPress(StateHandle stateHandle)
    {
        Skill s =  PressSkill;
        if (s != null)
            parent.OnClickSkill(this, s);
    }

    void OnClick()
    {
        Skill s = this.Skill;
        if (s == null) return;
        parent.OnClickSkill(this, s);

    }

    public void OnPointUp(PointerEventData p)
     {
         Skill s = DownSkill;
         if(s==null)return;
         s.Parent.RSM.StateCombat.PressSkill = null;//向战斗系统取消按紧的技能
    }

     public void OnPointDown(PointerEventData p)
     {
         
         Skill s = DownSkill;
         if (s == null) return;
         s.Parent.RSM.StateCombat.PressSkill = s;//向战斗系统记下按紧的技能
         parent.OnClickSkill(this, s);  
     }

    public void OnUseSkill()
    {
        if(playFx!=null)
        {
            playFx.SetActive(false);
            playFx.SetActive(true);
        }
            
    }





}
