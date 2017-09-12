using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelAreaFlyFx : UILevelArea
{
    #region Fields
      
    public SimplePool redSoul_pool3d;
    public SimplePool redSoul_pool2d;
    public SimplePool greenSoul_pool3d;
    public SimplePool greenSoul_pool2d;
    public SimplePool blueSoul_pool3d;
    public SimplePool blueSoul_pool2d;
    public SimplePool gold_pool3d;
    public SimplePool gold_pool2d;
    public SimplePool item_pool3d;
    public SimplePool item_pool2d;

    public float rMin=2;
    public float rMax=3;
    
    private List<FlyToUIFx> fx = new List<FlyToUIFx>();
    private List<FlyToUIFx2d> fx2d = new List<FlyToUIFx2d>();
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.soulFly; } }
    public override bool IsOpenOnStart { get { return true; } }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
    
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {       
        fx.Clear();
        fx2d.Clear();
    }

    protected override void OnUpdateArea()
    {

    }

    //关闭
    protected override void OnCloseArea()
    {      
        for (int i = 0; i < fx.Count; i++)
        {
            fx[i].SetEnd();
        }
        for (int i = 0; i < fx2d.Count; i++)
        {
            fx2d[i].SetEnd();
        }
        fx.Clear();
        fx2d.Clear();
    }

    protected override void OnRoleBorn()
    {

    }

    
    
    #endregion

    #region Private Methods
    void PlayItemFly(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = item_pool3d.Get();
        FlyToUIFx flyFx = go.GetComponent<FlyToUIFx>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx>();

        FlyToUIFx.Param cxt = new FlyToUIFx.Param();
        fx.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayItemFly2d(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = item_pool2d.Get();

        FlyToUIFx2d flyFx = go.GetComponent<FlyToUIFx2d>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx2d>();

        FlyToUIFx2d.Param cxt = new FlyToUIFx2d.Param();
        fx2d.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayGoldFly(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = gold_pool3d.Get();

        FlyToUIFx flyFx = go.GetComponent<FlyToUIFx>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx>();

        FlyToUIFx.Param cxt = new FlyToUIFx.Param();
        fx.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayGoldFly2d(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = gold_pool2d.Get();
        FlyToUIFx2d flyFx = go.GetComponent<FlyToUIFx2d>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx2d>();
        FlyToUIFx2d.Param cxt = new FlyToUIFx2d.Param();
        fx2d.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayRedSoulFly(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = redSoul_pool3d.Get();

        FlyToUIFx flyFx = go.GetComponent<FlyToUIFx>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx>();

        FlyToUIFx.Param cxt = new FlyToUIFx.Param();
        fx.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayRedSoulFly2d(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = redSoul_pool2d.Get();

        FlyToUIFx2d flyFx = go.GetComponent<FlyToUIFx2d>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx2d>();

        FlyToUIFx2d.Param cxt = new FlyToUIFx2d.Param();
        fx2d.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }

    void PlayGreenSoulFly(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = greenSoul_pool3d.Get();

        FlyToUIFx flyFx = go.GetComponent<FlyToUIFx>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx>();

        FlyToUIFx.Param cxt = new FlyToUIFx.Param();
        fx.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayGreenSoulFly2d(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = greenSoul_pool2d.Get();

        FlyToUIFx2d flyFx = go.GetComponent<FlyToUIFx2d>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx2d>();

        FlyToUIFx2d.Param cxt = new FlyToUIFx2d.Param();
        fx2d.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }

    void PlayBlueSoulFly(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = blueSoul_pool3d.Get();

        FlyToUIFx flyFx = go.GetComponent<FlyToUIFx>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx>();

        FlyToUIFx.Param cxt = new FlyToUIFx.Param();
        fx.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }
    void PlayBlueSoulFly2d(Vector3 startPos, Vector3 downPos, Transform uiTrans, System.Action onEnd)
    {
        GameObject go = blueSoul_pool2d.Get();
        FlyToUIFx2d flyFx = go.GetComponent<FlyToUIFx2d>();
        if (flyFx == null)
            flyFx = go.AddComponent<FlyToUIFx2d>();
        FlyToUIFx2d.Param cxt = new FlyToUIFx2d.Param();
        fx2d.Add(flyFx);
        cxt.posFrom = startPos;
        cxt.downPos = downPos;
        cxt.uiTrans = uiTrans;
        cxt.onEnd = onEnd;
        flyFx.FxParam = cxt;
        flyFx.PlayNow();
    }

    Vector3 GetDownPos(Vector3 root)
    {
        int times = UnityEngine.Random.Range(0, 360);
        float r = UnityEngine.Random.Range(rMin, rMax);
        float hudu = (2 * Mathf.PI / 360) * 6 * times;
        float X = root.x + Mathf.Sin(hudu) * r;
        float Y = root.z - Mathf.Cos(hudu) * r;
        return new Vector3(X, root.y, Y);
        
    }
    #endregion
   
    public void PlayItemFly(Role roleFrom, Transform uiTrans, int num, System.Action onEnd)
    {
        if (roleFrom == null)
            return;

        Vector3 startPos = roleFrom.TranPart.GetYOff(0.8f);
        Vector3 root = roleFrom.TranPart.GetRoot();
        Vector3 right = CameraMgr.instance.CurCamera.transform.right;
        for (int i = 0; i < num; i++)
        {
            Vector3 downPos = GetDownPos(root);
            PlayItemFly(startPos, downPos, uiTrans, () =>
            {
                PlayItemFly2d(startPos, downPos, uiTrans, onEnd);
            });
        }
    }
        
    public void PlayGoldFly(Role roleFrom, Transform uiTrans, int num, System.Action onEnd)
    {
        if (roleFrom == null)
            return;

        Vector3 startPos = roleFrom.TranPart.GetYOff(0.8f);

        Vector3 root = roleFrom.TranPart.GetRoot();
        Vector3 right = CameraMgr.instance.CurCamera.transform.right;
        for (int i = 0; i < num; i++)
        {
            Vector3 downPos = GetDownPos(root);
            PlayGoldFly(startPos, downPos, uiTrans, () =>
            {
                PlayGoldFly2d(startPos, downPos, uiTrans, onEnd);
            });
        }       
    }

    public void PlayRedSoulFly(Role roleFrom, Transform uiTrans, int num, System.Action onEnd)
    {
        if (roleFrom == null)
            return;

        Vector3 startPos = roleFrom.TranPart.GetYOff(0.8f);

        Vector3 root = roleFrom.TranPart.GetRoot();
        Vector3 right = CameraMgr.instance.CurCamera.transform.right;
        for (int i = 0; i < num; i++)
        {
            Vector3 downPos = GetDownPos(root);
            PlayRedSoulFly(startPos, downPos, uiTrans, () =>
                {                
                    PlayRedSoulFly2d(startPos, downPos, uiTrans, onEnd);
                });
        }
    }

    public void PlayGreenSoulFly(Role roleFrom, Transform uiTrans, int num, System.Action onEnd)
    {
        if (roleFrom == null)
            return;

        Vector3 startPos = roleFrom.TranPart.GetYOff(0.8f);

        Vector3 root = roleFrom.TranPart.GetRoot();
        Vector3 right = CameraMgr.instance.CurCamera.transform.right;
        for (int i = 0; i < num; i++)
        {
            Vector3 downPos = GetDownPos(root);
            PlayGreenSoulFly(startPos, downPos, uiTrans, () =>
            {
                PlayGreenSoulFly2d(startPos, downPos, uiTrans, onEnd);
            });
        }
    }

    public void PlayBlueSoulFly(Role roleFrom, Transform uiTrans, int num, System.Action onEnd)
    {
        if (roleFrom == null)
            return;

        Vector3 startPos = roleFrom.TranPart.GetYOff(0.8f);

        Vector3 root = roleFrom.TranPart.GetRoot();
        Vector3 right = CameraMgr.instance.CurCamera.transform.right;
        for (int i = 0; i < num; i++)
        {
            /* int j = i - num / 2;
             j += j >= 0 ? 2 : -1;
             Vector3 downPos = root + right * j + (new Vector3(0f, 0f, -0.5f));*/
            Vector3 downPos = GetDownPos(root);
            PlayBlueSoulFly(startPos, downPos, uiTrans, () =>
            {
                PlayBlueSoulFly2d(startPos, downPos, uiTrans, onEnd);
            });
        }
    }

    

  
}
