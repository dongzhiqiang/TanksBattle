using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// 申请界面
/// </summary>
public class FriReqPage : MonoBehaviour {
    //格子们
    public StateGroup m_GridGroup;
    //滚动区域
    public ScrollRect m_ScrollView;
    //清空按钮
    public StateHandle m_clearBtn;
    //箭头
    public ImageEx m_moreArrow;

    public void OnInit()
    {
        //一键拒绝好友
        m_clearBtn.AddClick(OnClearBtn);
        //监听滚动值的变化才执行
        m_ScrollView.onValueChanged.AddListener(OnScrollChanged);
    }

    void OnClearBtn()
    {
        Role role = RoleMgr.instance.Hero;
        if (role.SocialPart.reqs.Count > 0)  //请求数大于0才发请求
            NetMgr.instance.SocialHandler.SendHandleFriend(role.GetString(enProp.name), 0, (int)HandleFriendType.ResuseAll);
    }

    //更新界面数据
    public void OnUpdateData()
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        int count = socialPart.reqs.Count;
        m_GridGroup.SetCount(count);
        m_moreArrow.gameObject.SetActive(count >= 5 ? true : false);
        for (int i = 0; i < count; i++)
        {
            UIFriendReqItem sel = m_GridGroup.Get<UIFriendReqItem>(i);
            sel.OnSetData(socialPart.reqs[i]);
        }
    }

    public void ResetView()
    {
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_ScrollView, 0); });
    }

    void OnScrollChanged(Vector2 v)
    {
        SocialPart socialPart = RoleMgr.instance.Hero.SocialPart;
        m_moreArrow.gameObject.SetActive(socialPart.reqs.Count >= 5 && m_ScrollView.verticalNormalizedPosition > 0.02f ? true : false);
    }
}
