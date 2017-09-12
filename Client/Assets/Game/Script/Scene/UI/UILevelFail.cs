using UnityEngine;
using System.Collections;

public class UILevelFail : UIPanel
{
    public StateHandle mBtnEnd;

    bool isCanClose = false;

    public override void OnInitPanel()
    {
        mBtnEnd.AddClick(() => { if (isCanClose) LevelMgr.instance.GotoMaincity(); });
    }

    public override void OnOpenPanel(object param)
    {
        isCanClose = false;
        TimeMgr.instance.AddPause();
        StartCoroutine(DelayClose());
    }

    public override void OnClosePanel()
    {
        TimeMgr.instance.ResetPause();
    }

    IEnumerator DelayClose()
    {
        yield return new WaitForSeconds(3);
        isCanClose = true;
        yield return 0;
    }


}
