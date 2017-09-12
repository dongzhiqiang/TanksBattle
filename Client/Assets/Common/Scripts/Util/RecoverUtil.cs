using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//帮助计算恢复类道具数量

public class RecoverUtil
{
    /// <summary>
    /// 是不是正在恢复中
    /// </summary>
    /// <param name="recordTime">上一次恢复时记录的时间戳</param>
    /// <param name="curNum">当前记录的数量</param>
    /// <param name="recoverTime">每隔多久恢复1个</param>
    /// <param name="recoverMaxLimit">最多恢复多少个</param>
    /// <returns></returns>
    public static bool IsRecovering(long recordTime, int curNum, int recoverTime,int recoverMaxLimit)
    {
        if(recoverMaxLimit < 0)//说明可以无限恢复
            return true;
                    
        if(curNum >= recoverMaxLimit)//已经满了
            return false;

        var leftTime = recoverTime - recordTime % recoverTime;
        return (recordTime + (recoverMaxLimit - curNum - 1) * recoverTime) + leftTime >= TimeMgr.instance.GetTimestamp();
    }

    /// <summary>
    /// 剩余恢复时间
    /// </summary>
    /// <param name="recordTime">上一次恢复时记录的时间</param>
    /// <param name="recoverTime">每隔多久恢复1个</param>
    /// <returns></returns>
    public static long GetLeftTime(long recordTime, int recoverTime)
    {
        return recoverTime - recordTime % recoverTime;
    }

    /// <summary>
    /// 当前的数量
    /// </summary>
    /// <param name="recordTime">上一次恢复时记录的时间</param>
    /// <param name="curNum">当前记录的数量</param>
    /// <param name="recoverTime">每隔多久恢复1个</param>
    /// <param name="recoverMaxLimit">最多恢复多少个</param>
    /// <returns></returns>
    public static int GetNum(long recordTime, int curNum, int recoverTime, int recoverMaxLimit)
    {
        if (curNum >= recoverMaxLimit)//已经达到恢复上限就不用算了
            return curNum;

        var lastTimeRecover = GetLastTimeRecover(recordTime, recoverTime);
        var totalSec = Mathf.Max(0,TimeMgr.instance.GetTimestamp() - lastTimeRecover);
        
        return Mathf.Min(recoverMaxLimit, (int)totalSec / recoverTime + curNum);
    }

    /// <summary>
    /// 根据记录时间计算上一个恢复点时间
    /// </summary>
    /// <param name="recordTime"></param>
    /// <param name="recoverTime"></param>
    /// <returns></returns>
    public static long GetLastTimeRecover(long recordTime, int recoverTime)
    {
        return recordTime - recordTime % recoverTime;
    }
}
