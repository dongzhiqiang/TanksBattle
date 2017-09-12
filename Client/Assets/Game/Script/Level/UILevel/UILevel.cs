using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UILevel : UIPanel
{
    
    #region Fields
    public List<UILevelArea> m_areas;

    List<UILevelArea> m_cacheOpenArea = new List<UILevelArea>();
   
    Dictionary<Type,UILevelArea> m_areasByType = new Dictionary<Type, UILevelArea>();
    Dictionary<enLevelArea, UILevelArea> m_areasByEnum = new Dictionary<enLevelArea, UILevelArea>();
    Role m_role;
    
    int m_observer;
    
    #endregion

    #region Properties
    public Role Role { get{return m_role;}}
    #endregion

    #region Frame
    
    //初始化时调用
    public override void OnInitPanel()
    {
        m_areasByType.Clear();
        UILevelArea area;
        for(int i=0;i<m_areas.Count;++i)
        {
            area = m_areas[i];
            m_areasByType[area.GetType()] = area;
            m_areasByEnum.Add(area.Type,area);
            area.InitArea(this);
            area.gameObject.SetActive(false);
        }
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        //初始化
        if (RoleMgr.instance.Hero == null)
        {
            Debuger.LogError("当前还没有英雄，战斗界面不能初始化");
            return;
        }

        if (param == null)
            m_cacheOpenArea.Clear();

        m_role = RoleMgr.instance.Hero;
        for (int i = 0; i < m_areas.Count; ++i)
        {
            if (param == null)
            {
                if(m_areas[i].IsOpenOnStart)
                    m_areas[i].OpenArea();

            }
            else
            {
                if (m_cacheOpenArea.Contains(m_areas[i]))
                    m_areas[i].OpenArea(true);
            }
                
        }

        m_observer = m_role.Add(MSG_ROLE.BORN, OnRoleBorn);
        if (m_role.State == Role.enState.alive)
        {
            OnRoleBorn();
        }
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_cacheOpenArea.Clear();
        for (int i = 0; i < m_areas.Count; ++i)
        {
            if (m_areas[i].IsOpen)
            {
                m_cacheOpenArea.Add(m_areas[i]);
                m_areas[i].CloseArea();
            }

        }
        //界面关掉的时候要取消监听
        if(m_observer != EventMgr.Invalid_Id){EventMgr.Remove(m_observer);m_observer =EventMgr.Invalid_Id;}
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {
        for (int i = 0; i < m_areas.Count; ++i)
        {
            if (m_areas[i].IsOpen)
            {
                m_areas[i].UpdateArea();
            }
        }
    }
    #endregion

    #region Private Methods
   
    void OnRoleBorn()
    {
        
        for (int i = 0; i < m_areas.Count; ++i)
        {
            if (m_areas[i].IsOpen)
            {
                m_areas[i].RoleBorn();
            }
        }
    }

    #endregion
    public T Get<T>() where T : UILevelArea
    {
        return (T)m_areasByType.Get(typeof(T));
    }

    public T Open<T>(bool reopen = false) where T : UILevelArea
    {
        if (!this.IsOpen){Debuger.LogError("不能在没有打开UILevel，打开区域");return null;}
        T area = Get<T>();
        area.OpenArea(reopen);
        return area;
    }

    public T Close<T>() where T : UILevelArea
    {
        if (!this.IsOpen) { Debuger.LogError("不能在没有打开UILevel，关闭区域"); return null; }
        T area = Get<T>();
        area.CloseArea();
        return area;
    }

    public UILevelArea Get(enLevelArea en)
    {
        return m_areasByEnum.Get(en);
    }

    void OnTeachAction(string arg)
    {
        var arr = arg.Split(new char[] { ',' });
        switch (arr[0])
        {
            case "clearAllSkillCD":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null || hero.State != Role.enState.alive)
                        return;

                    var part = hero.CombatPart;
                    var enumVals = System.Enum.GetValues(typeof(enSkillType));
                    foreach (var e in enumVals)
                    {
                        var skill = part.GetSkill((enSkillType)e);
                        if (skill == null)
                            continue;
                        skill.CDNeed = 0;
                    }
                }
                break;
            case "forcePlaySkillByType":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null || hero.State != Role.enState.alive)
                        return;
                    if (arr.Length < 2)
                        return;
                    var skillType = (enSkillType)StringUtil.ToInt(arr[1]);
                    var skill = hero.CombatPart.GetSkill(skillType);
                    if (skill == null)
                        return;

                    hero.CombatPart.Play(skill, null, true, true);
                }
                break;
            case "forcePlayComboByType":
                {
                    var hero = RoleMgr.instance.Hero;
                    if (hero == null || hero.State != Role.enState.alive)
                        return;
                    if (arr.Length < 2)
                        return;
                    var skillType = (enSkillType)StringUtil.ToInt(arr[1]);
                    var skill = hero.CombatPart.GetSkill(skillType);
                    if (skill == null)
                        return;

                    hero.CombatPart.PlayCombo(skill);
                }
                break;
        }
    }
}