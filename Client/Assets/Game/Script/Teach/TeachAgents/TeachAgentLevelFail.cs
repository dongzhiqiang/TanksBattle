using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TeachAgentLevelFail : TeachAgent
{
    private string m_roomId;
    private string m_checkType;
    private string m_checkParam;

    private bool m_needPlayTeach = false;
    private int m_obSceneStart= EventMgr.Invalid_Id;
    private int m_obSceneExit = EventMgr.Invalid_Id;
    private int m_obLevelFail = EventMgr.Invalid_Id;

    public TeachAgentLevelFail(string teachName) : base(teachName)
    {
    }

    public override TeachAgentType Type
    {
        get
        {
            return TeachAgentType.levelFail;
        }
    }

    public override void Init(string param)
    {
        if (param == null)
        {
            Debuger.LogError("param不能为null");
            return;
        }

        //关卡ID，检查类型，检查参数
        var arr = param.Split(new char[] { ',' }, 3);
        if (arr.Length < 3)
        {
            Debuger.LogError("param必须能拆成三个参数");
            return;
        }

        m_roomId = arr[0];
        m_checkType = arr[1];
        m_checkParam = arr[2];

        AddSceneStartListener();
        AddSceneExitListener();
    }

    public override void Release()
    {
        RemoveFailListener();
        RemoveSceneStartListener();
        RemoveSceneExitListener();
    }

    private void AddSceneStartListener()
    {
        RemoveSceneStartListener();
        m_obSceneStart = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.START, OnSceneStart);
    }

    private void RemoveSceneStartListener()
    {
        if (m_obSceneStart != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obSceneStart);
            m_obSceneStart = EventMgr.Invalid_Id;
        }
    }

    private void AddSceneExitListener()
    {
        RemoveSceneExitListener();
        m_obSceneExit = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, OnSceneExit);
    }

    private void RemoveSceneExitListener()
    {
        if (m_obSceneExit != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obSceneExit);
            m_obSceneExit = EventMgr.Invalid_Id;
        }
    }

    private void AddFailListener()
    {
        RemoveFailListener();
        m_obLevelFail = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.LOSE, OnRoleFail);
    }

    private void RemoveFailListener()
    {
        if (m_obLevelFail != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obLevelFail);
            m_obLevelFail = EventMgr.Invalid_Id;
        }
    }

    private void OnSceneStart(object param)
    {
        var roomId = (string)param;
        if (roomId == m_roomId)
        {
            AddFailListener();
        }
        else if (roomId == LevelMgr.MainRoomID)
        {
            if (m_needPlayTeach)
            {
                m_needPlayTeach = false;
                TeachMgr.instance.PlayTeach(m_teachName);
            }
        }
    }

    private void OnSceneExit(object param)
    {
        var roomId = (string)param;
        if (roomId == m_roomId)
        {
            RemoveFailListener();
        }
    }

    private void OnRoleFail(object param1, object param2, object param3, EventObserver observer)
    {
        switch (m_checkType)
        {
            case "curWeaponElementNot": //当前武器的元素属性如果不是某个属性，就引发指引
                {
                    var targetElementType = StringUtil.ToInt(m_checkParam);
                    var hero = RoleMgr.instance.Hero;
                    var curWeapon = hero.WeaponPart.CurWeapon;
                    if (curWeapon == null)
                        return;
                    if ((int)curWeapon.CurElementType != targetElementType)
                        m_needPlayTeach = true;
                }
                break;
        }
    }
}