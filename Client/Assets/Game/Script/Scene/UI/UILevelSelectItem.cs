using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelSelectItem : MonoBehaviour
{
    public TextEx levelNum;
    public TextEx levelName;
    public TextEx challengeNum;
    public List<ImageEx> stars;

    public ImageEx nodeIcon;
    public ImageEx selIcon;

    public GameObject open;
    public GameObject close;

    public RoomCfg m_cfg;
    Role m_hero;
    public void Init(RoomCfg roomCfg)
    {
        m_hero = RoleMgr.instance.Hero;
        if (m_hero == null)
            return;

        m_cfg = roomCfg;

        selIcon.gameObject.SetActive(false);

        //没有前置 或者 前置关卡已经通关 则可以进入
        if (m_hero.LevelsPart.CanOpenLevel(m_cfg.id))
        {
            open.gameObject.SetActive(true);
            close.gameObject.SetActive(false);
        }
        else
        {
            open.gameObject.SetActive(false);
            close.gameObject.SetActive(true);
            return;
        }

        LevelInfo levelData = m_hero.LevelsPart.GetLevelInfoById(m_cfg.id);

        levelName.text = m_cfg.roomName;
        if (TimeMgr.instance.IsToday(levelData.lastEnter))
            challengeNum.text = string.Format("{0}/{1}", m_cfg.maxChallengeNum - levelData.enterNum, m_cfg.maxChallengeNum);
        else
            challengeNum.text = string.Format("{0}/{1}", m_cfg.maxChallengeNum, m_cfg.maxChallengeNum);

        List<int> taskIds = m_cfg.GetTaskIdList();
        if (taskIds.Count != 3)
            Debuger.LogError("配置的通关条件不是3个");
        else
        {
            for (int i = 0; i < stars.Count; i++)
            {
                RoomConditionCfg conditionCfg = RoomConditionCfg.GetCfg(taskIds[i]);
                int num = 0;
                if (levelData.starsInfo.TryGetValue(conditionCfg.id.ToString(), out num) && num > 0)
                {
                    stars[i].gameObject.SetActive(true);
                }
                else
                {
                    stars[i].gameObject.SetActive(false);
                }
            }
        }

        if (!levelData.isWin)
        {
            //没通过并且前置关卡已通过
            LevelInfo info = m_hero.LevelsPart.GetLevelInfoById(m_cfg.preLevelId);
            if (m_cfg.preLevelId == "0" || (info != null && info.isWin == true))
                selIcon.gameObject.SetActive(true);
        }
        
        nodeIcon.Set(m_cfg.roomSprite);
    }
}
