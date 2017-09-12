using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CostItem
{
    public int itemId;
    public int num;

    static List<int> s_tem = new List<int>(); 
    public void Init(string[] pp)
    {
        StringUtil.TryParse(pp, ref s_tem);
        if (s_tem.Count >= 1)
            itemId = s_tem[0];
        else
            itemId = 0;
        if (s_tem.Count >= 2)
            num = s_tem[1];
        else
            num = 0;
    }
}