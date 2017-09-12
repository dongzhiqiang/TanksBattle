using UnityEngine;
using System.Collections;

public class UICorpsInfo : UIPanel
{
    public TextEx m_corpsName;
    public TextEx m_president;
    public TextEx m_corpsId;
    public TextEx m_level;
    public TextEx m_num;
    public TextEx m_power;
    public TextEx m_declare;

    //格子们
    public StateGroup m_GridGroup;


    public override void OnInitPanel()
    {

    }

    public override void OnOpenPanel(object param)
    {
        OtherCorpsRes corpsInfo = (OtherCorpsRes)param;
        m_corpsName.text = corpsInfo.props.name;
        m_president.text = corpsInfo.props.president;
        m_corpsId.text = corpsInfo.props.corpsId.ToString();
        m_level.text = corpsInfo.props.level.ToString();
        m_num.text = corpsInfo.props.memsNum.ToString();
        m_power.text = corpsInfo.corpsPower.ToString();
        m_declare.text = corpsInfo.props.declare;

        int count = corpsInfo.mems.Count;
        m_GridGroup.SetCount(count);
        for(int i = 0; i<count; ++i)
        {
            CorpsMemberItem2 sel = m_GridGroup.Get<CorpsMemberItem2>(i);
            sel.OnSetData(corpsInfo.mems[i]);
        }
    }
    public override void OnClosePanel()
    {
    }
}
