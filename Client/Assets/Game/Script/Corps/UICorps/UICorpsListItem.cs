using UnityEngine;
using System.Collections;

public class UICorpsListItem : MonoBehaviour {
    //公会等级
    public TextEx m_level;
    //公户名
    public TextEx m_name;
    //公会人数
    public TextEx m_members;
    //公会宣言
    public TextEx m_declare;
    //加入等级
    public TextEx m_joinSet;
    //申请按钮
    public StateHandle m_reqBtn;
    //申请按钮上的文本
    public TextEx m_btnLabel;
    //申请状态
    public StateHandle m_reqState;
    

    bool isInit;
    int corpsId;

    public void OnSetData(CorpsProps props)
    {
        if (!isInit)
            OnInit();
        m_name.text = props.name;
        m_level.text = "Lv."+props.level;
        m_members.text = props.memsNum + "/" + CorpsCfg.Get(props.level).maxMember;
        m_declare.text = props.declare;
        m_joinSet.text = props.joinSetLevel.ToString();
        corpsId = props.corpsId;
        
        int level = RoleMgr.instance.Hero.GetInt(enProp.level);
        if (level < props.level)  //达不到等级要求
        {
            m_reqBtn.GetComponent<ImageEx>().SetGrey(true);
            m_reqBtn.GetComponent<StateHandle>().enabled = false;
            m_btnLabel.text = "申请";
        }
        else
        {
            m_reqBtn.GetComponent<ImageEx>().SetGrey(false);
            m_reqBtn.GetComponent<StateHandle>().enabled = true;
            if (props.joinSet == 0)
                m_btnLabel.text = "加入";
            else
            {
                CorpsPart part = RoleMgr.instance.Hero.CorpsPart;
                if (part.hasReqCorpsIds.Contains(props.corpsId))
                    m_reqState.SetState(1);
                else
                {
                    m_reqState.SetState(0);
                    m_btnLabel.text = "申请";
                }
            }
        }
    }

    public void OnInit()
    {
        m_reqBtn.AddClick(OnReqBtn);
        isInit = true;
    }

    private void OnReqBtn()
    {
        if (RoleMgr.instance.Hero.GetInt(enProp.corpsId) > 0)
        {
            UIMessage.Show("请先退出当前公会");
            return;
        }
        NetMgr.instance.CorpsHandler.ApplyJoinCorps(corpsId, RoleMgr.instance.Hero.GetInt(enProp.heroId));
    }
}
