using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UIGuardLevelItem : MonoBehaviour
{
    public int m_mode;
    public StateHandle m_btnEnter;
    public StateHandle m_btnSweep;

    public void Init()
    {
        m_btnEnter.AddClick(OnEnter);
        m_btnSweep.AddClick(OnSweep);
    }

    public void SetState(int maxGuardLvMode)
    {
        if (m_mode == maxGuardLvMode)
            this.GetComponent<StateHandle>().SetState(2);
        else if (m_mode == maxGuardLvMode + 1)
            this.GetComponent<StateHandle>().SetState(3);
        else if (m_mode > maxGuardLvMode + 1)
            this.GetComponent<StateHandle>().SetState(0);
        else
            this.GetComponent<StateHandle>().SetState(1);
    }

    private bool CheckAccess(bool tryEnter)
    {
        GuardLevelBasicCfg basicCfg = GuardLevelBasicCfg.Get();
        GuardLevelModeCfg modeCfg;
        if (!GuardLevelModeCfg.m_cfgs.TryGetValue(m_mode, out modeCfg))
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return false;
        }

        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;

        long guardLvlTime = part.GetLong(enActProp.guardLvlTime);
        int guardLvlCnt = part.GetInt(enActProp.guardLvlCnt);
        int guardLvlMax = part.GetInt(enActProp.guardLvlMax);

        long curTime = TimeMgr.instance.GetTimestamp();
        long timePass = curTime >= guardLvlTime ? curTime - guardLvlTime : guardLvlTime - curTime;

        if (timePass < basicCfg.coolDown)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.IN_COOL_DOWN));
            return false;
        }

        if (!TimeMgr.instance.IsToday(guardLvlTime))
            guardLvlCnt = 0;

        if (guardLvlCnt >= basicCfg.dayMaxCnt)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.DAY_MAX_CNT));
            return false;
        }

        if (tryEnter)
        {
            if (m_mode > guardLvlMax + 1)
            {
                UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.PRE_MODE_WRONG));
                return false;
            }
        }
        else
        {
            if (m_mode > guardLvlMax)
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
            NetMgr.instance.ActivityHandler.SendEnterGuardLevel(m_mode);
         
    }

    private void OnSweep()
    {
        GuardLevelModeCfg modeCfg;
        if (!GuardLevelModeCfg.m_cfgs.TryGetValue(m_mode, out modeCfg))
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


        if (CheckAccess(false))
            NetMgr.instance.ActivityHandler.SendSweepGuardLevel(m_mode);
       
    }
}
