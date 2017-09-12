using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UIGoldLevelItem : MonoBehaviour
{
    public int m_mode;
    public StateHandle m_btnEnter;
    public StateHandle m_btnSweep;

    public void Init()
    {
        m_btnEnter.AddClick(OnEnter);
        m_btnSweep.AddClick(OnSweep);
    }

    public void SetState(int maxGoldLvMode)
    {
        if (m_mode == maxGoldLvMode)
            this.GetComponent<StateHandle>().SetState(2);
        else if (m_mode == maxGoldLvMode + 1)
            this.GetComponent<StateHandle>().SetState(3);
        else if (m_mode > maxGoldLvMode + 1)
            this.GetComponent<StateHandle>().SetState(0);
        else
            this.GetComponent<StateHandle>().SetState(1);
    }

    private bool CheckAccess(bool tryEnter)
    {
        GoldLevelBasicCfg basicCfg = GoldLevelBasicCfg.Get();
        GoldLevelModeCfg modeCfg = GoldLevelModeCfg.Get(m_mode);
        if (modeCfg == null)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return false;
        }

        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;

        long goldLvlTime = part.GetLong(enActProp.goldLvlTime);
        int goldLvlCnt = part.GetInt(enActProp.goldLvlCnt);
        int goldLvlMax = part.GetInt(enActProp.goldLvlMax);

        if (!TimeMgr.instance.IsToday(goldLvlTime))
            goldLvlCnt = 0;

        if (goldLvlCnt >= basicCfg.dayMaxCnt)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.DAY_MAX_CNT));
            return false;
        }

        long curTime = TimeMgr.instance.GetTimestamp();
        long timePass = curTime >= goldLvlTime ? curTime - goldLvlTime : goldLvlTime - curTime;

        if (timePass < basicCfg.coolDown)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.IN_COOL_DOWN));
            return false;
        }

        if (tryEnter)
        {
            if (m_mode > goldLvlMax + 1)
            {
                UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.PRE_MODE_WRONG));
                return false;
            }
        }
        else
        {
            if (m_mode != goldLvlMax)
            {
                UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.CAN_NOT_SWEEP));
                return false;
            }
        }

        int roleLv = hero.GetInt(enProp.level);
        if (roleLv < modeCfg.openLevel)
        {
            UIMessage.Show(string.Format("{0}级才可以进入", modeCfg.openLevel));
            return false;
        }

        return true;
    }

    private void OnEnter()
    {
        if (CheckAccess(true))
            NetMgr.instance.ActivityHandler.SendEnterGoldLevel(m_mode);
    }

    private void OnSweep()
    {
        GoldLevelModeCfg modeCfg = GoldLevelModeCfg.Get(m_mode);
        if (modeCfg == null)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return;
        }

        RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(modeCfg.roomId);
        if (roomCfg == null)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return;
        }

        int roleLv = roomCfg.levelLv;
        string roleId = modeCfg.monsterId;
        PropertyTable props = new PropertyTable();
        RoleCfg.GetBasePropByCfg(roleId, props, roleLv);

        int hpMax = (int)props.GetFloat(enProp.hpMax);

        if (CheckAccess(false))
            NetMgr.instance.ActivityHandler.SendSweepGoldLevel(m_mode, hpMax);
    }
}
