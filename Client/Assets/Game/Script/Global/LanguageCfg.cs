using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LanguageConfig
{
    public string key;
    public string desc;
    public bool needRepeat=false;
}
/// <summary>
/// 客户端语言配置
/// </summary>
public class LanguageCfg
{
    public static Dictionary<string, LanguageConfig> m_cfgs = new Dictionary<string, LanguageConfig>();
    public static void Init()
    {
        Dictionary<string, LanguageConfig> cfgs = Csv.CsvUtil.Load<string, LanguageConfig>("other/languageCfg", "key");

        //检查重复描述
        HashSet<string> set = new HashSet<string>();
        foreach(LanguageConfig c in cfgs.Values)
        {
            if (!c.needRepeat &&set.Contains(c.desc))
            {
                Debug.LogError("存在重复的语言配置描述：" + c.desc);
                return;
            }
            m_cfgs[c.key] = c;
            set.Add(c.desc);
        }

    }
    
    public static string Get(string key)
    {
        LanguageConfig cfg = null;
        if (m_cfgs.TryGetValue(key, out cfg))
            return cfg.desc;
        else
        {
            Debug.LogError("语言配置找不到：" + key);
            return key;
        }
    }

}
