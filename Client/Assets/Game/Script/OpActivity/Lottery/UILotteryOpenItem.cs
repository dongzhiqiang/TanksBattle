using UnityEngine;
using System.Collections;

public class UILotteryOpenItem : MonoBehaviour
{
    public const int ANI_TYPE_STATIC_BACK = 0;
    public const int ANI_TYPE_SIMPLE_ROLL = 1;
    public const int ANI_TYPE_SIMPLE_FLAT = 2;
    public const int ANI_TYPE_PURPLE_ROLL = 3;
    public const int ANI_TYPE_PURPLE_FLAT = 4;
    public const int ANI_TYPE_GOLDEN_ROLL = 5;
    public const int ANI_TYPE_GOLDEN_FLAT = 6;

    public StateHandle m_switch;
    public UIItemIcon m_itemIcon;
    public UIPetIcon m_petIcon;
    public AnimatorHandle m_aniHandle;
    public string[] m_animNames;
    public float[] m_animLens;

    public void Init(int itemId, int itemNum, int aniType = ANI_TYPE_STATIC_BACK)
    {
        m_switch.SetState(0);
        m_itemIcon.Init(itemId, itemNum, false);
        PlayAnim(aniType);
    }

    public void Init(string roleId, int aniType = ANI_TYPE_STATIC_BACK)
    {
        m_switch.SetState(1);
        m_petIcon.Init(roleId, false, RoleCfg.Get(roleId).initStar);
        PlayAnim(aniType);
    }

    public void PlayAnim(int aniType)
    {
        m_aniHandle.m_curAni = m_animNames[aniType];
        m_aniHandle.Play();
    }

    public float GetAnimLen(int aniType)
    {
        return m_animLens[aniType];
    }
}