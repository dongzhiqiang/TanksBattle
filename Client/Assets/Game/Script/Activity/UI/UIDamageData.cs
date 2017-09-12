using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DamageDataParamItem
{
    public int roleObjId;
    public string roleName;
    public string roleId;

    public DamageDataParamItem(int objId, string roleName, string roleId)
    {
        this.roleObjId = objId;
        this.roleName = roleName;
        this.roleId = roleId;
    }
}

public class UIDamageData : UIPanel
{
    public class DamageDataParam
    {
        public List<DamageDataParamItem> leftParams = new List<DamageDataParamItem>();
        public List<DamageDataParamItem> rightParams = new List<DamageDataParamItem>();
    }

    private class AllCombatRecord
    {
        public List<CombatRecord> leftCombatRecords = new List<CombatRecord>();
        public List<CombatRecord> rightCombatRecords = new List<CombatRecord>();

        public void Clear()
        {
            leftCombatRecords.Clear();
            rightCombatRecords.Clear();
        }
    }

    private AllCombatRecord m_allRecord = new AllCombatRecord();
    private int m_maxDmg;
    private int m_maxHeal;
    private int m_maxHurt;

    public StateHandle m_btnBg;
    public StateGroup m_btnGrp;

    public List<UIDamgeDataItem> m_leftItems;
    public List<UIDamgeDataItem> m_rightItems;
    
    public override void OnInitPanel()
    {
        m_btnBg.AddClick(()=>{ Close(); });
        m_btnGrp.AddSel(OnSelBtn);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        var curParam = (DamageDataParam)param;
        var cbtMgr = CombatMgr.instance;

        m_maxDmg = 1;
        m_maxHeal = 1;
        m_maxHurt = 1;
        m_allRecord.Clear();
        
        foreach (var p in curParam.leftParams)
        {
            var record = cbtMgr.GetCombatRecord(p.roleObjId);
            if (string.IsNullOrEmpty(record.roleId))
                record.roleId = p.roleId;
            if (string.IsNullOrEmpty(record.roleName))
                record.roleName = p.roleName;
            m_allRecord.leftCombatRecords.Add(record);
            if (record.hitDamage > m_maxDmg)
                m_maxDmg = record.hitDamage;
            if (record.addHp > m_maxHeal)
                m_maxHeal = record.addHp;
            if (record.beHitDamage > m_maxHurt)
                m_maxHurt = record.beHitDamage;
        }
        foreach (var p in curParam.rightParams)
        {
            var record = cbtMgr.GetCombatRecord(p.roleObjId);
            if (string.IsNullOrEmpty(record.roleId))
                record.roleId = p.roleId;
            if (string.IsNullOrEmpty(record.roleName))
                record.roleName = p.roleName;
            m_allRecord.rightCombatRecords.Add(record);
            if (record.hitDamage > m_maxDmg)
                m_maxDmg = record.hitDamage;
            if (record.addHp > m_maxHeal)
                m_maxHeal = record.addHp;
            if (record.beHitDamage > m_maxHurt)
                m_maxHurt = record.beHitDamage;
        }

        m_btnGrp.SetSel(0);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_allRecord.Clear();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    private void OnSelBtn(StateHandle btn, int index)
    {
        RefreshUI(index);
    }

    private void RefreshUI(int type)
    {
        for (var i = 0; i < m_leftItems.Count; ++i)
        {
            var uiItem = m_leftItems[i];
            if (i >= m_allRecord.leftCombatRecords.Count)
            {
                uiItem.gameObject.SetActive(false);
            }
            else
            {
                uiItem.gameObject.SetActive(true);
                var dataItem = m_allRecord.leftCombatRecords[i];
                int val;
                int maxVal;
                switch (type)
                {
                    case 1:
                        val = dataItem.addHp;
                        maxVal = m_maxHeal;
                        break;
                    case 2:
                        val = dataItem.beHitDamage;
                        maxVal = m_maxHurt;
                        break;
                    default:
                        val = dataItem.hitDamage;
                        maxVal = m_maxDmg;
                        break;
                }
                uiItem.Init(dataItem.roleId, dataItem.roleName, val, maxVal);
            }
        }
        for (var i = 0; i < m_rightItems.Count; ++i)
        {
            var uiItem = m_rightItems[i];
            if (i >= m_allRecord.rightCombatRecords.Count)
            {
                uiItem.gameObject.SetActive(false);
            }
            else
            {
                uiItem.gameObject.SetActive(true);
                var dataItem = m_allRecord.rightCombatRecords[i];
                int val;
                int maxVal;
                switch (type)
                {
                    case 1:
                        val = dataItem.addHp;
                        maxVal = m_maxHeal;
                        break;
                    case 2:
                        val = dataItem.beHitDamage;
                        maxVal = m_maxHurt;
                        break;
                    default:
                        val = dataItem.hitDamage;
                        maxVal = m_maxDmg;
                        break;
                }
                uiItem.Init(dataItem.roleId, dataItem.roleName, val, maxVal);
            }
        }
    }
}