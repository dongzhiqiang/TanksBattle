using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UISweepLevelRewardItem : MonoBehaviour
{
    public TextEx m_title;
    public StateGroup m_propRewardGrp;
    public StateGroup m_itemRewardGrp;
    public float m_itemHeightShort;
    public float m_itemHeightTall;
    public float m_animShowItemGap = 0.15f;
    public float m_animEndWaitTime = 0.5f;

    private bool m_isPlayingAnim = false;

    public void Init(bool previewMode, string title, Dictionary<enProp, int> propRewards, Dictionary<int, int> itemRewards)
    {
        var curStateHandle = GetComponent<StateHandle>();
        curStateHandle.SetState(3);
        var curLayoutElement = GetComponent<LayoutElement>();
        curLayoutElement.minHeight = curStateHandle.CurStateIdx == 1 || curStateHandle.CurStateIdx == 3 ? m_itemHeightShort : m_itemHeightTall;

        m_title.text = title == null ? "" : title;

        if (propRewards != null && propRewards.Count > 0)
        {
            m_propRewardGrp.SetCount(propRewards.Count);
            var index = 0;
            foreach (var item in propRewards)
            {
                var stateIdx = 0;
                switch (item.Key)
                {
                    case enProp.exp:
                        stateIdx = 0;
                        break;
                    case enProp.gold:
                        stateIdx = 1;
                        break;
                    case enProp.diamond:
                        stateIdx = 2;
                        break;
                    case enProp.stamina:
                        stateIdx = 3;
                        break;
                }
                var uiItem = m_propRewardGrp.Get<StateHandle>(index++);
                uiItem.gameObject.SetActive(true);
                uiItem.SetState(stateIdx);
                var uiText = uiItem.GetComponentInChildren<TextEx>();
                if (uiText != null)
                    uiText.text = item.Value.ToString();
            }
        }
        else
        {
            m_propRewardGrp.SetCount(0);
        }

        if (itemRewards != null && itemRewards.Count > 0)
        {
            m_itemRewardGrp.SetCount(itemRewards.Count);
            var index = 0;
            foreach (var item in itemRewards)
            {
                var uiItem = m_itemRewardGrp.Get<UIItemIcon>(index++);
                uiItem.gameObject.SetActive(true);
                uiItem.Init(item.Key, item.Value);
            }
        }
        else
        {
            m_itemRewardGrp.SetCount(0);
        }
    }

    private IEnumerator CoPlayShowAnim()
    {
        m_isPlayingAnim = true;

        for (var i = 0; i < m_propRewardGrp.Count; ++i)
        {
            if (m_isPlayingAnim)
                yield return new WaitForSeconds(m_animShowItemGap);
            m_propRewardGrp.Get(i).gameObject.SetActive(true);
        }

        for (var i = 0; i < m_itemRewardGrp.Count; ++i)
        {
            if (m_isPlayingAnim)
                yield return new WaitForSeconds(m_animShowItemGap);
            m_itemRewardGrp.Get(i).gameObject.SetActive(true);
        }

        if (m_isPlayingAnim)
            yield return new WaitForSeconds(m_animEndWaitTime);

        m_isPlayingAnim = false;
    }

    public void HideAllItems()
    {
        for (var i = 0; i < m_propRewardGrp.Count; ++i)
            m_propRewardGrp.Get(i).gameObject.SetActive(false);
        for (var i = 0; i < m_itemRewardGrp.Count; ++i)
            m_itemRewardGrp.Get(i).gameObject.SetActive(false);
    }

    public void PlayShowAnim()
    {
        UIMgr.instance.StartCoroutine(CoPlayShowAnim());
    }

    public void CancelAnim()
    {
        m_isPlayingAnim = false;
    }

    public bool IsPlayingAnim()
    {
        return m_isPlayingAnim;
    }
}