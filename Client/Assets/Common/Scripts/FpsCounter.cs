using UnityEngine;
using System.Collections;

public class FpsCounter : MonoBehaviour {
    //用于计算帧率
    int curFrameRate = 60;
    int frameCounter = 0;
    float frameTimeCounter = 0f;

   
    // Use this for initialization
    void Start () {
       
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        //做下分辨率适配，以免控件在不同设备上太小
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        //帧率相关
        frameTimeCounter += Time.unscaledDeltaTime;
        ++frameCounter;
        if (frameTimeCounter >= 1f)
        {
            curFrameRate = frameCounter - 1;
            frameCounter = 1;
            frameTimeCounter = frameTimeCounter - 1f;
        }
        GUI.Label(new Rect(150 * s, 0 * s, 300 * s, 30 * s), "帧率:" + curFrameRate);
    }
}
