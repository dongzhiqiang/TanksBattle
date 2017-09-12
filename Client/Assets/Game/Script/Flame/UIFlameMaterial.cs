using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

class MaterialInfo
{
    public int itemId;
    public int num;
    public int order;
}

public class UIFlameMaterial : UIPanel
{
    #region SerializeFields
    public StateGroup m_items;
    //public Text m_flameName;
    public Text m_flameLevel;
    public Text m_flameLevelAdd;
    public Text m_flameAttr;
    public ImageEx m_expBar;
    public Text m_exp;
    public StateHandle m_upgrade;
    public GameObject m_noMaterial;
    public Text m_gold;
    public FlyIconFx m_flyFx;
    public GameObject m_expFx;
    public StateHandle m_close2;
    #endregion
    private Dictionary<int, int> m_useItems = new Dictionary<int, int>();
    private int m_flameId;
    private int m_costGold;
    private Color m_goldColor;
    private Color m_red = QualityCfg.ToColor("CE3535");
    private static Vector3 m_floatPos = new Vector3(0, 200);
    private TimeMgr.Timer m_fxTimer;
    //初始化时调用
    public override void OnInitPanel()
    {
        m_upgrade.AddClick(OnUpgrade);
        m_goldColor=m_gold.color;
        m_close2.AddClick(() =>
        {
            Close();
        });
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_flameId = (int)param;
        m_useItems.Clear();
        RefleshMaterial();
    }

    void RefleshMaterial()
    {
        int addExp = 0;
        List<MaterialInfo> items = new List<MaterialInfo>();
        foreach(FlameMaterialCfg materialCfg in FlameMaterialCfg.m_cfgs.Values)
        {
            if(RoleMgr.instance.Hero.ItemsPart.GetItemNum(materialCfg.id)==0)
            {
                continue;
            }
            int num;
            if( !m_useItems.TryGetValue(materialCfg.id, out num))
            {
                num = 0;
            }
            MaterialInfo info = new MaterialInfo();
            info.itemId = materialCfg.id;
            info.num = num;
            info.order = materialCfg.order;
            addExp += num * materialCfg.exp;
            items.Add(info);
        }
        items.Sort((MaterialInfo info1, MaterialInfo info2) =>
            {
                return info1.order - info2.order;
            });
        m_items.SetCount(items.Count);
        for(int i=0; i<items.Count; i++)
        {
            m_items.Get<UIFlameItemIcon>(i).Init(this, items[i].itemId, items[i].num);
        }
        if(items.Count>0)
        {
            m_noMaterial.SetActive(false);
        }
        else
        {
            m_noMaterial.SetActive(true);
        }

        Flame flame = RoleMgr.instance.Hero.FlamesPart.GetFlame(m_flameId);
        FlameCfg flameCfg = FlameCfg.m_cfgs[m_flameId];
        int level = 0;
        int exp = 0;
        if (flame != null)
        {
            level = flame.Level;
            exp = flame.Exp;
        }

        m_costGold = addExp * flameCfg.costGold;
        m_gold.text = m_costGold.ToString();
        if(m_costGold > RoleMgr.instance.Hero.GetInt(enProp.gold))
        {
            m_gold.color = m_red;
        }
        else
        {
            m_gold.color = m_goldColor;
        }

        int oldLevel = level;
        exp += addExp;
        FlameLevelCfg levelCfg = FlameLevelCfg.Get(m_flameId, level + 1);
        while(levelCfg != null && exp >= levelCfg.exp)
        {
            exp -= levelCfg.exp;
            level++;
            levelCfg = FlameLevelCfg.Get(m_flameId, level + 1);
        }
        if (levelCfg == null)
        {
            m_exp.text = "已满级";
            m_expBar.fillAmount = 1f;
        }
        else
        {
            m_exp.text = exp + "/" + levelCfg.exp;
            float rate = exp / (float)levelCfg.exp;
            if (rate > 1) rate = 1;
            m_expBar.fillAmount = rate;
        }
        if(oldLevel==level)
        {
            m_flameLevel.text = "Lv." + level;
            m_flameLevelAdd.gameObject.SetActive(false);
        }
        else
        {
            m_flameLevel.text = "Lv." + oldLevel;
            m_flameLevelAdd.gameObject.SetActive(true);
            m_flameLevelAdd.text = "+"+(level - oldLevel);
        }


        FlameLevelCfg thisLevelCfg = FlameLevelCfg.Get(m_flameId, oldLevel);
        FlameLevelCfg nextLevelCfg = FlameLevelCfg.Get(m_flameId, level);
        PropValueCfg thisValueCfg = null;
        if(thisLevelCfg!=null)
            thisValueCfg  = PropValueCfg.Get(thisLevelCfg.attributeId);
        PropValueCfg nextValueCfg = null;
        if (nextLevelCfg != null)
            nextValueCfg = PropValueCfg.Get(nextLevelCfg.attributeId);

        string attrName = "";
        string attrValueStr = "";

        int attrLevel = level;
        if (oldLevel == level) attrLevel++;
        if (Flame.GetAddAttr(m_flameId, attrLevel, out attrName, out attrValueStr))
        {
            m_flameAttr.text = "<color=#49D74AFF>" + attrName + "+" + attrValueStr + "</color>";
        }
        else
        {
            m_flameAttr.text = "";
        }

        /*
        if (oldLevel == level)
        {
            m_flameAttr.text = PropTypeCfg.Get(flameCfg.attrLimitCxts[0].prop).name + "：" + (thisValueCfg!=null?thisValueCfg.props.GetFloat(flameCfg.attrLimitCxts[0].prop):0);
        }
        else
        {
            m_flameAttr.text = PropTypeCfg.Get(flameCfg.attrLimitCxts[0].prop).name + "：" + (thisValueCfg != null ? thisValueCfg.props.GetFloat(flameCfg.attrLimitCxts[0].prop) : 0)
                + " <color=#49D74AFF>+" + (nextValueCfg.props.GetFloat(flameCfg.attrLimitCxts[0].prop) - (thisValueCfg != null ? thisValueCfg.props.GetFloat(flameCfg.attrLimitCxts[0].prop) : 0)) + "</color>";
        }
         * */
        
    }

    void OnUpgrade()
    {
        if(RoleMgr.instance.Hero.GetInt(enProp.gold) < m_costGold)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_FLAME, RESULT_CODE_FLAME.FLAME_NO_ENOUGE_GOLD));
            return;
        }
        List<ItemVo> items = new List<ItemVo>();
        foreach(int itemId in m_useItems.Keys)
        {
            ItemVo itemVo = new ItemVo();
            itemVo.itemId = itemId;
            itemVo.num = m_useItems[itemId];
            items.Add(itemVo);
        }
        if (items.Count==0)
        {
            UIMessage.Show("请先添加材料");
            return;
        }
        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);
        NetMgr.instance.FlameHandler.SendUpgradeFlame(m_flameId, items);
    }

    public bool CanIncreaseItem()
    {
        int addExp = 0;
        int num;

        foreach (var materialCfg in FlameMaterialCfg.m_cfgs.Values)
        {
            if (RoleMgr.instance.Hero.ItemsPart.GetItemNum(materialCfg.id) == 0)
            {
                continue;
            }

            if (!m_useItems.TryGetValue(materialCfg.id, out num))
            {
                num = 0;
            }
            addExp += num * materialCfg.exp;
        }

        Flame flame = RoleMgr.instance.Hero.FlamesPart.GetFlame(m_flameId);
        FlameCfg flameCfg = FlameCfg.m_cfgs[m_flameId];
        int level = 0;
        int exp = 0;
        if (flame != null)
        {
            level = flame.Level;
            exp = flame.Exp;
        }

        int oldLevel = level;
        exp += addExp;
        FlameLevelCfg levelCfg = FlameLevelCfg.Get(m_flameId, level + 1);
        while (levelCfg != null && exp >= levelCfg.exp)
        {
            exp -= levelCfg.exp;
            level++;
            levelCfg = FlameLevelCfg.Get(m_flameId, level + 1);
        }
        if (levelCfg == null)
        {
            return false;
        }

        return true;
    }

    public void IncreaseItem( int itemId )
    {
        int num;

        if (!m_useItems.TryGetValue(itemId, out num))
        {
            num = 0;
        }
        m_useItems[itemId] = num+1;
        RefleshMaterial();
        FlameMaterialCfg materialCfg = FlameMaterialCfg.m_cfgs[itemId];
        UIMgr.instance.Get<UITxtFloatPanel>().Show("经验值 + " + materialCfg.exp, m_floatPos);
    }

    public void DecreaseItem(int itemId)
    {
        int num;
        if (!m_useItems.TryGetValue(itemId, out num))
        {
            num = 0;
        }
        m_useItems[itemId] = num - 1;
        RefleshMaterial();
    }

    public void StartIncreaseItem()
    {
        UIMgr.instance.Open<UITxtFloatPanel>();
        UIMgr.instance.Get<UITxtFloatPanel>().StartShow();
        ShowExpFx();
    }

    public void EndIncreaseItem()
    {
        UIMgr.instance.Get<UITxtFloatPanel>().EndShow();
        if (m_fxTimer != null)
        {
            TimeMgr.instance.RemoveTimer(m_fxTimer);
            m_fxTimer = null;
        }
    }

    public void FlyIcon(ImageEx background, ImageEx icon)
    {
        m_flyFx.StartFlyImage2(background, icon, 0.5f);
    }

    void ShowExpFx()
    {
        m_expFx.SetActive(false);
        m_expFx.SetActive(true);
        m_fxTimer = TimeMgr.instance.AddTimer(0.8f, ShowExpFx);
    }
}