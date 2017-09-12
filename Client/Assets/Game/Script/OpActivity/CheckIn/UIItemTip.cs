using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class UIItemTip : UIPanel
{
    #region SerializeFields
    public Text m_itemName;
    public ImageEx m_icon;
    public ImageEx m_bg;
    public Text m_description;

    #endregion
    //初始化时调用
    public override void OnInitPanel()
    {
        
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        ItemCfg itemCfg = ItemCfg.m_cfgs[(int)param];
        m_itemName.text = itemCfg.name;
        m_icon.Set(itemCfg.icon);
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        m_bg.Set(qualityCfg.backgroundSquare);

        m_description.text = itemCfg.description;


    }
}
