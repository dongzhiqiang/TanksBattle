#region Header
/**
 * 名称：SkillLvValueCfg
 
 * 日期：2016.4.5
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class LvValue
{
    //有可能解析出来是个数，也可能是一个配置，比如30%，30，{up:kratos}，{up:kratos}%
    public static bool TryParse(string s, out  bool isPercent,out float val,out bool isMinus, out SkillLvValueCfg lvValueCfg)
    {
        lvValueCfg = null;
        isPercent = false;
        val = 0;
        isMinus = false;
        if (string.IsNullOrEmpty(s))
            return false;

        int startIdx = 0;
        int endIdx = s.Length - 1;

        //百分比
        isPercent = s[endIdx] == '%';
        if (isPercent)
            endIdx = endIdx - 1;
        
        //值
        if(s[endIdx] != '}')
        {
            if (float.TryParse(s.Substring(startIdx, endIdx - startIdx + 1), out val))
            {
                if (isPercent)
                    val = val / 100f;

                isMinus = val < 0;
                return true;
            }
            else
                return false;
        }
        

        //检查正负号
        if(s[startIdx] == '-')
        {
            isMinus = true;
            startIdx += 1;
        }
        
        if (s[startIdx] != '{' )
            return false;

        //等级值
        startIdx += 1;
        endIdx -= 1;
        s = s.Substring(startIdx, endIdx- startIdx+1);
        lvValueCfg = SkillLvValueCfg.Get(s);

        return lvValueCfg != null;
    }

    //解析文本描述
    public static string ParseText(string str,int lv)
    {
        s_temLv = lv;
        return s_regex.Replace(str, MatchEvaluator);
    }
    static string MatchEvaluator(Match match)
    {
        bool isPercent;
        float val;
        bool isMinus;
        SkillLvValueCfg cfg;
        if (!TryParse(match.ToString(), out isPercent, out val,out isMinus, out cfg))
            return "(error)";

        if (cfg == null)
            return val.ToString();
        SkillLvRateCfg r = SkillLvRateCfg.Get(cfg.prefix, s_temLv);
        if (r == null)
        {
            return "(error2)";
        }
        float v = cfg.value + r.rate * cfg.rate;
        string s = (isPercent ? v / 100f : v).ToString(cfg.DotFormat);
        if (isMinus)
            return "-" + s;
        else
            return s;
    }


    static Regex s_regex = new Regex("\\{\\w*:\\w*\\}");
    static int s_temLv = 0;

    public bool isPercent;
    public float value;
    public SkillLvValueCfg cfg;
    public bool error;
    public bool isMinus;//是不是负数
    

    public bool NeedLv { get { return cfg != null; } }
    
    public LvValue(string s)
    {
        
        if (!TryParse(s, out isPercent, out value, out isMinus, out cfg))
        {
            isPercent = false;
            cfg = null;
            value = 0;
            error = true;
        }
        else
            error = false;
    }
    public float Get()
    {
        if (cfg != null)
        {
            Debuger.LogError("逻辑错误，有配置的情况下获取了值:{0}",cfg.id);
        }

        return value;
    }


    public float GetByLv(int lv )
    {
        if (cfg == null)
            return value;

        SkillLvRateCfg r = SkillLvRateCfg.Get(cfg.prefix,lv);
        if(r == null)
        {
            return 0;
        }
        float v = cfg.value + r.rate * cfg.rate;
        if (isMinus)
            v = -v;
        return isPercent ? v / 100f : v;
    }
    
}

public class SkillLvValueCfg 
{
    static StringBuilder s_parse = new StringBuilder();

    public string id;
    public float value=1;
    public float rate =1;
    public int dot = 0;
    
    public string prefix = null;
    

    public static Dictionary<int, string> s_dotFormats = new Dictionary<int, string>();
    public static Dictionary<string, SkillLvValueCfg> s_cfgs = new Dictionary<string, SkillLvValueCfg>();

    public string DotFormat { get { return s_dotFormats[dot]; } }

    public static void Init()
    {
        s_cfgs = Csv.CsvUtil.Load<string, SkillLvValueCfg>("systemSkill/skillLvValue", "id");
        s_dotFormats.Clear();
        foreach (SkillLvValueCfg c in s_cfgs.Values)
        {
            int idx = c.id.IndexOf(":");
            c.prefix =c.id.Substring(0, idx == -1 ? c.id.Length : idx);
            if(!s_dotFormats.ContainsKey(c.dot))
            {
                //##########.####
                string[] ss = new string[c.dot + 1];
                ss[0] = "#########0.";
                for (int i = 0;i< c.dot;++i)
                    ss[i + 1] = "#";

                s_dotFormats[c.dot] = string.Join("", ss);
            }
        }
    }

    public static SkillLvValueCfg Get(string id)
    {
        SkillLvValueCfg cfg = s_cfgs.Get(id);
        if (cfg == null)
        {
            Debuger.LogError("技能的等级值表找不到id:{0}", id);
            return null;
        }
        return cfg;
    }
    
}
