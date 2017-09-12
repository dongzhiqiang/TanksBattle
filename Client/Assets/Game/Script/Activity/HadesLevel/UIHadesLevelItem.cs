using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UIHadesLevelItem : MonoBehaviour
{
    public int m_mode;
    public StateHandle m_btnEnter;
    public StateHandle m_btnSweep;

    public void Init()
    {
        m_btnEnter.AddClick(OnEnter);
        m_btnSweep.AddClick(OnSweep);
    }

    public void SetState(int maxHadesLvMode)
    {
        if (m_mode == maxHadesLvMode)
            this.GetComponent<StateHandle>().SetState(2);
        else if (m_mode == maxHadesLvMode + 1)
            this.GetComponent<StateHandle>().SetState(3);
        else if (m_mode > maxHadesLvMode + 1)
            this.GetComponent<StateHandle>().SetState(0);
        else
            this.GetComponent<StateHandle>().SetState(1);
    }

    private bool CheckAccess(bool tryEnter)
    {
        HadesLevelBasicCfg basicCfg = HadesLevelBasicCfg.Get();
        HadesLevelModeCfg modeCfg;
        if (!HadesLevelModeCfg.m_cfgs.TryGetValue(m_mode, out modeCfg))
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return false;
        }

        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;

        long hadesLvlTime = part.GetLong(enActProp.hadesLvlTime);
        int hadesLvlCnt = part.GetInt(enActProp.hadesLvlCnt);
        int hadesLvlMax = part.GetInt(enActProp.hadesLvlMax);

        long curTime = TimeMgr.instance.GetTimestamp();

        if (!TimeMgr.instance.IsToday(hadesLvlTime))
            hadesLvlCnt = 0;

        if (hadesLvlCnt >= basicCfg.dayMaxCnt)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.DAY_MAX_CNT));
            return false;
        }

        if (tryEnter)
        {
            if (m_mode > hadesLvlMax + 1)
            {
                UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, RESULT_CODE_ACTIVITY.PRE_MODE_WRONG));
                return false;
            }
        }
        else
        {
            if (m_mode > hadesLvlMax)
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
            NetMgr.instance.ActivityHandler.SendEnterHadesLevel(m_mode);
         
    }

    private void OnSweep()
    {
        HadesLevelModeCfg modeCfg;
        if (!HadesLevelModeCfg.m_cfgs.TryGetValue(m_mode, out modeCfg))
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
            NetMgr.instance.ActivityHandler.SendSweepHadesLevel(m_mode);
       
    }
}
