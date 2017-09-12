using UnityEngine;
using System.Collections;

public class ActionStory : SceneAction
{

    public ActionCfg_Story mActionCfg;
    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_Story;
    }

    public override void OnAction()
    {
        if (SceneMgr.SceneDebug)
            Debug.Log(string.Format("触发剧情 : {0}", mActionCfg.storyId));

        StoryMgr.instance.PlayStory(mActionCfg.storyId);
    }
}