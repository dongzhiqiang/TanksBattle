using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomNodeCfg
{
    #region Fields
    public string id = "";
    public string mapTitle = "";
    public string mapName = "";
    public int openLevel = 0;       //开启等级
    public string icon = "";
    #endregion

    //这里暂时用列表 RoomNode 表里也是按int类型填的
    public static List<RoomNodeCfg> mRoomNodeList = new List<RoomNodeCfg>();
    public static void Init()
    {
        mRoomNodeList = Csv.CsvUtil.Load<RoomNodeCfg>("room/roomNode");
    }

    public static RoomNodeCfg Get(string nodeId)
    {
        foreach(RoomNodeCfg cfg in mRoomNodeList)
        {
            if (cfg.id == nodeId)
                return cfg;
        }
        return null;
    }

}
