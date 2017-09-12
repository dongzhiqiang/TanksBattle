/*
 * *********************************************************
 * 名称：计算阶段工具
 
 * 日期：2014.10.27
 * 描述：
 * 1.阶段和阶段对应的值为键值对，键值对的数组则构成了阶段列表
 * 2.这里阶段(即键值)永远从0累加，如果希望从1累加，那么可以给阶段0的值配置成0
 * 3.用途主要是阶段属性计算，比如等级、段位、vip等
 * 4.举例，阶段从0开始,阶段表{10,10,10,10}。那么阶段累计表为{10,20,30,40},从0升到1需要10，从0升到2需要20，从2升到3需要30-20=10
 * *********************************************************
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageUtil
{
    
    //从阶段表转换出阶段累计表
    public static List<int> ListToTotalList(List<int> l)
    {
        List<int> totalList = new List<int>();
        if(l.Count == 0)
            return totalList;

        //最后一个阶段的需要值不等于0，那么补上,因为最高阶段的需要值肯定是0
        if(l[l.Count-1] !=0)
            l.Add(0);
        
        //0阶段时，总值为0
        totalList.Add(0);

        //大于0阶段，总值为上个阶段的总值加上个阶段声阶所需值
        for (int i = 1; i < l.Count; ++i)
            totalList.Add(totalList[i-1] + l[i-1]);

        
        return totalList;
    }

    //从阶段累计表转换出阶段表
    public static List<int> TotalListToList(List<int> totalList)
    {
        List<int> l = new List<int>();
        if(totalList.Count == 0)
            return l;

        //如果累计表开始的累计值不为0，那么说明不是0阶段，插入0阶段
        if(totalList[0]!=0)
            totalList.Insert(0,0);
        
        //不是最后一阶段，升级所需值为下一阶段的累计值减当前阶段
        for (int i = 0; i < totalList.Count-1; ++i)
            l.Add(totalList[i + 1] - totalList[i]);

        //最后一个阶段，升阶段所需要的值为0
            l.Add(0);
        return l;
    }

    //从阶段计算出总值
    
    public static void CalcStateToTotalByTotal(bool lastIsFull,List<int> totalList,int stage,int value,out int totalValue,out bool isFull)
    {
        totalValue = totalList[stage]+value;
        if (totalValue >= totalList[totalList.Count - 1])
        {
            totalValue = totalList[totalList.Count - 1];
            isFull = true;
        }
        else
            isFull = false;
            
    }

    //从总值计算出阶段和值
    //lastIsFull见下面GetTopStage()的说明
    public static void CalcTotalToStageByTotal(bool lastIsFull, List<int> totalList, int totalValue, out int stage, out int value, out bool isFull)
    {
        //满阶的情况
        if (totalValue >= totalList[totalList.Count - 1])
        {
            isFull = true;
            stage = GetTopStageByTotal(lastIsFull, totalList);
            value = GetTopStageNeedByTotal(lastIsFull,totalList);
            return;
        }
        
        //不满阶的情况
        isFull = false;
        stage = 0;
        value =0;
        for (int i = totalList.Count - 2; i >= 0; --i)
        {
            if (totalValue >= totalList[i])
            {
                stage = i + 1;
                value = totalValue - totalList[i];
                break;
            }
        }
        
    }

    //从阶段和值和增量计算出阶段和值
    public static void CalcStageByTotal(bool lastIsFull, List<int> totalList, int oldStage, int oldValue, int addValue, out int stage, out int value, out bool isFull)
    {
        int oldTotalValue;
        CalcStateToTotalByTotal(lastIsFull, totalList, oldStage, oldValue + addValue, out oldTotalValue, out isFull);
        CalcTotalToStageByTotal(lastIsFull, totalList, oldTotalValue, out stage, out value, out isFull);
    }

    //获取最高阶段
    //比如list 为0~100，,其中100级的升级经验为0，那么lastIsFull=true则返回99，否则返回100
    public static int GetTopStageByTotal(bool lastIsFull, List<int> totalList)
    {
        if(lastIsFull)
            return totalList.Count - 2;
        else
            return totalList.Count - 1;
    }

    //获取阶段升级所需经验
    public static int GetTopStageNeedByTotal(bool lastIsFull, List<int> totalList)
    {
        if (lastIsFull)
            return totalList[totalList.Count - 2] - totalList[totalList.Count - 3];
        else
            return 0;
    }
}

