#region Header
/**
 * 名称：属性定义表
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class QTESkillCfg
{
    public string skillId="";
    public float rate = 1f;

    public QTESkillCfg(string param){
        if(string.IsNullOrEmpty(param))
            return;

        string[] pp = param.Split('|');

        if (pp.Length < 1 )
            skillId ="";
        else
            skillId = pp[0];

        bool isPercent;
        if (pp.Length < 2 || !StringUtil.TryParsePercent(pp[1], out isPercent, out rate))
            rate= 1f;
    }
}

public class QTECfg
{
    public int id=0;//状态id
    public string sourceId ="";//释放者角色id
    public string targetId = "";//被释放者角色id
    public List<QTESkillCfg> skills = new List<QTESkillCfg>();//要释放的技能，多个的情况下根据情况选择一个
    public List<QTESkillCfg> skills2 = new List<QTESkillCfg>();//要释放的技能，多个的情况下根据情况选择一个
    public int sourceHp = 100;  //释放者hp限制 百分比
    public int targetHp = 100;  //被释放者hp限制 百分比

    static Dictionary<string, Dictionary<string, QTECfg>> s_cfgs = new Dictionary<string, Dictionary<string, QTECfg>>();
    public static void Init()
    {
        s_cfgs.Clear();

        List<QTECfg> l = Csv.CsvUtil.Load<QTECfg>("systemSkill/qte");
        foreach(QTECfg c in l){
            s_cfgs.GetNewIfNo(c.targetId)[c.sourceId] = c;//第一个索引是target，因为一般查找的时候source都会有的，先找target而不是source可以提升效率
        }

    }
    public static QTECfg Get(string sourceId,string targetId)
    {
        Dictionary<string, QTECfg> dict = s_cfgs.Get(targetId);//第一个索引是target，因为一般调用到这里source都会有的，先找target而不是source可以提升效率
        if (dict == null)
            return null;

        return dict.Get(sourceId);
    }
}

//大qte
public class QTECfg2
{
    public int id = 0;//状态id
    public string sourceId = "";//释放者角色id
    public string targetId = "";//被释放者角色id
    public string cameraMod = "";//相机模型id
    
    public QTESkillCfg skill;
    public int sourceHp = 100;  //释放者hp限制 百分比
    public int targetHp = 100;  //被释放者hp限制 百分比

    static Dictionary<string, Dictionary<string, QTECfg2>> s_cfgs = new Dictionary<string, Dictionary<string, QTECfg2>>();
    public static void Init()
    {
        s_cfgs.Clear();

        List<QTECfg2> l = Csv.CsvUtil.Load<QTECfg2>("systemSkill/bigQte");
        foreach (QTECfg2 c in l)
        {
            s_cfgs.GetNewIfNo(c.targetId)[c.sourceId] = c;//第一个索引是target，因为一般查找的时候source都会有的，先找target而不是source可以提升效率
        }

    }
    public static QTECfg2 Get(string sourceId, string targetId)
    {
        Dictionary<string, QTECfg2> dict = s_cfgs.Get(targetId);//第一个索引是target，因为一般调用到这里source都会有的，先找target而不是source可以提升效率
        if (dict == null)
            return null;

        return dict.Get(sourceId);
    }
}
