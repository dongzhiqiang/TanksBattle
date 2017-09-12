using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class Talent
{
    public string talentId;
    public int level;
    public int pos;
    public static Talent Create(TalentVo vo)
    {
        Talent talent;
        talent = new Talent();
        talent.LoadFromVo(vo);
        return talent;
    }

    virtual public void LoadFromVo(TalentVo vo)
    {
        talentId = vo.talentId;
        level = vo.level;
    }
}