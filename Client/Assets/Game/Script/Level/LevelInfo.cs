using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelOpenState{
    NotOpen,
    NotWin,
    Open,
}

public class LevelInfo
{

    #region Fields
    public bool isWin = false;           //是否通关
    public string roomId;          //关卡ID
    public string nodeId;          //所属节点ID
    public Dictionary<string, int> starsInfo = new Dictionary<string, int>();        //当前关卡星
    public int enterNum = 0; //已经挑战次数
    public long lastEnter = 0;  //最后一次进入时间
    #endregion

    public int GetStars()
    {
        int totalNum = 0;
        foreach(int num in starsInfo.Values)
        {
            totalNum += num;
        }
        return totalNum;
    }

    public LevelOpenState GetLevelOpenState()
    {
        if (roomId == null || nodeId == null)
            return LevelOpenState.NotOpen;

        if (isWin)
            return LevelOpenState.Open;
        else
            return LevelOpenState.NotWin;
    }
}