#region Header
/**
 * 名称：UILevelAreaHead
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaMonster : UILevelArea
{
    
    #region Fields
    public ImageEx m_monsterImg;
    public TextEx m_monsterCount;
    
    int m_obExit;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.monster; } }
    public override bool IsOpenOnStart { get{return false;} }

    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {

    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        m_obExit = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, OnSceneExit);
    }

    protected override void OnUpdateArea()
    {

    }

    //关闭
    protected override void OnCloseArea()
    {
        if (m_obExit != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obExit);
            m_obExit = EventMgr.Invalid_Id;
        }
    }

    protected override void OnRoleBorn()
    {
       
    }




    public void SetMonsterCount(int monsterCount)
    {
        m_monsterCount.text = monsterCount + "只";
    }

    public void Clear()
    {

    }

    #endregion

    #region Private Methods


    void OnSceneExit()
    {
        Clear();
    }

    #endregion

    
}
