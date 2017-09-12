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

public class UILevelTreasureItem : MonoBehaviour
{
    public StateHandle btn;
    public ImageEx icon;
    public ImageEx cdMask;
    public Text cdText;
    public StateHandle state;
    public Image[] actives;
    public Image[] hides;

    UILevelAreaJoystick parent;
    KeyCode key;
    KeyCode joystickKey;
    
    int lastCounter=-1;
   


    public void Init(UILevelAreaJoystick parent, KeyCode key, KeyCode joystickKey)
    {
        this.key = key;
        this.joystickKey = joystickKey;
        this.parent = parent;

        btn.AddClick(OnClick);
    }

    public void Open(bool reOpen)
    {
        
    }

    void Update()
    {
        FreshItem(false);

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

 
    
    public void FreshItem(bool force){
        if(parent == null)return;
        var r = parent.Role;
        var combatPart = r ==null?null:r.CombatPart;
        int useCount = combatPart == null ? 0 : combatPart.TreasureCounter;
        int totalCount = combatPart == null ? 0 : combatPart.TreasureCount;
        var preTreasure = combatPart == null?null:combatPart.GetPreTreasure();
        var curTreasure = combatPart == null ? null : combatPart.GetCurTreasure();
        var preSkill = (preTreasure == null || string.IsNullOrEmpty(preTreasure.skillId)) ? null : combatPart.GetSkill(preTreasure.skillId);
        var curSkill = (curTreasure == null || string.IsNullOrEmpty(curTreasure.skillId)) ? null : combatPart.GetSkill(curTreasure.skillId);
        enSkillState st = preSkill == null ? enSkillState.noSkill : preSkill.State;//状态和cd时间看的是上个宝物技能，而非当前
        
        //刷新三个槽
        if (lastCounter != useCount || force)//这里判断下需不需要刷新，提升下性能
        {
            lastCounter = useCount;
            for (int i = 0; i < actives.Length; ++i)
            {
                if (i >= totalCount)
                {
                    actives[i].gameObject.SetActive(false);
                    hides[i].gameObject.SetActive(false);
                }
                else if (i < lastCounter)
                {
                    actives[i].gameObject.SetActive(false);
                    hides[i].gameObject.SetActive(true);
                }
                else
                {
                    actives[i].gameObject.SetActive(true);
                    hides[i].gameObject.SetActive(false);
                }
            }
        }

        //所有技能都用过后置灰
        bool isFinishAll = useCount > 0 && useCount >= totalCount;
        bool isGrey = isFinishAll && (preSkill.State == enSkillState.normal || preSkill.State == enSkillState.cd);//播放中的话等播放完再置灰
        if (icon.IsGrey != isGrey)
            icon.SetGrey(isGrey);

        //设置图标
        if (isFinishAll&& preTreasure!=null)//所有技能都用完的话，取最后一个
            icon.Set(preTreasure.icon);
        else if (curTreasure != null)
            icon.Set(curTreasure.icon);
        else
            icon.Set(null);

        //设置状态，注意这里要取上一个宝物的
        enSkillState curState;
        if (isGrey)//所有技能都用完的话，不用看cd了
            curState = enSkillState.normal;
        else if (preSkill != null)
            curState = preSkill.State;
        else
            curState = enSkillState.noSkill;
        if (state.CurStateIdx != (int)curState)
            state.SetState((int)curState);
        
        //cd中的话要刷新时间
        if (curState == enSkillState.cd)
        {
            float cd = preSkill.CDNeed;
            if (cdMask != null)
                cdMask.fillAmount = cd / preSkill.CD;
            if (cdText != null)
            {
                if (cd >= 1)
                    cdText.text = Mathf.FloorToInt(cd).ToString();
                else
                    cdText.text = cd.ToString("0.#");
            }
        }
        else if(cdMask != null&& cdMask.fillAmount!=1f)
                cdMask.fillAmount = 1f;
    }
    
     

    void OnClick()
    {
        if (parent == null) return;
        var r = parent.Role;
        if (r == null || r.State != Role.enState.alive) return;
        
        CombatPart.enPlaySkill ret = r.CombatPart.PlayTreasureSkill();
        if (ret == CombatPart.enPlaySkill.normal)
            r.AIPart.FreshOperation();

        //刷新图标
        FreshItem(true);
    }




}
