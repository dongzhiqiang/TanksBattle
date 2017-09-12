using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UICombatLogItem : MonoBehaviour
{
    private int heroId;

    public TextEx rankVal;
    public ImageEx headImg;
    public TextEx roleName;
    public TextEx timeVal;
    public ImageEx gradeIcon;
    public StateHandle btnView;

    public void Init(UICombatLog.CombatLogItem item)
    {
        heroId = item.opHeroId;
        rankVal.text = item.rank < 0 ? "无排行" : (item.rank + 1).ToString();
        headImg.Set(RoleCfg.GetHeadIcon(item.opRoleId));
        roleName.text = item.opName;
        timeVal.text = StringUtil.FormatTimeSpan2(item.time);
        gradeIcon.Set(item.iconName);
        btnView.AddClick(OnRequestHeroInfo);
        StateHandle state = this.GetComponent<StateHandle>();
        if (item.win)
        {
            state.SetState(item.rank < item.oldRank ? 0 : 1);
        }
        else
        {
            state.SetState(item.rank > item.oldRank ? 3 : 2);
        }
    }

    private void OnRequestHeroInfo()
    {
        NetMgr.instance.RoleHandler.RequestHeroInfo(heroId);
    }
}
