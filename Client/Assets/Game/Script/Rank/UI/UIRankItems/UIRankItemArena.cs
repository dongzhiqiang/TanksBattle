using UnityEngine;
using System.Collections;

public class UIRankItemArena : UIScrollWrapItem
{
    public UIArtFont m_rank;
    public string m_rankPrefix1;
    public string m_rankPrefix2;
    public TextEx m_name;
    public ImageEx m_gradeImg;
    public TextEx m_score;
    public RectTransform heroIconRect;
    public ImageEx heroIcon;
    public StateHandle m_bgHandle;
    public ImageEx m_redBg;

    private int m_heroId;
    private string m_heroName;

    public override void OnInitData(object data)
    {
        if (!(data is RankItemWrapper))
            return;

        var dataWrapper = (RankItemWrapper)data;
        var myRankData = (ArenaRankItem)dataWrapper.data;

        m_rank.m_prefix = dataWrapper.rank < 3 ? m_rankPrefix1 : m_rankPrefix2;
        m_rank.SetNum((dataWrapper.rank + 1).ToString());
        m_name.text = myRankData.name;
        var grade = ArenaGradeCfg.GetGrade(myRankData.score);
        var gradeCfg = ArenaGradeCfg.Get(grade);
        m_gradeImg.Set(gradeCfg.nameImg);
        m_score.text = myRankData.score.ToString();
        if (string.IsNullOrEmpty(myRankData.roleId))
            heroIcon.Set(null);
        else
            heroIcon.Set(RoleCfg.GetHeadIcon(myRankData.roleId));
        m_heroId = myRankData.key;
        m_heroName = myRankData.name;

        if (m_bgHandle)
            m_bgHandle.AddClick(OnClickBg);

        var myHeroId = RoleMgr.instance.Hero.GetInt(enProp.heroId);
        if (myHeroId == m_heroId)
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
        var hero = RoleMgr.instance.Hero;
        var myHeroId = hero.GetInt(enProp.heroId);
        if (myHeroId == m_heroId)
            return;

        UIHeroMenu.Show(m_heroName, m_heroId);
        //NetMgr.instance.RoleHandler.RequestHeroInfo(m_heroId);
    }
}
