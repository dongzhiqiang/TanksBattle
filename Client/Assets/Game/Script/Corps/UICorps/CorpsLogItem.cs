using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsLogItem : MonoBehaviour
{
    public TextEx m_timeTxt;
    public TextEx m_content;

    public void SetLogData(CorpsLogTimeInfo info)
    {
        m_timeTxt.text = info.time;
        string m = "";
        for(int i = 0,len = info.list.Count; i < len; i++)
        {
            if (i < len - 1)
                m += StringUtil.FormatDateTime(info.list[i].time, "HH:mm") + "    " + string.Format(CorpsLogCfg.Get(info.list[i].id).logDesc, info.list[i].opt) + "\n";
            else
                m += StringUtil.FormatDateTime(info.list[i].time, "HH:mm") + "    " + string.Format(CorpsLogCfg.Get(info.list[i].id).logDesc, info.list[i].opt);
        }
        m_content.text = m.ToString();
    }
}
