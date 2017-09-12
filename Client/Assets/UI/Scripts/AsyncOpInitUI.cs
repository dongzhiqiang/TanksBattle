using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AsyncOpInitUI
{
    bool _isDone = false;
    float _progress = 0;
    UIMgr _mgr;
    public bool isDone { get { return _isDone; } }

    public float progress { get { return _progress; } }

    public AsyncOpInitUI(UIMgr mgr)
    {
        _mgr = mgr;
        _mgr.StartCoroutine(CoInit());
    }

    IEnumerator CoInit()
    {
        //先加载loading界面
        _mgr.AddPrefab(Resources.Load<GameObject>("UILoading"));
        yield return 2;

        //加载所有图片
        var resMgr = UIResMgr.instance;
        foreach (var sprite in resMgr.m_sprites)
        {
            if (sprite == null)
                continue;
            _mgr.AddSprite(sprite);
        }
        _progress = 0.1f;
        yield return 0;

        //加载所有界面
        var cfg = Util.LoadJsonFile<UIPanelCfg>("UIPanelCfg");
        List<ResourceRequest> ops = new List<ResourceRequest>();
        foreach (var panelName in cfg.panelNames)
        {
            if (panelName != "UILoading")
                ops.Add(Resources.LoadAsync<GameObject>(panelName));
        }

        int totalCount = ops.Count;
        while (ops.Count != 0)
        {
            for (int i = ops.Count - 1; i >= 0; --i)
            {
                if (ops[i].isDone)
                {
                    _mgr.AddPrefab((GameObject)ops[i].asset);
                    ops.RemoveAt(i);
                }
            }

            _progress = 0.1f + 0.8f * (totalCount - ops.Count) / totalCount;
            yield return 0;
        }


        //需要一直显示的界面
        foreach (var panelType in UIMgr.PANEL_SHOW_ALWAYS)
        {
            _mgr.Open(panelType.ToString());
        }
        UIResMgr.NotAllowInstance = true;
        yield return 0;

        _progress = 1f;
        _isDone = true;
    }
}