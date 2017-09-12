#region Header
/**
 * 名称：UIServerItem
 
 * 日期：2015.11.29
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIServerItem : MonoBehaviour
{
    public ImageEx m_flag;
    public Text m_serverName;

    [HideInInspector]
    public ServerInfo s;

    public void Init(ServerInfo s)
    {
        this.s = s;
        m_flag.Set(s.LoadState == ServerInfo.enLoadState.down ? "ui_guanqia_dian_01" : "ui_guanqia_dian_02");
        m_serverName.text = s.name;
        
    }    
}
