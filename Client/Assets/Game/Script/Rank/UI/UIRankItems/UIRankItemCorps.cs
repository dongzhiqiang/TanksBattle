using UnityEngine;
using System.Collections;

public class UIRankItemCorps : UIScrollWrapItem
{
    public UIArtFont m_rank;
    public string m_rankPrefix1;
    public string m_rankPrefix2;
    public TextEx m_name;
    public TextEx m_level;
    public TextEx m_leader;
    public TextEx m_power;
    public StateHandle m_bgHandle;
    public ImageEx m_redBg;

    private int m_corpsId;

    public override void OnInitData(object data)
    {
        if (!(data is RankItemWrapper))
            return;

        var dataWrapper = (RankItemWrapper)data;
        var myRankData = (CorpsRankItem)dataWrapper.data;

        m_rank.m_prefix = dataWrapper.rank < 3 ? m_rankPrefix1 : m_rankPrefix2;
        m_rank.SetNum((dataWrapper.rank + 1).ToString());
        m_name.text = myRankData.name;
        m_level.text = myRankData.level.ToString();
        m_leader.text = myRankData.president;
        m_power.text = myRankData.power.ToString();
        m_corpsId = myRankData.key;

        if (m_bgHandle)
            m_bgHandle.AddClick(OnClickBg);

        var myCorpsId = RoleMgr.instance.Hero.GetInt(enProp.corpsId);
        if (myCorpsId == m_corpsId)
        {
            if (!m_redBg.gameObject.activeSelf)
                m_redBg.gameObject.SetActive(true);
        }
        else
        {
            if (m_redBg.gameObject.activeSelf)
                m_redBg.gameObject.SetActive(false);
        }
    }

    private void OnClickBg()
    {
        NetMgr.instance.CorpsHandler.ReqOtherCorps(m_corpsId);
    }
}
