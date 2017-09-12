using UnityEngine;
using System.Collections;

//boss出场UI
public class UILevelBossBorn : UIPanel
{

    #region Fields
    public TextEx m_BossName;
    public GameObject m_BornAni;
    #endregion

    #region Properties
    #endregion

    #region Frame
     public override void OnInitPanel()
    {
       
    }

    public override void OnOpenPanel(object param)
    {
        m_BossName.text = param.ToString();
    }

    public override void OnOpenPanelEnd()
    {
        m_BornAni.gameObject.SetActive(true);
    }

    public override void OnClosePanel()
    {
        m_BornAni.gameObject.SetActive(false);
    }


    public override void OnUpdatePanel()
    {

    }

    #endregion

    #region Private Methods

    #endregion
}
