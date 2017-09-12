using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EliteLevelScene : LevelScene
{
    public override void SendResult(bool isWin)
    {
        Debug.Log(string.Format("通关时间 : {0}", TimeMgr.instance.logicTime - startTime));
        //单机直接回城
        if (Main.instance.isSingle)
        {
            LevelMgr.instance.GotoMaincity();
            return;
        }

        EliteLevelCfg eliteLevelCfg = (EliteLevelCfg)mParam;

        EndEliteLevelRequestVo request = new EndEliteLevelRequestVo();
        request.isWin = isWin;
        request.levelId = eliteLevelCfg.id;
        request.time = (int)(TimeMgr.instance.logicTime - startTime);      //通关时间
        request.starsInfo = new Dictionary<string, int>();
        List<SceneTrigger> triList = SceneEventMgr.instance.conditionTriggerList;
        for (int i = 0; i < triList.Count; i++)
        {
            RoomConditionCfg cfg = triList[i].GetConditionCfg();
            request.starsInfo.Add(cfg.id + "", triList[i].bReach() ? 1 : 0);
        }

        request.monsterItems = m_monsterItems;
        request.specialItems = m_specialItems;
        request.bossItems = m_bossItems;
        request.boxItems = m_boxItems;

        NetMgr.instance.EliteLevelHandler.SendEndEliteLevel(request);
        bShowLimitTime = false;
    }
}
