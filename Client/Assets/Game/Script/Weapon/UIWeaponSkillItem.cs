using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIWeaponSkillItem : MonoBehaviour {

    public ImageEx icon;
    public TextEx skillName;
    //public TextEx comboLimit;
    public TextEx skillDesc;
    public TextEx cost;
    public StateHandle btn;
    public StateHandle iconBtn;
    public GameObject m_fx;
    WeaponSkill skill=null;
    float m_btnTimeInterval = 1f;
    float m_lastClickTime = -1;
    GrowFxMgr m_fxMgr = new GrowFxMgr();
    static bool m_updateButtonLock = false;

    public WeaponSkill GetSkill() { return skill; }

    public void Init(WeaponSkill s)
    {
        this.skill = s;
        var roleSkillCfg =s.RoleSkillCfg;
        var skillCfg = s.SkillCfg;

        if (roleSkillCfg != null && skillCfg != null)
        {
            //计算下连击限制
            int comboLimitCount;
            int limitLv;
            s.GetComboLimit(out comboLimitCount, out limitLv);

            icon.Set(skillCfg == null ? null : skillCfg.icon);
            skillName.text = string.Format("{0} Lv.{1}", roleSkillCfg.name, s.lv);
            //comboLimit.text = limitLv == -1 ? "" : string.Format("Lv.{0}解锁{1}连击", limitLv, comboLimitCount + 1);
            skillDesc.text = LvValue.ParseText(roleSkillCfg.briefDesc, s.lv);
            bool isMax = s.lv >= ConfigValue.GetInt("maxSkillLevel");
            int costGold = isMax || roleSkillCfg == null ? 0 : SkillLvCostCfg.GetCostGold(roleSkillCfg.levelCostId, s.lv);
            cost.text = costGold.ToString();
        }
        else
        {
            icon.Set(null);
            skillName.text = "";
            //comboLimit.text = "";
            skillDesc.text = "";
            cost.text = "";
        }
        btn.AddClick(OnClick);
        iconBtn.AddClick(OnIconClick);
        m_updateButtonLock = false;
    }

    void OnClick()
    {
        //if (m_lastClickTime != -1 && TimeMgr.instance.realTime - m_lastClickTime < m_btnTimeInterval)
        //    return;
        m_lastClickTime = TimeMgr.instance.realTime;
        //已经满级
        if (skill.lv >= ConfigValue.GetInt("maxSkillLevel"))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.LEVEL_MAX);
            return;
        }

        //不能超过角色等级
        if (skill.lv >= RoleMgr.instance.Hero.GetInt(enProp.level))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.ROLE_LEVEL_LIMIT);
            return;
        }

        //钱不够
        var roleSkillCfg = skill.RoleSkillCfg;
        List<CostItem> costItems = SkillLvCostCfg.GetCost(roleSkillCfg.levelCostId, skill.lv);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            UIMessage.ShowError(MODULE.MODULE_WEAPON, ResultCodeWeapon.NO_ENOUGH_GOLD);
            return;
        }

        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);
        NetMgr.instance.WeaponHandler.SendSkillUp(skill);
        m_updateButtonLock = true;
    }

    void OnIconClick()
    {
        UIMgr.instance.Open<UIWeaponSkillInfo>(skill);
    }

    void Awake()
    {
        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFxPlay(m_fx));
        actList1.Add(new GrowActAttrTextShake());

        m_fxMgr.m_actList.Add(new GrowActBatch(actList1, 0.2f));
        m_fxMgr.m_attrAnis.Add(cost.GetComponent<SimpleHandle>());
        m_fxMgr.m_callback = () => {
            UIWeapon ui = UIMgr.instance.Get<UIWeapon>();
            if (ui.IsOpen)
                ui.Refresh();



        };
    }

    public void StartFx()
    {
        m_fxMgr.Start();

        var roleSkillCfg = skill.RoleSkillCfg;
        var skillCfg = skill.SkillCfg;

        if (roleSkillCfg != null && skillCfg != null)
        {

            skillName.text = string.Format("{0} Lv.{1}", roleSkillCfg.name, skill.lv);

        }
    }
}
