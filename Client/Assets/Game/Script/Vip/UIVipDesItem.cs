using UnityEngine;
using System.Collections;

public class UIVipDesItem : MonoBehaviour {

    public TextEx description;
	public void Init(string text)
    {
        description.text = text;
    }
}
