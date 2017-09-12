using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LevelEvaluateCfg
{
    public int id;
    public string name;



    public static Dictionary<int, LevelEvaluateCfg> m_cfgs = new Dictionary<int, LevelEvaluateCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, LevelEvaluateCfg>("activity/levelEvaluate", "id");
    }
}