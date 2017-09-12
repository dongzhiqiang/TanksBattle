using UnityEngine;
using System.Collections;

public class ActionShowDir : SceneAction
{
    public ActionCfg_ShowDir mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_ShowDir;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log(string.Format("开始寻路 {0},{1},{2}", mActionCfg.findPos.x, mActionCfg.findPos.y, mActionCfg.findPos.z));


        PoolMgr.instance.GCCollect();//垃圾回收下

        SceneMgr.instance.StartFind(mActionCfg.findPos);
    }
}
