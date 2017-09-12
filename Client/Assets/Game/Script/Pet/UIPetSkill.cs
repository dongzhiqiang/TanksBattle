using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetSkill : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public StateHandle m_button;
    public Text m_gold;
    public StateHandle m_upgrade;
    public GameObject m_goldObj;
    public Text m_needStar;
    public GameObject m_fx;
    private bool m_eventAdded = false;
    private PetSkill m_petSkill;
    private Role m_pet;
    Color m_colorGoldNormal;
    Color m_colorRed = QualityCfg.ToColor("CE3535");
    GrowFxMgr m_fxMgr = new GrowFxMgr();
    static bool m_updateButtonLock = false;

    public bool IsSkillId(string skillId)
    {
        return m_petSkill != null && m_petSkill.skillId == skillId;
    }

    public void Init(Role pet, PetSkill petSkill)
    {
        m_pet = pet;
        m_petSkill = petSkill;
        RoleSystemSkillCfg roleSkillCfg = RoleSystemSkillCfg.Get(m_pet.Cfg.id, m_petSkill.skillId);
        SystemSkillCfg skillCfg = SystemSkillCfg.Get(m_pet.Cfg.id, m_petSkill.skillId);
        m_name.text = roleSkillCfg.name + " Lv." + petSkill.level;
        m_icon.Set(skillCfg.icon);
        
        if (!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_upgrade.AddClick(OnUpgrade);
            m_eventAdded = true;
        }

        if (pet.GetInt(enProp.star) < roleSkillCfg.needPetStar)
        {
            m_goldObj.SetActive(false);
            m_upgrade.gameObject.SetActive(false);
            m_needStar.gameObject.SetActive(true);
            m_needStar.text = "升星至<color=#d5aa64>" + roleSkillCfg.needPetStar + "星</color>开启";
        }
        else if (petSkill.level < ConfigValue.GetInt("maxSkillLevel"))
        {
            int costGold = SkillLvCostCfg.GetCostGold(roleSkillCfg.levelCostId, petSkill.level);
            m_gold.text = costGold.ToString();
            if (costGold > RoleMgr.instance.Hero.GetInt(enProp.gold))
            {
                m_gold.color = m_colorRed;
            }
            else
            {
                m_gold.color = m_colorGoldNormal;
            }
            m_goldObj.SetActive(true);
            m_upgrade.gameObject.SetActive(true);
            m_needStar.gameObject.SetActive(false);
        }
        else
        {
            m_goldObj.SetActive(false);
            m_upgrade.gameObject.SetActive(false);
            m_needStar.gameObject.SetActive(false);
        }
        m_updateButtonLock = false;
    }

    void OnClick()
    {
        UIMgr.instance.Open<UIPetSkillInfo>(new object[]{m_pet.GetString(enProp.roleId),m_petSkill});
    }

    void OnUpgrade()
    {
        if (m_updateButtonLock)
        {
            //return;
        }
        RoleSystemSkillCfg roleSkillCfg = RoleSystemSkillCfg.Get(m_pet.Cfg.id, m_petSkill.skillId);
        if(m_petSkill.level>=m_pet.GetInt(enProp.level))
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_PET, RESULT_CODE_PET.PET_SKILL_OVER_PET_LV));
            return;
        }
        // check items
        List<CostItem> costItems = SkillLvCostCfg.GetCost(roleSkillCfg.levelCostId, m_petSkill.level);
        int needItemId;
        if (!RoleMgr.instance.Hero.ItemsPart.CanCost(costItems, out needItemId))
        {
            ItemCfg itemCfg = ItemCfg.m_cfgs[needItemId];
            if (itemCfg.type == ITEM_TYPE.ABSTRACT_ITEM)
            {
                UIMessage.Show("所需" + itemCfg.name + "不足");
            }
            else
            {
                UIMessage.Show("所需材料" + itemCfg.name + "不足");
            }
            return;
        }
        NetMgr.instance.PetHandler.SendUpgradePetSkill(m_pet.GetString(enProp.guid), m_petSkill.skillId);
        m_updateButtonLock = true;
    }

    void Awake()
    {
        m_colorGoldNormal = m_gold.color;
        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFxPlay(m_fx));
        actList1.Add(new GrowActAttrTextShake());
        //List<GrowAct> actList2 = new List<GrowAct>();
        //actList2.Add(new GrowActFx("fx_ui_zhuangbei_shengjichengong", new Vector3(0, 200, 0)));

        m_fxMgr.m_actList.Add(new GrowActBatch(actList1, 0.2f));
        //m_fxMgr.m_actList.Add(new GrowActBatch(actList2, 0.5f));
        m_fxMgr.m_attrAnis.Add(m_gold.GetComponent<SimpleHandle>());
        m_fxMgr.m_callback = () => UIMgr.instance.Get<UIPet>().Refresh();
    }

    public void StartFx()
    {
        m_fxMgr.Start();
    }
}
