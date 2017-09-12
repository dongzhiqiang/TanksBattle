using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIItemInfo : UIPanel
{
    #region SerializeFields
    public Text m_itemName;
    public ImageEx m_icon;
    public Text m_description;
    public Text m_itemNum;
    public ImageEx m_background;
    public GameObject m_qualityObj;
    public Text m_quality;
    public StateGroup m_achieves;
    
    #endregion
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        int itemId = (int)param;
        ItemCfg itemCfg = ItemCfg.m_cfgs[itemId];
        string name = itemCfg.name;
        //if(itemCfg.quality>0)
        //{
            //name = name + "+" + itemCfg.quality;
        //}
        m_itemName.text = name;
        m_itemNum.text = "现有：" + RoleMgr.instance.Hero.ItemsPart.GetItemNum(itemId) + "件";
        m_description.text = itemCfg.description;
        m_icon.Set(itemCfg.icon);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        m_itemName.color = QualityCfg.GetColor(itemCfg.quality);
        m_background.Set(qualityCfg.backgroundSquare);

        if (m_qualityObj != null)
        {
            if (itemCfg.qualityLevel == 0)
            {
                m_qualityObj.SetActive(false);
            }
            else
            {
                m_qualityObj.SetActive(true);
                m_quality.text = "" + itemCfg.qualityLevel;
            }
        }

        if(itemCfg.achieve == null)
        {
            m_achieves.SetCount(0);
        }
        else
        {
            m_achieves.SetCount(itemCfg.achieve.Length);
            for(int i=0;i<itemCfg.achieve.Length; i++)
            {
                m_achieves.Get<UIItemAchieve>(i).Init(itemCfg.achieve[i]);
            }
            
        }
    }

}