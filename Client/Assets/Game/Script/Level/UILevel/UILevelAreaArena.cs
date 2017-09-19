using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UILevelAreaArena : UILevelArea
{
    public ImageEx m_myPartAllHP;
    public TextEx m_myHeroName;

    public ImageEx m_itsPartAllHP;
    public TextEx m_itsHeroName;

    public StateGroup myRolesGroup;
    public StateGroup itsRolesGroup;
    
    private Role m_myHero = null;
    private Role m_itsHero = null;
    private Role[] myRoles= new Role[3];
    private Role[] itsRoles = new Role[3];
    private bool[] isMyRoleInit = new bool[3];
    private bool[] isItsRoleInit = new bool[3];    
    private HashSet<Role> m_myPartRoles = new HashSet<Role>();
    private float m_myAllHPMax = -1;
    private Dictionary<Role, List<int>> m_myPartObs = new Dictionary<Role, List<int>>();
    private HashSet<Role> m_itsPartRoles = new HashSet<Role>();
    private float m_itsAllHPMax = -1;
    private Dictionary<Role, List<int>> m_itsPartObs = new Dictionary<Role, List<int>>();

    public override enLevelArea Type { get { return enLevelArea.arena; } }
    public override bool IsOpenOnStart { get { return false; } }

    protected override void OnInitPage()
    {
        ClearAll();
    }

    protected override void OnOpenArea(bool reopen)
    {
    }

    protected override void OnUpdateArea()
    {

    }

    protected override void OnCloseArea()
    {
        ClearAll();
    }

    protected override void OnRoleBorn()
    {

    }

    public bool IsInMoniter(Role role)
    {
        // return role == m_myHero || m_pet1.Pet == role || m_pet2.Pet == role;
        return true;
    }

    public void ClearAll()
    {
        m_myPartAllHP.fillAmount = 0;
        m_myHeroName.text = "";
        //m_myHeroHP.fillAmount = 0;
        //m_myHeroMP.fillAmount = 0;

        m_itsPartAllHP.fillAmount = 0;
        m_itsHeroName.text = "";     
        //m_pet1.Init(null);
        //m_pet2.Init(null);

        ClearMyPartRoles();
        ClearItsPartRole();
    }

    public void RefreshUI()
    {
        //OnMyHeroHP();
        //OnMyHeroMP();
        OnMyPartHP();
        OnItsPartHP();
    }

    

    public void AddMyPartRole(Role role, bool refreshUINow = false)
    {
        if (m_myPartRoles.Add(role))
        {
            var obs = m_myPartObs.GetNewIfNo(role);
            var ob = role.AddPropChange(enProp.hp, OnMyPartHP);
            obs.Add(ob);
            ob = role.AddPropChange(enProp.hpMax, OnMyPartHP);
            obs.Add(ob);
            if (refreshUINow)
                OnMyPartHP();
            if (role.GetInt(enProp.heroId) != 0)
            {
                m_myHero = role;
                myRoles[0] = m_myHero;
                m_myHeroName.text = role.GetString(enProp.name);

                /*var ob = role.AddPropChange(enProp.hp, OnMyHeroHP);
                obs.Add(ob);
                ob = role.AddPropChange(enProp.hpMax, OnMyHeroHP);
                obs.Add(ob);
                ob = role.AddPropChange(enProp.mp, OnMyHeroMP);
                obs.Add(ob);
                ob = role.AddPropChange(enProp.mpMax, OnMyHeroMP);
                obs.Add(ob);*/

                PetFormation myPetFormation = role.PetFormationsPart.GetCurPetFormation();

                string guid1 = myPetFormation.GetPetGuid(enPetPos.pet1Main);
                myRoles[1] = string.IsNullOrEmpty(guid1) ? null : role.PetsPart.GetPet(guid1);               
                //m_pet1.Init(pet1);

                string guid2 = myPetFormation.GetPetGuid(enPetPos.pet2Main);
                myRoles[2] = string.IsNullOrEmpty(guid2) ? null : role.PetsPart.GetPet(guid2);                
                //m_pet2.Init(pet2);               
                /*if (refreshUINow)
                {
                    OnMyHeroHP();
                    OnMyHeroMP();
                }*/
            }          
            role.Add(MSG_ROLE.DEAD, OnMyRoleDead);
        }
    }

    public void ClearMyPartRoles()
    {
        foreach (var obs in m_myPartObs.Values)
        {
            foreach (var ob in obs)
            {
                EventMgr.Remove(ob);
            }
        }
        m_myAllHPMax = -1;
        m_myHero = null;
        m_myPartObs.Clear();
        m_myPartRoles.Clear();

        for (int i = 0; i < myRoles.Length; ++i)
            myRoles[i] = null;
        for (int i = 0; i < isMyRoleInit.Length; ++i)
            isMyRoleInit[i] = false;
        for(int i=0;i<myRolesGroup.Count;++i)
        {
            UILevelAreaArenaItem item = myRolesGroup.Get<UILevelAreaArenaItem>(i);
            item.Clear();
        }
    }

    private void OnMyRoleDead(object param1, object param2, object param3, EventObserver observer)
    {
        Role role = observer.GetParent<Role>();
        m_myPartRoles.Remove(role);
        m_myPartObs.Remove(role);

        OnMyPartHP();

        if (m_myHero == role)
        {
            //OnMyHeroHP();
            //OnMyHeroMP();
            m_myHero = null;
        }            
    }

    public void CalcMyAllHPMax()
    {
        m_myAllHPMax = 0.0f;
        foreach (var role in m_myPartRoles)
        {
            m_myAllHPMax += role.GetFloat(enProp.hpMax);
        }
        m_myAllHPMax = Mathf.Max(1.0f, m_myAllHPMax);
    }

    public void AddItsPartRole(Role role, bool refreshUINow = false)
    {
        if (m_itsPartRoles.Add(role))
        {
            var obs = m_itsPartObs.GetNewIfNo(role);
            var ob = role.AddPropChange(enProp.hp, OnItsPartHP);
            obs.Add(ob);
            ob = role.AddPropChange(enProp.hpMax, OnItsPartHP);
            obs.Add(ob);          

            if (refreshUINow)
                OnItsPartHP();
            if (role.GetInt(enProp.heroId) != 0)
            {              
                itsRoles[0] = role;
                m_itsHero = role;
                m_itsHeroName.text = role.GetString(enProp.name);
            }
            role.Add(MSG_ROLE.DEAD, OnItsRoleDead);
        }
    }

    public void ClearItsPartRole()
    {
        foreach (var obs in m_itsPartObs.Values)
        {
            foreach (var ob in obs)
            {
                EventMgr.Remove(ob);
            }
        }
        m_itsAllHPMax = -1;
        m_itsHero = null;
        m_itsPartObs.Clear();
        m_itsPartRoles.Clear();

        for(int i = 0; i < itsRoles.Length; ++i)
            itsRoles[i] = null;
        for (int i = 0; i < isItsRoleInit.Length; ++i)
            isItsRoleInit[i] = false;
        for (int i = 0; i < itsRolesGroup.Count; ++i)
        {
            UILevelAreaArenaItem item = itsRolesGroup.Get<UILevelAreaArenaItem>(i);
            item.Clear();
        }
    }
    private void OnItsRoleDead(object param1, object param2, object param3, EventObserver observer)
    {
        Role role = observer.GetParent<Role>();
        m_itsPartRoles.Remove(role);
        m_itsPartObs.Remove(role);

        OnItsPartHP();
    }

    public void CalcItsAllHPMax()
    {
        m_itsAllHPMax = 0.0f;
        foreach (var role in m_itsPartRoles)
        {
            m_itsAllHPMax += role.GetFloat(enProp.hpMax);
        }
        m_itsAllHPMax = Mathf.Max(1.0f, m_itsAllHPMax);
    }

    private void OnMyPartHP()
    {
        if (m_myAllHPMax < 0)
            CalcMyAllHPMax();

        int allHP = 0;
        foreach (var role in m_myPartRoles)
        {
            allHP += role.GetInt(enProp.hp);
        }
        m_myPartAllHP.fillAmount = Mathf.Clamp01((float)allHP / m_myAllHPMax);
    }

   /* private void OnMyHeroHP()
    {
        if (m_myHero == null)
            return;

        int hp = m_myHero.GetInt(enProp.hp);
        float hpMax = Mathf.Max(1.0f, m_myHero.GetFloat(enProp.hpMax));
        m_myHeroHP.fillAmount = Mathf.Clamp01((float)hp / hpMax);

        OnMyPartHP();
    }*/

    /*private void OnMyHeroMP()
    {
        if (m_myHero == null)
            return;

        int mp = m_myHero.GetInt(enProp.mp);
        float mpMax = Mathf.Max(1.0f, m_myHero.GetFloat(enProp.mpMax));
        m_myHeroMP.fillAmount = Mathf.Clamp01((float)mp / mpMax);
    }*/

    private void OnItsPartHP()
    {
        if (m_itsAllHPMax < 0)
            CalcItsAllHPMax();

        int allHP = 0;
        foreach (var role in m_itsPartRoles)
        {
            allHP += role.GetInt(enProp.hp);
        }
        m_itsPartAllHP.fillAmount = Mathf.Clamp01((float)allHP / m_itsAllHPMax);
    }

    public void InitAllRolesGroup()
    {

        string heroArenaPosStr = m_myHero.ActivityPart.GetString(enActProp.arenaPos);
        List<int> heroArenaPos = heroArenaPosStr == "" ? ArenaBasicCfg.GetArenaPos("1,0,2") : ArenaBasicCfg.GetArenaPos(heroArenaPosStr);
        string enemyArenaPosStr = m_itsHero.ActivityPart.GetString(enActProp.arenaPos);
        List<int> enemyArenaPos = enemyArenaPosStr == "" ? ArenaBasicCfg.GetArenaPos("1,0,2") : ArenaBasicCfg.GetArenaPos(enemyArenaPosStr);

        InitRolesGroup(myRolesGroup, heroArenaPos, myRoles, isMyRoleInit, m_myPartRoles.Count);
        InitRolesGroup(itsRolesGroup, enemyArenaPos, itsRoles, isItsRoleInit, m_itsPartRoles.Count);        

    }
    void InitRolesGroup(StateGroup group,List<int> ArenaPos,Role[] role,bool[] init,int count)
    {
        group.SetCount(count);
        for (int i = 0; i < group.Count; ++i)
        {
            if (!init[ArenaPos[i]] && role[ArenaPos[i]] != null)
            {
                group.Get<UILevelAreaArenaItem>(i).Init(role[ArenaPos[i]]);
                init[ArenaPos[i]] = true;
            }
            else if (!init[ArenaPos[i + 1]] && role[ArenaPos[i + 1]] != null)
            {
                group.Get<UILevelAreaArenaItem>(i).Init(role[ArenaPos[i + 1]]);
                init[ArenaPos[i + 1]] = true;
            }
            else if (!init[ArenaPos[i + 2]] && role[ArenaPos[i + 2]] != null)
            {
                group.Get<UILevelAreaArenaItem>(i).Init(role[ArenaPos[i + 2]]);
                init[ArenaPos[i + 2]] = true;
            }
        }
    }
}