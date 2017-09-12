using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorpsPart : RolePart
{
    //所在公会的信息
    CorpsInfo m_corpsInfo;
    //个人数据
    CorpsMember m_personalInfo;
    //存储会员数据的字典
    Dictionary<int, CorpsMember> m_membersDic = new Dictionary<int, CorpsMember>();
    //所有公会（只包含基础数据，不包括会员详细信息和日志等）
    List<CorpsProps> m_corpsList = new List<CorpsProps>();
    //公会存储id字典
    Dictionary<int, CorpsProps> m_corpsIdMap = new Dictionary<int, CorpsProps>();
    //公会存储名字字典
    Dictionary<string, CorpsProps> m_corpsNameMap = new Dictionary<string, CorpsProps>();
    //日志字典
    Dictionary<string, List<CorpsLogInfo>> m_logs = new Dictionary<string, List<CorpsLogInfo>>();
    //排序后包含时间的日志列表
    List<CorpsLogTimeInfo> m_logList = new List<CorpsLogTimeInfo>();

    #region Properties
    public override enPart Type { get { return enPart.corps; } }
    public CorpsInfo corpsInfo { get { return m_corpsInfo; } set { m_corpsInfo = value; } }
    public CorpsMember personalInfo { get { if (m_personalInfo == null) m_personalInfo = GetMemberInfo(RoleMgr.instance.Hero.GetInt(enProp.heroId)); return m_personalInfo; } }   //判空一下
    public List<CorpsProps> corpsList { get { return m_corpsList; } }
    public List<CorpsLogTimeInfo> logsList { get { return m_logList; } }

    //自己的建设状态
    public List<int> ownBuildState = new List<int>();  //0今日未建设  1今日已建设

    //公会数据是否初始化一次，记得退出公会时重置成false
    public bool hasInit;

    public bool getHasReq = true;
    //申请过的公会id 记得进入公会后清空
    public List<int> hasReqCorpsIds = new List<int>();
    #endregion

    public override bool OnInit()
    {
        m_corpsInfo = new CorpsInfo();
        return true;
    }
    
    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if (vo.corps == null)
            return;

        corpsInfo = vo.corps;
        InitMemsReqs(corpsInfo.members, corpsInfo.reqs);  //初始化字典和排序
        UpdateAllLog(corpsInfo.logs);   //对日志进行处理、排序
      
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }
    public override void OnClear()
    {
    }
    //初始化成员、申请r人的字典
    public void InitMemsReqs(List<CorpsMember> members, List<CorpsMember> reqs)
    {
        m_corpsInfo.members = members;
        //存进字典
        for(int i = 0,len = m_corpsInfo.members.Count; i<len; i++)
        {
            CorpsMember m = m_corpsInfo.members[i];
            m_membersDic[m.heroId] = m;
        }
        m_corpsInfo.reqs = reqs;
        hasInit = true;
        SortMembersByPos();
        GetPersonalInfo();
    } 
    //更新公会基础数据
    public void UpdateCorpsProps(CorpsProps props)
    {
        m_corpsInfo.props = props;  //更新公会属性数据
    }
    //更新或添加会员数据
    public void UpdateCorpsMembers(List<CorpsMember> members)
    {
        for(int i = 0; i<members.Count; i++)
        {
            CorpsMember data = members[i];
            bool isUpdate = false;
            for(int j = 0; j < m_corpsInfo.members.Count; ++j)
            {
                if(m_corpsInfo.members[j].heroId == data.heroId)   //找到一样的id就更新
                {
                    m_corpsInfo.members[j] = data;           
                    isUpdate = true;
                    break;
                }
            }
            //遍历了一遍发现还是没有的话说明是新的
            if (!isUpdate)
                m_corpsInfo.members.Add(data);

            m_membersDic[data.heroId] = data;
        }
        GetPersonalInfo();
        SortMembersByPos();
    }
    //获取自己在公会会员里的个人数据，存起来
    public void GetPersonalInfo()
    {
        Role role = RoleMgr.instance.Hero;
        if (role != null)  //做一下判空处理，登录的时候获取数据还没有role
            m_personalInfo = GetMemberInfo(role.GetInt(enProp.heroId));
    }
    //获取公会某个人的信息
    public CorpsMember GetMemberInfo(int heroId)
    {
        return m_membersDic.Get(heroId);
    }
    //根据id查找公会
    public CorpsProps GetCorpsById(int corpsId)
    {
        return m_corpsIdMap.Get(corpsId);
    }
    //根据名字查找公会
    public CorpsProps GetCorpsByName(string corpsName)
    {
        return m_corpsNameMap.Get(corpsName);
    }
    
    //根据id更新他的公会职位
    public void UpdatePosById(int heroId, int pos)
    {
        m_membersDic[heroId].pos = pos;
        SortMembersByPos();
    }
    //增加申请人
    public void AddReq(CorpsMember info)
    {
        m_corpsInfo.reqs.Add(info);
    }
    //将指定heroid移除申请
    public void RemoveReqById(int heroId)
    {
        for(int i = 0,len = m_corpsInfo.reqs.Count; i<len;++i)
        {
            if(m_corpsInfo.reqs[i].heroId == heroId)
            {
                m_corpsInfo.reqs.RemoveAt(i);
                break;
            }
        }
    }
    //增加成员
    public void AddMember(CorpsMember info)
    {
        m_corpsInfo.members.Add(info);
        m_membersDic[info.heroId] = info;
        GetPersonalInfo();
        SortMembersByPos();
    }
    //将指定heroid移除出公会
    public void RemoveMemberById(int heroId)
    {
        for (int i = 0, len = m_corpsInfo.members.Count; i < len; ++i)
        {
            if (m_corpsInfo.members[i].heroId == heroId)
            {
                m_corpsInfo.members.RemoveAt(i);
                break;
            }
        }
        m_membersDic.Remove(heroId);
    }
    //设置所有公会数据
    public void SetCorpsList(List<CorpsProps> list)
    {
        m_corpsList = list;
        SortCorpsList();
        //存进字典方便查找
        for (int i = 0, cnt = m_corpsList.Count; i < cnt; i++)
        {
            m_corpsIdMap[m_corpsList[i].corpsId] = m_corpsList[i];
            m_corpsNameMap[m_corpsList[i].name] = m_corpsList[i];
        }
    }
    //退出公会清理数据
    public void ClearCorps()
    {
        m_corpsInfo = new CorpsInfo();
        m_personalInfo = null;
        //if (m_ids.Count > 0)
        //    m_ids.Clear();
        //if (m_ids2.Count > 0)
        //    m_ids2.Clear();
        m_membersDic.Clear();
        hasInit = false;

    }

    //设置日志数据 
    public void UpdateAllLog(List<CorpsLogInfo> logs)
    {
        m_corpsInfo.logs = logs;
    }
    //打开公会日志界面的时候才去排，可以减少每次更新日志的都去排列的措次数
    public void SortLogs()
    {
        //超出上限就截掉多余的
        int logMax = CorpsBaseCfg.Get().logMax;
        if (m_corpsInfo.logs.Count > CorpsBaseCfg.Get().logMax)
            m_corpsInfo.logs = m_corpsInfo.logs.GetRange(0, logMax);

        m_logs.Clear();  //先清理一下，避免重复
        for (int i = 0, len = m_corpsInfo.logs.Count; i < len; ++i)
        {
            long timeStamp = m_corpsInfo.logs[i].time;
            string m = StringUtil.FormatDateTime(timeStamp, "MM月dd日");

            if (m_logs.Get(m) == null)
                m_logs[m] = new List<CorpsLogInfo>();

            m_logs[m].Add(m_corpsInfo.logs[i]);
        }
        m_logList = new List<CorpsLogTimeInfo>();
        foreach (var key in m_logs.Keys)
        {
            m_logList.Add(new CorpsLogTimeInfo(key, m_logs[key]));
        }
    }
    //添加日志
    public void AddLog(CorpsLogInfo newLog)
    {
        m_corpsInfo.logs.Insert(0, newLog); 
    }
    //获取会长数据
    public CorpsMember GetPresident()
    {
        for(int i = 0,len = m_corpsInfo.members.Count; i < len; ++i)
        {
            if (m_corpsInfo.members[i].pos == (int)CorpsPosEnum.President)
                return m_corpsInfo.members[i];
        }
        return null;
    }
    //将原会长职位变更为会员
    public void MakeOldPreCommon()
    {
        for(int i = 0,len = m_corpsInfo.members.Count; i < len; ++i)
        {
            if(m_corpsInfo.members[i].pos == (int)CorpsPosEnum.President)
            {
                m_corpsInfo.members[i].pos = (int)CorpsPosEnum.Common;
                break;
            }
        }
    }
    //检查今天自己是否已经建设过
    public bool CheckTodayHasBuild()
    {
        if (RoleMgr.instance.Hero.GetInt(enProp.corpsId) == 0)
            return false;
        for(int i = 0,len = ownBuildState.Count; i < len; ++i)
        {
            if (ownBuildState[i] == 1)
                return true;
        }
        return false;
    }

    //默认排序 按职务排序
    public void SortMembersByPos()
    {
        m_corpsInfo.members.SortEx((CorpsMember a, CorpsMember b) =>
        {
            int value = a.pos.CompareTo(b.pos);
            if (value == 0)
                value = b.level.CompareTo(a.level);
            if (value == 0)
                value = b.contribution.CompareTo(a.contribution);
            if (value == 0)
                value = CompareLastLogout(a.lastLogout, b.lastLogout);
            return value;
        });
    }
    //按等级
    public void SortMembersByLevel()
    {
        m_corpsInfo.members.SortEx((CorpsMember a, CorpsMember b) =>
        {
            int value = b.level.CompareTo(a.level);
            if (value == 0)
                value = a.pos.CompareTo(b.pos);
            if (value == 0)
                value = b.contribution.CompareTo(a.contribution);
            if (value == 0)
                value = CompareLastLogout(a.lastLogout, b.lastLogout);
            return value;
        });
    }
    //按贡献度排序
    public void SortMembersByContribute()
    {
        m_corpsInfo.members.SortEx((CorpsMember a, CorpsMember b) =>
        {
            int value = b.contribution.CompareTo(a.contribution);
            if (value == 0)
                value = a.pos.CompareTo(b.pos);
            if (value == 0)
                value = b.level.CompareTo(a.level);
            if (value == 0)
                value = CompareLastLogout(a.lastLogout, b.lastLogout);
            return value;

        });
    }
    //按离线时间排序
    public void SortMembersByLastOut()
    {
        m_corpsInfo.members.SortEx((CorpsMember a, CorpsMember b) =>
        {
            int value = CompareLastLogout(a.lastLogout, b.lastLogout);
            if (value == 0)
                value = a.pos.CompareTo(b.pos);
            if (value == 0)
                value = b.level.CompareTo(a.level);
            if (value == 0)
                value = b.contribution.CompareTo(a.contribution);

            return value;
        });
    }
    //在线排序，特殊 在线为0排最先
    int CompareLastLogout(long a, long b)
    {
        if(a == 0)
        {
            return b == 0 ? 0 : -1;
        }
        else
        {
            return b == 0 ? 1 : b.CompareTo(a);
        }
    }


    #region PrivateMethod

    //公会列表信息排序
    void SortCorpsList()
    {
        m_corpsList.SortEx((CorpsProps a, CorpsProps b) =>
        {
            //先排等级再排Id、人数
            int value = b.level.CompareTo(a.level);
            if (value == 0)
                value = a.corpsId.CompareTo(b.corpsId);
            if (value == 0)
                value = b.memsNum.CompareTo(a.memsNum);      
            return value;
        });
    }

    #endregion
}
