using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TeachAgentSkillFrame : TeachAgent
{
    private string m_roomId;
    private string m_roleId;
    private string m_skillId;
    private int m_frameId;

    private int m_obSceneStart= EventMgr.Invalid_Id;
    private int m_obSceneExit = EventMgr.Invalid_Id;
    private int m_obBorn = EventMgr.Invalid_Id;
    private int m_obDead = EventMgr.Invalid_Id;
    private int m_obSkill= EventMgr.Invalid_Id;
    private int m_roleCnt= 0;

    public TeachAgentSkillFrame(string teachName) : base(teachName)
    {
    }

    public override TeachAgentType Type
    {
        get
        {
            return TeachAgentType.skillFrame;
        }
    }

    public override void Init(string param)
    {
        if (param == null)
        {
            Debuger.LogError("param不能为null");
            return;
        }

        //关卡ID,角色ID,技能ID,技能帧ID
        var arr = param.Split(new char[] { ',' }, 4);
        if (arr.Length < 4)
        {
            Debuger.LogError("param必须能拆成四个参数");
            return;
        }

        m_roomId = arr[0];
        m_roleId = arr[1];
        m_skillId = arr[2];
        m_frameId = StringUtil.ToInt(arr[3]);

        AddSceneStartListener();
        AddSceneExitListener();
    }

    public override void Release()
    {
        RemoveBornListener();
        RemoveDeadListener();
        RemoveSkillListener();
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

    private void AddBornListener()
    {
        RemoveBornListener();
        m_obBorn = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.BORN, OnRoleBorn);
    }

    private void RemoveBornListener()
    {
        if (m_obBorn != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obBorn);
            m_obBorn = EventMgr.Invalid_Id;
        }
    }

    private void AddDeadListener()
    {
        RemoveDeadListener();
        m_obDead = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.DEAD, OnRoleDead);
    }

    private void RemoveDeadListener()
    {
        if (m_obDead != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obDead);
            m_obDead = EventMgr.Invalid_Id;
        }
    }

    private void AddSkillListener()
    {
        RemoveSkillListener();
        m_obSkill = EventMgr.AddAll(MSG.MSG_ROLE, MSG_ROLE.SOURCE_SKILL_EVENT, OnRoleSkill);
    }

    private void RemoveSkillListener()
    {
        if (m_obSkill != EventMgr.Invalid_Id)
        {
            EventMgr.Remove(m_obSkill);
            m_obSkill = EventMgr.Invalid_Id;
        }
    }

    private void OnSceneStart(object param)
    {
        var roomId = (string)param;
        if (roomId == m_roomId)
        {
            AddBornListener();
            AddDeadListener();
        }
    }

    private void OnSceneExit(object param)
    {
        var roomId = (string)param;
        if (roomId == m_roomId)
        {
            RemoveBornListener();
            RemoveDeadListener();
            RemoveSkillListener();
        }
    }

    private void OnRoleBorn(object param)
    {
        var role = (Role)param;
        var roleId = role.GetString(enProp.roleId);
        if (roleId == m_roleId)
        {
            ++m_roleCnt;
            if (m_roleCnt == 1)
                AddSkillListener();
        }
    }

    private void OnRoleDead(object param1, object param2, object param3, EventObserver observer)
    {
        var role = observer.notifier.GetParent<Role>();
        var roleId = role.GetString(enProp.roleId);
        if (roleId == m_roleId)
        {
            m_roleCnt = Math.Max(0, m_roleCnt - 1);
            if (m_roleCnt == 0)
                RemoveSkillListener();
        }
    }

    private void OnRoleSkill(object param1, object param2, object param3, EventObserver observer)
    {
        var skillframe = (SkillEventFrame)param3;
        if (skillframe.Skill == null)
            return;

        if (skillframe.Skill.Cfg.skillId != m_skillId)
            return;

        if (skillframe.Skill.CurFrame != m_frameId)
            return;

        var role = observer.notifier.GetParent<Role>();
        var roleId = role.GetString(enProp.roleId);
        if (roleId != m_roleId)
            return;

        TeachMgr.instance.PlayTeach(m_teachName);
    }
}