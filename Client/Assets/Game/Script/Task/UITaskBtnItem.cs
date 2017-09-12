using UnityEngine;
using System.Collections;

public class UITaskBtnItem : MonoBehaviour {
    public TextEx name;
    public ImageEx tip;    
    public int index;

	public  void Init(int index)
    {
        this.index = index;
        if (index==0)
        {            
            name.text = "每日任务";
            
        }
        else if(index ==1)
        {
            name.text = "成长任务";
        }
    }
}
