using UnityEngine;
using System.Collections;

public class UITaskRewardItem : MonoBehaviour
{
    public ImageEx icon;
    public TextEx num;


    public void init(string iconName, int itemNum)
    {
        icon.Set(iconName);
        float height = icon.rectTransform.sizeDelta.y;
        float width = icon.preferredWidth / icon.preferredHeight * height;
        icon.rectTransform.sizeDelta = new Vector2(width, height);
        //icon.SetNativeSize();
        num.text = itemNum.ToString();
    }

    public void initVitality(int vitality)
    {
        icon.Set("ui_huodong_jiangli_icon_huouedu");
        float height = icon.rectTransform.sizeDelta.y;
        float width = icon.preferredWidth / icon.preferredHeight * height;
        icon.rectTransform.sizeDelta = new Vector2(width, height);
        num.text = vitality.ToString();
    }
}
