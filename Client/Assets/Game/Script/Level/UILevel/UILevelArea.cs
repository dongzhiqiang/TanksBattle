#region Header
/**
 * 名称：UILevel的子功能区基类
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public enum enLevelArea{
    min,
    head,//左上角人物头像、武器图标、宠物图标
    joystick,//摇杆、combo和hits
    time,//上边的时间提示
    bossHead,//右上角boss血条
    setting,//关闭和挂机按钮
    gizmos,//方向箭头和非屏幕内的小怪箭头提示
    soulFly,//飞魂值
    condition,//通关条件
    blood,//血条
    wave, //怪物波次
    reward,//奖励计数
    arena,//多人竞技
    monster,//怪物数量
    familyHead,//家人血条
    notice, //关卡一些提示信息
    num,//飘血数字
    max
}
public abstract class UILevelArea : MonoBehaviour
{
    #region Fields
    protected UILevel m_parent;
    #endregion

    #region Properties
    public abstract enLevelArea Type { get; }
    public abstract bool IsOpenOnStart { get; }
    public bool IsOpen { get{return this.gameObject.activeSelf;}}
    public Role Role { get{return m_parent.Role;}}
    #endregion

    #region Frame
    //初始化
    public void InitArea(UILevel parent)
    {
        m_parent = parent;
        OnInitPage();
    }

    //首次初始化的时候调用
    protected abstract void OnInitPage();

    public void OpenArea(bool reopen = false)
    {
        if (this.gameObject.activeSelf)
        {
            Debuger.LogError("重复打开");
            return;
        }

        this.gameObject.SetActive(true);
        OnOpenArea(reopen);
    }

    //显示
    protected abstract void OnOpenArea(bool reopen);

    public void CloseArea()
    {
        if (!this.gameObject.activeSelf)
        {
            Debuger.LogError("重复关闭");
            return;
        }
        this.gameObject.SetActive(false);
        OnCloseArea();
    }

    public void UpdateArea() { OnUpdateArea();}

    protected abstract void OnUpdateArea();
    //关闭
    protected abstract void OnCloseArea();

    public void RoleBorn()
    {
        OnRoleBorn();
    }
    //角色出生
    protected abstract void OnRoleBorn();
    

    #endregion

    #region Private Methods


    #endregion

    
}
