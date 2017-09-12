using System.Linq;
using UnityEngine.UI;

public class UITreasure2 : UIPanel
{
    #region SerializeFields
    public StateGroup m_treasures;
    public StateGroup m_battleTreasures;
    public Text m_power;

    private Role m_hero;
    #endregion

    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        m_hero = param as Role;

        UpdateTreasures();
    }

    public override void OnClosePanel()
    {
        m_hero = null;
    }

    void UpdateTreasures()
    {
        var treasures = m_hero.TreasurePart.Treasures;
        var battleTreasures = m_hero.TreasurePart.BattleTreasures;
        m_treasures.SetCount(TreasureCfg.m_cfgs.Count);

        var idx = 0;        
        foreach (var dataItem in treasures.OrderByDescending(e => e.Value.level))
        {
            var uiItem = m_treasures.Get<UITreasureIcon2>(idx++);
            var treasure = dataItem.Value;
            uiItem.Init(treasure.treasureId, treasure.level, battleTreasures.IndexOf(treasure.treasureId));
        }

        foreach (TreasureCfg cfg in TreasureCfg.m_cfgs.Values)
        {
            if (treasures.ContainsKey(cfg.id))
                continue;

            var uiItem = m_treasures.Get<UITreasureIcon2>(idx++);
            uiItem.Init(cfg.id, 0, -1);
        }

        m_battleTreasures.SetCount(battleTreasures.Count);
        for (int i = 0; i < battleTreasures.Count; i++)
        {
            var treasureId = battleTreasures[i];
            var treasure = m_hero.TreasurePart.GetTreasure(treasureId);
            m_battleTreasures.Get<UITreasureIcon2>(i).Init(treasure.treasureId, treasure.level, i);
        }

        m_power.text = m_hero.TreasurePart.GetTreasurePower().ToString();
    }

    public void Refresh()
    {
        UpdateTreasures();
    }
}