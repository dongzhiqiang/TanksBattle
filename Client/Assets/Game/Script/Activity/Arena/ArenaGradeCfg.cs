using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ArenaGradeCfg
{
    public int grade;
    public string gradeName;
    public string iconName;
    public string nameImg;
    public int minScore;
    public int maxScore;
    public int atkWinScore;
    public int atkCtnWinScore;
    public int dfdLoseScore;   
    public int dayRewardId;
    public int upgradeRewardId;

    public static Dictionary<int, ArenaGradeCfg> m_cfgs = new Dictionary<int, ArenaGradeCfg>();

    public static int GetGrade(int score)
    {
        foreach (var cfg in m_cfgs.Values)
        {
            if (score >= cfg.minScore && score <= cfg.maxScore)
                return cfg.grade;
        }
        return 0;
    }

    public static string GetGradeNameByScore(int score)
    {
        foreach (var cfg in m_cfgs.Values)
        {
            if (score >= cfg.minScore && score <= cfg.maxScore)
                return cfg.gradeName;
        }
        return "";
    }

    public static string GetGradeNameByGrade(int grade)
    {
        ArenaGradeCfg cfg;
        m_cfgs.TryGetValue(grade, out cfg);
        return cfg == null ? "" : cfg.gradeName;
    }

    public static ArenaGradeCfg Get(int grade)
    {
        ArenaGradeCfg cfg;
        m_cfgs.TryGetValue(grade, out cfg);
        return cfg;
    }

    public static string GetIconByScore(int score)
    {
        ArenaGradeCfg cfg;
        m_cfgs.TryGetValue(GetGrade(score), out cfg);
        return cfg == null ? "" : cfg.iconName;
    }

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, ArenaGradeCfg>("activity/arenaGradeCfg", "grade");
    }
}