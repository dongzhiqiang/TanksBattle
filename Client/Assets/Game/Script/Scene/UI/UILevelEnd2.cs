using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UILevelEnd2Context
{
    public bool moveCamera = true;
    public string rate = ""; //空字符串、C、B、A、S、SS、SSS，空字符串表示非评价
    public List<KeyValuePair<string, string>> desc = new List<KeyValuePair<string,string>>();   //value为null时，就不是title、value左右显示，而是中间显示成一行，value为""，还是title、value
    public List<KeyValuePair<int, int>> items = new List<KeyValuePair<int,int>>();
}

public class UILevelEnd2 : UIPanel
{
    public Transform[] m_bgPanelForRate; //按 空字符串、C、B、A、S、SS、SSS 的放
    public Transform m_uiWidget;
    public float m_delayShowTime = 2.0f;
    public StateGroup m_descGroup;
    public StateGroup m_itemGroup;
    public StateHandle m_btnOK;
    public CameraInfo m_cameraInfo;

    public override void OnInitPanel()
    {
        m_btnOK.AddClick(() =>
        {
            //在主城里就直接关闭，不用跳转
            if (LevelMgr.instance.IsMainCity())
            {
                this.Close();
                return;
            }

            LevelMgr.instance.GotoMaincity();
        });
    }

    private int GetRatePanelIndex(string rate)
    {
        switch(rate)
        {
            case "SSS":
                return m_bgPanelForRate.Length - 1;
            case "SS":
                return m_bgPanelForRate.Length - 2;
            case "S":
                return m_bgPanelForRate.Length - 3;
            case "A":
                return m_bgPanelForRate.Length - 4;
            case "B":
                return m_bgPanelForRate.Length - 5;
            case "C":
                return m_bgPanelForRate.Length - 6;
            default:
                return m_bgPanelForRate.Length - 7;
        }
    }

    public override void OnOpenPanel(object param)
    {
        UILevelEnd2Context info = (UILevelEnd2Context)param;

        var panelIdx = GetRatePanelIndex(info.rate);
        for (var i = 0; i < m_bgPanelForRate.Length; ++i)
            m_bgPanelForRate[i].gameObject.SetActive(i == panelIdx);

        if (info.desc.Count <= 0)
        {
            m_descGroup.gameObject.SetActive(false);
        }
        else
        {
            m_descGroup.gameObject.SetActive(true);
            m_descGroup.SetCount(info.desc.Count);

            for (var i = 0; i < info.desc.Count; ++i)
            {
                var infoItem = info.desc[i];
                UILevelEnd2TextItem ctrlItem = m_descGroup.Get<UILevelEnd2TextItem>(i);
                ctrlItem.Init(infoItem.Key, infoItem.Value);
            }
        }

        if (info.items.Count <= 0)
        {
            m_itemGroup.gameObject.SetActive(false);
        }
        else
        {
            m_itemGroup.gameObject.SetActive(true);
            m_itemGroup.SetCount(info.items.Count);

            for (var i = 0; i < info.items.Count; ++i)
            {
                var infoItem = info.items[i];

                UIItemIcon ctrlItem = m_itemGroup.Get<UIItemIcon>(i);
                ctrlItem.Init(infoItem.Key, infoItem.Value);
            }
        }

        UIMgr.instance.StartCoroutine(DelayShowUIWidget());
    }

    private IEnumerator DelayShowUIWidget()
    {
        m_uiWidget.gameObject.SetActive(false);
        yield return new WaitForSeconds(m_delayShowTime);
        m_uiWidget.gameObject.SetActive(true);
    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
    }

    public void OnLevelEnd(UILevelEnd2Context cxt)
    {
        if (cxt.moveCamera)
        {
            Main.instance.StartCoroutine(CoOnLevelEnd(cxt));
        }
        else
        {
            this.Open(cxt);
        }        
    }

    public IEnumerator CoOnLevelEnd(UILevelEnd2Context cxt)
    {
        //修改主角朝向
        Role hero = RoleMgr.instance.Hero;
        if (hero != null && hero.TranPart != null)
        {
            var camNegForward = -CameraMgr.instance.CurCamera.transform.forward;
            hero.TranPart.SetDir(new Vector3(camNegForward.x, 0, camNegForward.y));
            
            m_cameraInfo.horizontalAngle = hero.transform.rotation.eulerAngles.y + 200;
            m_cameraInfo.refPos = hero.transform.position;
            CameraMgr.instance.Add(m_cameraInfo);
            yield return new WaitForSeconds(m_cameraInfo.durationSmooth);
        }

        this.Open(cxt);
    }
}