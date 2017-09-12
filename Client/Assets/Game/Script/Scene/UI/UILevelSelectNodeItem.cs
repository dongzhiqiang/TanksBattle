using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelSelectNodeItem : MonoBehaviour
{
    public TextEx nodeName;
    public TextEx nodeDesc;
    public TextEx nodeProgress;
    public TextEx limitLevel;
    public ImageEx lockImg;
    public ImageEx StarImg;
    public ImageEx linkImg;
    public ImageEx selIcon;

    public RoomNodeCfg m_cfg;
    public void Init(RoomNodeCfg nodeCfg)
    {
        linkImg.gameObject.SetActive(true);
        m_cfg = nodeCfg;
        Role hero = RoleMgr.instance.Hero;
        lockImg.gameObject.SetActive(false);
        selIcon.gameObject.SetActive(false);
        if (hero.LevelsPart.CanOpenNode(m_cfg.id))
        {
            nodeName.text = m_cfg.mapTitle;
            nodeDesc.text = m_cfg.mapName;

            if (hero == null)
                return;

            int curStarsNum = hero.LevelsPart.GetStarsByNodeId(m_cfg.id);
            
            int allStarsNum = 0;
            
            if (RoomCfg.mRoomDict.ContainsKey(m_cfg.id))
                allStarsNum = RoomCfg.mRoomDict[m_cfg.id].Count * 3;

            nodeProgress.text = string.Format("{0}/{1}", curStarsNum, allStarsNum);

            nodeName.gameObject.SetActive(true);
            nodeDesc.gameObject.SetActive(true);
            nodeProgress.gameObject.SetActive(true);

            StarImg.gameObject.SetActive(true);

            lockImg.gameObject.SetActive(false);
        }
        else
        {
            nodeName.gameObject.SetActive(false);
            nodeDesc.gameObject.SetActive(false);
            nodeProgress.gameObject.SetActive(false);
            StarImg.gameObject.SetActive(false);

            lockImg.gameObject.SetActive(true);
            limitLevel.gameObject.SetActive(false);
        }

    }
}
