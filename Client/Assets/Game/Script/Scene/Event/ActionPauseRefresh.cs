using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionPauseRefresh : SceneAction {

    public ActionCfg_PauseRefresh mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_PauseRefresh;
    }

    public override void OnAction()
    {
        Dictionary<string, RefreshBase> reDic = SceneMgr.instance.refreshNpcDict;
        // 空 暂停所有
        if (string.IsNullOrEmpty(mActionCfg.refreshGroup))
        {
            foreach (RefreshBase re in reDic.Values)
                re.Pause();
        }
        else
        {
            string[] groupIds = mActionCfg.refreshGroup.Split(',');
            for(int i = 0; i < groupIds.Length; i++)
            {
                RefreshBase refresh;
                reDic.TryGetValue(groupIds[i], out refresh);
                if (refresh != null)
                    refresh.Pause();
            }
        }
    }
}
