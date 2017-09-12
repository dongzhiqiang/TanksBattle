using UnityEngine;
using System.Collections;

public class SceneAction
{
    public int _idx;
    public float _delay;
    public virtual void OnAction() { }
    public virtual void Init(ActionCfg actionCfg) { _idx = actionCfg._idx; _delay = actionCfg._delay; }

    public virtual void OnRelease() { }
}
