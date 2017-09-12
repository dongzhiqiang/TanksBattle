using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class PetInfo
{
    public string petId;
    public int star;
    public int level = 1;
    public int advLv = 1;
    public RoleCfg Cfg { get { return RoleCfg.Get(petId); } }
}

public class UIPetInfo : UIPanel
{
    #region SerializeFields
    public List<ImageEx> m_starCovers;
    public Text m_textLevel;
    public Text m_textName;
    public UI3DView m_petMod;
    public Text m_hp;
    public Text m_atk;
    public Text m_def;
    public Text m_damageDef;
    public Text m_damage;
    public Text m_critical;
    public Text m_criticalDef;
    public Text m_criticalDamage;
    public Text m_fire;
    public Text m_fireDef;
    public Text m_ice;
    public Text m_iceDef;
    public Text m_thunder;
    public Text m_thunderDef;
    public Text m_dark;
    public Text m_darkDef;
    public Text m_positioning;
    public Text m_quality;
    public RectTransform m_equipsParent;
    public StateGroup m_equips;
    public StateGroup m_skills;
    public StateGroup m_talents;
    public StateGroup m_bonds;
    public ImageEx m_pieceBar;
    public Text m_pieceNum;
    public StateHandle m_piece;
    public Text m_power;
    #endregion

    private PetInfo m_petInfo;
    private Role m_petObj;
    private bool m_needDestroy = false;

    void SetQuality(Text text, PetAdvLvPropRateCfg cfg)
    {
        text.text = GetQualityText(cfg.quality, cfg.qualityLevel);
        text.color = QualityCfg.GetColor(cfg.quality);
    }

    string GetQualityText(int quality, int qualityLevel)
    {
        string text = QualityCfg.m_cfgs[quality].name;
        if (qualityLevel > 0)
        {
            text = text + "+" + qualityLevel;
        }
        return text;
    }

    //初始化时调用
    public override void OnInitPanel()
    {
        // 暂时！！！m_piece.AddClick(OnPiece);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        if (param is Role)
        {
            m_petObj = (Role)param;
            m_needDestroy = false;

            m_petInfo = new PetInfo();
            m_petInfo.petId = m_petObj.GetString(enProp.roleId);
            m_petInfo.level = m_petObj.GetInt(enProp.level);
            m_petInfo.advLv = m_petObj.GetInt(enProp.advLv);
            m_petInfo.star = m_petObj.GetInt(enProp.star);
        }
        else if (param is FullRoleInfoVo)
        {
            FullRoleInfoVo roleVo = (FullRoleInfoVo)param;
            RoleBornCxt cxt = IdTypePool<RoleBornCxt>.Get();
            cxt.OnClear();
            cxt.roleId = roleVo.props["roleId"].String;

            try
            {
                m_petObj = RoleMgr.instance.CreateNetRole(roleVo, true, cxt);
                m_needDestroy = true;

                m_petInfo = new PetInfo();
                m_petInfo.petId = m_petObj.GetString(enProp.roleId);
                m_petInfo.level = m_petObj.GetInt(enProp.level);
                m_petInfo.advLv = m_petObj.GetInt(enProp.advLv);
                m_petInfo.star = m_petObj.GetInt(enProp.star);
            }
            catch (Exception)
            {
                return;
            }
        }
        else if (param is PetInfo)
        {
            m_petObj = null;
            m_needDestroy = false;

            m_petInfo = (PetInfo)param;
        }
        else
        {
            Debuger.LogError("UIPetInfo，参数类型不对");
            return;
        }

        UpdatePet();
        m_petMod.gameObject.SetActive(false);

        GetComponent<ShowUpController>().Prepare();
    }

    public override void OnClosePanel()
    {
        if (m_petObj != null && m_needDestroy)
        {
            RoleMgr.instance.DestroyRole(m_petObj, false);
            m_petObj = null;
        }
    }

    public void Refresh()
    {
        UpdatePet();
    }


    void UpdatePet()
    {
        for (int i = 0; i < m_starCovers.Count; i++)
        {
            if (i < m_petInfo.star )
            {
                m_starCovers[i].gameObject.SetActive(false);
            }
            else
            {
                m_starCovers[i].gameObject.SetActive(true);
            }
        }
        m_textLevel.text = "Lv."+m_petInfo.level.ToString();
        m_textName.text = m_petObj == null ? m_petInfo.Cfg.name : m_petObj.GetString(enProp.name);
        PetAdvLvPropRateCfg cfg = PetAdvLvPropRateCfg.m_cfgs[m_petInfo.advLv];
        m_textName.color = QualityCfg.GetColor(cfg.quality);
        m_petMod.SetModel(m_petInfo.Cfg.mod, m_petInfo.Cfg.uiModScale);

        /*
        int pieceNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(m_pet.Cfg.pieceItemId);
        float percent = (float)pieceNum / m_pet.Cfg.pieceNum;
        if (percent > 1) percent = 1;
        m_pieceBar.fillAmount = percent;
        m_pieceNum.text = pieceNum + "/" + m_pet.Cfg.pieceNum;
         * */
        RefreshAttribute();
    }

    void RefreshAttribute()
    {
        SetQuality(m_quality, PetAdvLvPropRateCfg.m_cfgs[m_petInfo.advLv]);

        RefreshProps();        
        RefreshEquips();
        RefreshBonds();
        RefreshSkills();
        RefreshTalents();
    }

    void RefreshProps()
    {
        if (m_petObj == null)
        {
            var m_props = new PropertyTable();
            m_petInfo.Cfg.GetBaseProp(m_props, m_petInfo.level, m_petInfo.advLv, m_petInfo.star);//自身成长属性
            m_hp.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.hpMax));
            m_atk.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.atk));
            m_def.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.def));
            m_damageDef.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.damageDef));
            m_damage.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.damage));
            m_critical.text = String.Format("{0:F}", m_props.GetFloat(enProp.critical) * 100) + "%";
            m_criticalDef.text = String.Format("{0:F}", m_props.GetFloat(enProp.criticalDef) * 100) + "%";
            m_criticalDamage.text = String.Format("{0:F}", m_props.GetFloat(enProp.criticalDamage) * 100) + "%";
            m_fire.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.fire));
            m_fireDef.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.fireDef));
            m_ice.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.ice));
            m_iceDef.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.iceDef));
            m_thunder.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.thunder));
            m_thunderDef.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.thunderDef));
            m_dark.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.dark));
            m_darkDef.text = "" + Mathf.RoundToInt(m_props.GetFloat(enProp.darkDef));
            m_positioning.text = m_petInfo.Cfg.positioning;
            m_power.text = "" + m_props.GetInt(enProp.power);
        }
        else
        {
            m_hp.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.hpMax));
            m_atk.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.atk));
            m_def.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.def));
            m_damageDef.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.damageDef));
            m_damage.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.damage));
            m_critical.text = String.Format("{0:F}", m_petObj.GetFloat(enProp.critical) * 100) + "%";
            m_criticalDef.text = String.Format("{0:F}", m_petObj.GetFloat(enProp.criticalDef) * 100) + "%";
            m_criticalDamage.text = String.Format("{0:F}", m_petObj.GetFloat(enProp.criticalDamage) * 100) + "%";
            m_fire.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.fire));
            m_fireDef.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.fireDef));
            m_ice.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.ice));
            m_iceDef.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.iceDef));
            m_thunder.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.thunder));
            m_thunderDef.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.thunderDef));
            m_dark.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.dark));
            m_darkDef.text = "" + Mathf.RoundToInt(m_petObj.GetFloat(enProp.darkDef));
            m_positioning.text = RoleCfg.Get(m_petObj.GetString(enProp.roleId)).positioning;
            m_power.text = "" + m_petObj.GetInt(enProp.power);
        }
    }

    void RefreshEquips()
    {
        if (m_petObj == null)
        {
            m_equipsParent.gameObject.SetActive(false);
            m_equips.SetCount(0);
        }
        else
        {
            var part = m_petObj.EquipsPart;

            m_equipsParent.gameObject.SetActive(true);
            m_equips.SetCount(part.Equips.Count);

            var uiIdx = 0;

            for (var i = enEquipPos.minWeapon; i <= enEquipPos.maxWeapon; ++i)
            {
                Equip equip;
                if (part.Equips.TryGetValue(i, out equip))
                {
                    m_equips.Get<UIEquipIcon>(uiIdx++).Init(equip.EquipId, equip.Level, equip.AdvLv);
                }
            }

            for (var i = enEquipPos.minNormal; i <= enEquipPos.maxNormal; ++i)
            {
                Equip equip;
                if (part.Equips.TryGetValue(i, out equip))
                {
                    m_equips.Get<UIEquipIcon>(uiIdx++).Init(equip.EquipId, equip.Level, equip.AdvLv);
                }
            }
        }
    }

    void RefreshBonds()
    {
        RoleCfg roleCfg = m_petInfo.Cfg;
        m_bonds.SetCount(roleCfg.petBonds.Count);
        for (int i = 0; i < roleCfg.petBonds.Count; i++)
        {
            m_bonds.Get<UIPetBondItem>(i).Init(roleCfg.petBonds[i], roleCfg.id);
        }

    }

    void RefreshSkills()
    {
        if (m_petObj == null)
        {
            List<string> petSkills = new List<string>();
            petSkills.Add(m_petInfo.Cfg.atkUpSkill);
            foreach (string skillId in m_petInfo.Cfg.skills)
            {
                petSkills.Add(skillId);
            }
            m_skills.SetCount(petSkills.Count);
            for (int i = 0; i < petSkills.Count; i++)
            {
                m_skills.Get<UIPetSkillItem>(i).Init(m_petInfo, petSkills[i], 1);
            }
        }
        else
        {
            List<PetSkill> petSkills = new List<PetSkill>();
            var skill = m_petObj.PetSkillsPart.GetPetSkill(m_petObj.Cfg.atkUpSkill);
            if (skill != null)
                petSkills.Add(skill);
            foreach (string skillId in m_petObj.Cfg.skills)
            {
                skill = m_petObj.PetSkillsPart.GetPetSkill(skillId);
                if (skill != null)
                    petSkills.Add(skill);
            }
            m_skills.SetCount(petSkills.Count);
            for (int i = 0; i < petSkills.Count; i++)
            {
                m_skills.Get<UIPetSkillItem>(i).Init(m_petInfo, petSkills[i].skillId, petSkills[i].level);
            }
        }
    }

    void RefreshTalents()
    {
        if (m_petObj == null)
        {
            m_talents.SetCount(m_petInfo.Cfg.talents.Count);
            for (int i = 0; i < m_petInfo.Cfg.talents.Count; i++)
            {
                m_talents.Get<UIPetTalentItem>(i).Init(m_petInfo, m_petInfo.Cfg.talents[i], 1, i);
            }
        }
        else
        {
            List<Talent> talents = new List<Talent>();
            foreach (string talentId in m_petObj.Cfg.talents)
            {
                var talent = m_petObj.TalentsPart.GetTalent(talentId);
                if (talent != null)
                    talents.Add(talent);
            }
            m_talents.SetCount(talents.Count);
            for (int i = 0; i < talents.Count; i++)
            {
                m_talents.Get<UIPetTalentItem>(i).Init(m_petInfo, talents[i].talentId, talents[i].level, talents[i].pos);
            }
        }
    }

    public override void OnOpenPanelEnd()
    {
        m_petMod.ResetRotation();
        m_petMod.gameObject.SetActive(true);
        RoleCfg roleCfg = m_petInfo.Cfg;
        m_petMod.SetModel(roleCfg.mod, roleCfg.uiModScale, true);
        GetComponent<ShowUpController>().Start();
    }



    void OnPiece()
    {
        RoleCfg roleCfg = m_petInfo.Cfg;
        UIMgr.instance.Open<UIItemInfo>(roleCfg.pieceItemId);
    }
}