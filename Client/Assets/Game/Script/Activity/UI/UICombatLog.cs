using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UICombatLog : UIPanel
{
    public class CombatLogItem
    {
        public bool     win;
        public int      oldRank;
        public int      rank;
        public int      opHeroId;
        public string   opRoleId;
        public string   opName;
        public string   iconName;
        public long     time;
    }

    public StateGroup m_grid;

    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        //先清除数据
        m_grid.SetCount(0);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    public void OnUpdateData(List<CombatLogItem> items)
    {
        if (items == null || items.Count <= 0)
        {
            m_grid.SetCount(0);
            return;
        }

        m_grid.SetCount(items.Count);
        for (int i = 0; i < items.Count; ++i)
        {
            var item = items[i];
            var uiItem = m_grid.Get<UICombatLogItem>(i);
            uiItem.Init(item);
        }
    }
}
