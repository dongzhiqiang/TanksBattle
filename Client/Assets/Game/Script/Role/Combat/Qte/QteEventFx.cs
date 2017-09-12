using UnityEngine;
using System.Collections;

public class QteEventFx : QteEvent
{
    public string fxName = "";
    
    GameObject go;

    public override void Init()
    {

    }

    public override void Start()
    {
        go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(fxName, false);
        if (go == null)
            return;
        go.SetActive(true);

    }
    public override void Update(float time)
    {
    }

    public override void Stop()
    {
        if (go != null)
            FxDestroy.Destroy(go);
    }
}
