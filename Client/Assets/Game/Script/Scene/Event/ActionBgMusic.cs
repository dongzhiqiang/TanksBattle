using UnityEngine;
using System.Collections;

public class ActionBgMusic : SceneAction
{
    public ActionCfg_BgMusic mActionCfg;

    public override void Init(ActionCfg actionCfg)
    {
        base.Init(actionCfg);
        mActionCfg = actionCfg as ActionCfg_BgMusic;

        if (mActionCfg.bgmId != 0)
            SoundMgr.instance.PreLoad(mActionCfg.bgmId);
    }

    public override void OnAction()
    {

        if (mActionCfg.bgmId == 0)
            SoundMgr.instance.Stop2DSound(Sound2DType.bgm);
        else
            SoundMgr.instance.Play2DSound(Sound2DType.bgm, mActionCfg.bgmId);

    }
}
