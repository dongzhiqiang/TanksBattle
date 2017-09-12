using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIHades : UIPanel
{

    public StateGroup m_monsters;
    public StateGroup m_bosses;
    public StateGroup m_bloods;
    public UIHadesMonsterIcon m_player;


    public override void OnInitPanel()
    {


    }

    public override void OnOpenPanel(object param)
    {

    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
   
    }

    public void FreshRole(Role player, List<Role> monsters, List<Role> bosses, List<Role> bloods)
    {
        m_player.FreshRole(player);
        m_monsters.SetCount(monsters.Count);
        for (int i = 0; i < monsters.Count;i++ )
        {
            m_monsters.Get<UIHadesMonsterIcon>(i).FreshRole(monsters[i]);
        }
        m_bosses.SetCount(bosses.Count);
        for (int i = 0; i < bosses.Count; i++)
        {
            m_bosses.Get<UIHadesMonsterIcon>(i).FreshRole(bosses[i]);
        }
        m_bloods.SetCount(bloods.Count);
        for (int i = 0; i < bloods.Count; i++)
        {
            m_bloods.Get<UIHadesMonsterIcon>(i).FreshRole(bloods[i]);
        }
    }
}
