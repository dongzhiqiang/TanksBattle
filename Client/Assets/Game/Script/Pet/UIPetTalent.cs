using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIPetTalent : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public StateHandle m_button;
    public Text m_gold;
    public StateHandle m_upgrade;
    public GameObject m_goldObj;
    public Text m_needAdvLv;
    public GameObject m_fx;
    private bool m_eventAdded = false;
    private Talent m_talent;
    private Role m_pet;
    Color m_colorGoldNormal;
    Color m_colorRed = QualityCfg.ToColor("CE3535");
    GrowFxMgr m_fxMgr = new GrowFxMgr();
    static bool m_updateButtonLock = false;

    public bool IsTalentId(string talentId)
    {
        return m_talent != null && m_talent.talentId == talentId;
    }

    public void Init(Role pet, Talent talent)
    {
        m_pet = pet;
        TalentCfg talentCfg = TalentCfg.m_cfgs[talent.talentId];
        TalentPosCfg talentPosCfg = TalentPosCfg.m_cfgs[talent.pos];
        m_name.text = talentCfg.name + " Lv." + talent.level;
        m_icon.Set(talentCfg.icon);
        m_talent = talent;
        if (!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_upgrade.AddClick(OnUpgrade);
            m_eventAdded = true;
        }

        if (pet.GetInt(enProp.advLv) < talentPosCfg.needAdvLv)
        {
            m_goldObj.SetActive(false);
            m_upgrade.gameObject.SetActive(false);
            m_needAdvLv.gameObject.SetActive(true);
            PetAdvLvPropRateCfg advLvCfg = PetAdvLvPropRateCfg.m_cfgs[talentPosCfg.needAdvLv];
            QualityCfg qualityCfg = QualityCfg.m_cfgs[advLvCfg.quality];
            m_needAdvLv.text = "进阶至" + "<color=#" + qualityCfg.color + ">" + qualityCfg.name + (advLvCfg.qualityLevel > 0 ? ("+" + advLvCfg.qualityLevel) : "") + "</color>" + "开启";
        }
        else if (talent.level < talentCfg.maxLevel)
        {
            int costGold = PetTalentLvCfg.GetCostGold(talentCfg.upgradeId, talent.level);
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
            m_needAdvLv.gameObject.SetActive(false);
        }
        else
        {
            m_goldObj.SetActive(false);
            m_upgrade.gameObject.SetActive(false);
            m_needAdvLv.gameObject.SetActive(false);
        }
        m_updateButtonLock = false;
    }

    void OnClick()
    {
        UIMgr.instance.Open<UIPetTalentInfo>(m_talent);
    }

    void OnUpgrade()
    {
        if (m_updateButtonLock)
        {
            //return;
        }
        TalentCfg talentCfg = TalentCfg.m_cfgs[m_talent.talentId];
        PetAdvLvPropRateCfg advLvCfg = PetAdvLvPropRateCfg.m_cfgs[m_pet.GetInt(enProp.advLv)];
        if (m_talent.level >= advLvCfg.maxTalentLv)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_PET, RESULT_CODE_PET.PET_TALENT_OVER_PET_ADV_LV));
            return;
        }
        // check items
        List<CostItem> costItems = PetTalentLvCfg.GetCost(talentCfg.upgradeId, m_talent.level);
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
        NetMgr.instance.PetHandler.SendUpgradePetTalent(m_pet.GetString(enProp.guid), m_talent.talentId);
        m_updateButtonLock = true;
    }

    void Awake()
    {
        m_colorGoldNormal = m_gold.color;
        List<GrowAct> actList1 = new List<GrowAct>();
        actList1.Add(new GrowActFxPlay(m_fx));
        actList1.Add(new GrowActAttrTextShake());
        ///List<GrowAct> actList2 = new List<GrowAct>();
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
