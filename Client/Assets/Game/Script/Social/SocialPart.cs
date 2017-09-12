using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SocialPart : RolePart {

    #region Fields
    //好友列表 需要排序，所以用数组存
    List<Friend> m_friends = new List<Friend>();
    //申请列表 需要排序，所以用数组存
    List<Friend> m_reqs = new List<Friend>();
    //已领取体力的id数组
    List<int> m_colStms = new List<int>();
    //可领取体力的id数组
    List<UnCollStamina> m_unColStms = new List<UnCollStamina>();
    //赠送过的id数组
    List<int> m_sendStms = new List<int>();
    //推荐好友
    List<Friend> m_recommends = new List<Friend>();
    //客户端记录的申请添加过的列表
    HashSet<string> m_addReqs = new HashSet<string>();

    #endregion

    #region Properties
    public override enPart Type { get { return enPart.social; } }
    public List<Friend> friends { get { return m_friends; } }
    public List<Friend> reqs { get { return m_reqs; } }
    public List<int> colStms { get { return m_colStms; } set { m_colStms = value; } }
    public List<UnCollStamina> unColStms { get { return m_unColStms; } set { m_unColStms = value; } }
    public List<int> sendStms { get { return m_sendStms; } set { m_sendStms = value; } }
    public List<Friend> recommends { get { return m_recommends; } }
    public HashSet<string> addReqs { get { return m_addReqs; }set { m_addReqs = value; } }

    //推荐更新时间
    public long recommendUptime;
    //标记是否首次推荐
    public bool isRecFirst;
    
    

    //记录回到主城是否弹出赠送体力框
    public string sendStamName = "";
    #endregion

    #region Mono Frame
    #endregion

    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        isRecFirst = true;
        return true;
    }

    //网络数据初始化 初始化的时候只会发在线的好友和申请人的详细数据，
    //不在线的需等到服务器从数据库取到数据后再通过消息通知
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if (vo.social == null)
            return;
        //好友
        List<Friend> friends = vo.social.friends;
        if (friends != null && friends.Count > 0)
            m_friends = friends;

        if (m_friends.Count > 0)
            SortFriends();
        //申请
        List<Friend> reqs = vo.social.addReqs;
        if (reqs != null && reqs.Count > 0)
            m_reqs = reqs;

        m_sendStms = vo.social.sendStam;
        m_colStms = vo.social.collStam;
        m_unColStms = vo.social.unCollStam;

        CheckTip();

    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }
    public override void OnClear()
    {
        if (m_friends.Count > 0)
            m_friends.Clear();
        if (m_reqs.Count > 0)
            m_reqs.Clear();
    }

    //更新有变动或者新加的好友信息
    public void UpdateFriends(List<Friend> listData)
    {
        for (int i = 0; i < listData.Count; i++)
        {
            Friend fData = listData[i];
            bool isUpdate = false;
            for (int j = 0; j < m_friends.Count; j++)
            {
                if (m_friends[j].heroId == fData.heroId)
                {
                    //找到id一样的就更新数据
                    m_friends[j] = fData;
                    isUpdate = true;
                    break;
                }
            }
            //遍历了一遍发现还是没有的话说明是新的
            if (!isUpdate)
                m_friends.Add(fData);
        }
        SortFriends();
    }
    //添加好友
    public void AddFriend(Friend f)
    {
        bool isFriend = false;
        for(int i=0;i<m_friends.Count;i++)
        {
            if(m_friends[i].heroId == f.heroId)//已经是好友了
            {
                isFriend = true;
                break;
            }
        }
       if(!isFriend)
        {
            m_friends.Add(f);
            SortFriends();
        }
        //从推荐列表中移除
        RemoveRecommendById(f.heroId);
    }
    //删除好友
    public void RemoveFriend(int heroId)
    {
        for (int i = 0; i < m_friends.Count; i++)
        {
            if (m_friends[i].heroId == heroId)
            {
                m_friends.RemoveAt(i);
                break;
            }
        }
    }

    //添加申请人
    public void AddReqList(Friend f)
    {
        for (int i = 0, count = m_reqs.Count; i < count; i++)
        {
            if (m_reqs[i].heroId == f.heroId)  //存在申请人
                return;
        }
        if (m_reqs.Count >= ConfigValue.GetInt("friendReqMax"))//超出最大申请数
            m_reqs.RemoveAt(m_reqs.Count - 1);  //移掉最早的

        m_reqs.Add(f);
        SystemMgr.instance.SetTip(enSystem.social, true);  //叹号提示

    }
    //移除申请人
    public void RemoveReq(Friend f)
    {
        for (int i = 0; i < m_reqs.Count; i++)
        {
            if (m_reqs[i].heroId == f.heroId)
            {
                m_reqs.RemoveAt(i);
                break;
            }
        }
        CheckTip();
    }
    //拒绝所有申请
    public void RemoveAllReq()
    {
        m_reqs.Clear();
        CheckTip();
    }
    //设置推荐好友
    public void SetRecommends(List<Friend> value)
    {
        m_recommends = new List<Friend>();
        for (int i = 0, len = value.Count; i < len; ++i)
        {
            if (!IsFriendById(value[i].heroId))   //推荐好友1小时才刷新一次，这里防止加了好友的又显示出来，做个筛选
                recommends.Add(value[i]);
        }
    }

    public void RemoveRecommendById(int heroId)
    {
        for (int i = 0, len = m_recommends.Count; i < len; ++i)
        {
            if (m_recommends[i].heroId == heroId)
            {
                m_recommends.RemoveAt(i);
                break;
            }
        }
    }

    //根据名字判断是不是自己的好友
    public bool IsFriendByName(string name)
    {
        for (int i = 0; i < m_friends.Count; i++)
        {
            if (m_friends[i].name == name)
                return true;
        }
        return false;
    }
    //根据heroId判断是不是自己的好友
    public bool IsFriendById(int heroId)
    {
        for (int i = 0; i < m_friends.Count; i++)
        {
            if (m_friends[i].heroId == heroId)
                return true;
        }
        return false;
    }
    public void AddSendStm(int heroId)
    {
        m_sendStms.Add(heroId);
    }
    public void RemoveSendStm(int heroId)
    {
        m_sendStms.Remove(heroId);
    }

    public void AddColStm(int heroId)
    {
        RemoveUnColStm(heroId);
        m_colStms.Add(heroId);

    }
    public void RemoveColStm(int heroId)
    {
        m_colStms.Remove(heroId);
    }
    public void AddUnColStm(int heroId, long timeStamp)
    {
        m_unColStms.Add(new UnCollStamina(heroId, timeStamp));
        SystemMgr.instance.SetTip(enSystem.social, true);  //叹号提示
    }
    public void RemoveUnColStm(int heroId)
    {
        for (int i = 0; i < m_unColStms.Count; i++)
        {
            if (m_unColStms[i].heroId == heroId)
            {
                m_unColStms.RemoveAt(i);
                break;
            }
        }
        CheckTip();
    }
    //检查主城图标的叹号是否需要显示
    public void CheckTip()
    {
        //检测主城邮件图标叹号是否显示
        if (m_reqs.Count == 0 && (m_unColStms.Count == 0 || m_colStms.Count >= ConfigValue.GetInt("maxGetFriendStam")))
            SystemMgr.instance.SetTip(enSystem.social, false);
        else
            SystemMgr.instance.SetTip(enSystem.social, true);

        UIFriend ui = UIMgr.instance.Get<UIFriend>();
        //检测申请标签叹号提示
        if (m_reqs.Count > 0)
            ui.SetReqTip(true);
        else
            ui.SetReqTip(false);
    }
    //检测是否需要弹出别人赠送体力的提示
    public void CheckStamDlg()
    {
        if(!string.IsNullOrEmpty(sendStamName))
        {
            //UIMessageBox.Open(string.Format(LanguageCfg.Get("some_send_stamina"), sendStamName), () =>
            //{
            //    UIFriend u = UIMgr.instance.Get<UIFriend>();
            //    if (!u.IsOpen)
            //        UIMgr.instance.Open<UIFriend>();
            //}, null, "前往");

            UIMessage.ShowFlowTip("some_send_stamina", sendStamName);
            sendStamName = "";
        }
    }
    //从role创建好友数据结构
    public Friend MakeFriendDataByRole(Role role)
    {
        Friend f = new Friend();
        f.heroId = role.GetInt(enProp.heroId);
        f.level = role.GetInt(enProp.level);
        f.name = role.GetString(enProp.name);
        f.powerTotal = role.GetInt(enProp.powerTotal);
        f.roleId = role.GetString(enProp.roleId);
        return f;
    }
    
    #region Private Methods

    //对好友进行优先级排序
    void SortFriends()
    {
        m_friends.SortEx((Friend a, Friend b) =>
        {
            //登录时间、战力、等级、heroId
            int value = a.lastLogout.CompareTo(b.lastLogout);
            if (value == 0)
                value = b.powerTotal.CompareTo(a.powerTotal);
            if(value ==0)
                value = b.level.CompareTo(a.level);
            if (value == 0)
                value = a.heroId.CompareTo(b.heroId);
            return value;
        });
    }
    
    #endregion
}
