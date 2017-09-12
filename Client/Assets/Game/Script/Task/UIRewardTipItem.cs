using UnityEngine;
using System.Collections;

public class UIRewardTipItem : MonoBehaviour {

    public ImageEx icon;
    public ImageEx bg;
    public TextEx name;
    public TextEx num;
    public GameObject text;
    public float oldX = 0;
    Vector3 oldPos;
    
    public void init(int itemId, int itemNum)
    {
        ItemCfg itemCfg = ItemCfg.m_cfgs[itemId];
        QualityCfg qualityCfg = QualityCfg.m_cfgs[itemCfg.quality];
        oldPos = text.GetComponent<RectTransform>().localPosition;
        oldPos.x = oldX;
        text.GetComponent<RectTransform>().localPosition = oldPos;


        icon.Set(itemCfg.icon);
        bg.Set(qualityCfg.backgroundSquare);
        name.text = itemCfg.name;
        num.text = itemNum.ToString();
        if (name.text.Length <= 6)
        {
            float diffience = (6 - name.text.Length) * (name.GetComponent<RectTransform>().sizeDelta.x / 6);
            Vector3 pos = text.GetComponent<RectTransform>().localPosition;
            Vector3 newPos = new Vector3(pos.x - diffience, pos.y, pos.z);
            text.GetComponent<RectTransform>().localPosition = newPos;

        }

    }
}
