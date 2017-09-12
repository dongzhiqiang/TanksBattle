#region Header
/**
 * 名称：UIMyServerItem
 
 * 日期：2015.11.29
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIMyServerItem : MonoBehaviour
{
    public ImageEx m_flag;
    public Text m_serverName;
    public Text m_playerName;

    [HideInInspector]
    public RoleInfo role;

    public void Init(RoleInfo role)
    {
        this.role = role;
        m_flag.Set(role.serverInfo.LoadState == ServerInfo.enLoadState.down ? "ui_guanqia_dian_01" : "ui_guanqia_dian_02"); 
        m_serverName.text = role.serverInfo.name;
        m_playerName.text = role.name;
        
    }
}
