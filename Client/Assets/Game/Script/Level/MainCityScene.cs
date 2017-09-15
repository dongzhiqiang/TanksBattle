using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//主城场景
public class MainCityScene : LevelBase
{

    AniFxMgr heroAni;
    AniFxMgr pet1Ani;
    AniFxMgr pet2Ani;
    AniFxMgr shinv1Ani;
    AniFxMgr shinv2Ani;

    GameObject heroGo;
    GameObject pet1Go;
    GameObject pet2Go;

   
  

    static string Ani_zhuCheng_DaiJi_01 = "zhucheng_daiji01";
    static string Ani_zhuCheng_DaiJi_02 = "zhucheng_daiji02";
    static string Ani_zhuCheng_DaiJiGuoDu = "zhucheng_daijiguodu";
    static string Ani_zhuCheng_ShiNvDaiJi = "xiuxiandaiji";
    static string Ani_zhuCheng_DaiJi = "zhuchengdaiji";

    static bool isFirst = true;

    //能不能加非战斗状态
    public override bool CanAddUnaliveBuff { get { return true; } }

    public override IEnumerator OnLoad()
    {
        Role hero = RoleMgr.instance.Hero;
        //加载主角模型
        GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(RoleMgr.instance.Hero.Cfg.mod, null, OnLoadHeroMod, false);

        //加载神侍模型
        List<Role> myPets = hero.PetsPart.GetMainPets();
        Role pet1 = null, pet2 = null;
        PetFormation heroPetFormation = hero.PetFormationsPart.GetCurPetFormation();
        for (int i = 0; i < myPets.Count; ++i)
        {
            if (myPets[i].GetString(enProp.guid) == heroPetFormation.GetPetGuid(enPetPos.pet1Main))
            {
                pet1 = myPets[i];
                continue;
            }
            if (myPets[i].GetString(enProp.guid) == heroPetFormation.GetPetGuid(enPetPos.pet2Main))
            {
                pet2 = myPets[i];
                continue;
            }
        }

        if (pet1 != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(pet1.Cfg.mod, pet1.Cfg.petAniGroupId, OnLoadPet1Mod, false);

        if (pet2 != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(pet2.Cfg.mod, pet2.Cfg.petAniGroupId, OnLoadPet2Mod, false);

        hero.Add(MSG_ROLE.PET_FORMATION_CHANGE, refreshPetMod);

        yield return 0;
    }

    //场景切换完成
    public override void OnLoadFinish()
    {      
        //打开主城界面        
        UIMgr.instance.Open<UIMainCity>();

        GameObject shinv1Go = GameObject.Find("mod_shinv01_zc");
        if (shinv1Go != null)
        {
            shinv1Ani = shinv1Go.transform.Find("model").GetComponent<AniFxMgr>();
            PlayShiNv1Ani();
        }

        GameObject shinv2Go = GameObject.Find("mod_shinv02_zc");
        if (shinv2Go != null)
        {
            shinv2Ani = shinv2Go.transform.Find("model").GetComponent<AniFxMgr>();
            PlayShiNv2Ani();
        }

        //相机切到主角身上
        GameObject heroPos = GameObject.Find("heroPos");
        CameraTriggerMgr caTriggerMgr = CameraTriggerMgr.instance;
        CameraTriggerGroup caTriggerGroup = caTriggerMgr.CurGroup;
        CameraMgr.instance.SetFollow(heroPos.transform);
        if (CameraTriggerMgr.instance != null)
            CameraTriggerMgr.instance.CurGroup.SetGroupActive(true);

        if (isFirst)
        {
            isFirst = false;
            CameraMgr.instance.Add(CameraTriggerMgr.instance.CurGroup.Triggers[0].m_info);
        }
        else
        {
            CameraMgr.instance.Set(CameraTriggerMgr.instance.CurGroup.Triggers[0].m_info);
        }



        //回城弹窗
        CheckOpen();


    }
    //创建全局敌人的时候，返回全局敌人的阵营，如果不希望创建可以返回enCamp.max
    public override enCamp OnCreateGlobalEnemy() { return enCamp.max; }


    public override void OnExit()
    {
        heroGo.transform.Find("model/weapon_mesh_01").gameObject.SetActive(true);
        heroGo.transform.Find("model/weapon_mesh").gameObject.SetActive(true);
        heroGo.transform.localScale = Vector3.one;
        if (heroGo != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(heroGo);
            heroGo = null;
        }
        if (pet1Go != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(pet1Go);
            pet1Go = null;
        }
        if (pet2Go != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(pet2Go);
            pet2Go = null;
        }

    }

    void OnLoadHeroMod(GameObject modelObj, object obj)
    {        
        heroGo = modelObj;
        GameObject heroPos = GameObject.Find("heroPos");
        modelObj.transform.position = heroPos.transform.position;
        modelObj.transform.rotation = heroPos.transform.rotation;
        modelObj.transform.localScale = heroPos.transform.localScale;

        modelObj.transform.Find("model/weapon_mesh_01").gameObject.SetActive(false);
        modelObj.transform.Find("model/weapon_mesh").gameObject.SetActive(false);


        heroAni = modelObj.transform.Find("model").GetComponent<AniFxMgr>();
        Room.instance.StartCoroutine(CoPlayHeroAni());

        UIMgr.instance.Get<UIMainCity>().SetRoleMod(modelObj, 0);


    }

    void OnLoadPet1Mod(GameObject modelObj, object obj)
    {        
        pet1Go = modelObj;        
        GameObject pet1Pos = GameObject.Find("pet1Pos");
        modelObj.transform.position = pet1Pos.transform.position;
        modelObj.transform.rotation = pet1Pos.transform.rotation;
        UIMgr.instance.Get<UIMainCity>().SetRoleMod(pet1Go, 1);
        pet1Ani = modelObj.transform.Find("model").GetComponent<AniFxMgr>();

        int petAniCfgId = (int)obj;
        Room.instance.StartCoroutine(PlayPetAni(pet1Ani, petAniCfgId)); 
    }

    void OnLoadPet2Mod(GameObject modelObj, object obj)
    {        
        pet2Go = modelObj;        
        GameObject pet2Pos = GameObject.Find("pet2Pos");
        modelObj.transform.position = pet2Pos.transform.position;
        modelObj.transform.rotation = pet2Pos.transform.rotation;
        UIMgr.instance.Get<UIMainCity>().SetRoleMod(pet2Go, 2);
        pet2Ani = modelObj.transform.Find("model").GetComponent<AniFxMgr>();
        int petAniCfgId = (int)obj;
        Room.instance.StartCoroutine(PlayPetAni(pet2Ani, petAniCfgId));



    }

    void refreshPetMod()
    {
        Role hero = RoleMgr.instance.Hero;
        if (pet1Go != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(pet1Go);
            pet1Go = null;
        }
        if (pet2Go != null)
        {
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Put(pet2Go);
            pet2Go = null;
        }

        //加载神侍模型
        List<Role> myPets = hero.PetsPart.GetMainPets();
        //Role pet1 = hero.PetsPart.GetPet(hero.GetString(enProp.pet1Main));
        //Role pet2 = hero.PetsPart.GetPet(hero.GetString(enProp.pet2Main));
        Role pet1 = null, pet2 = null;

        PetFormation heroPetFormation = hero.PetFormationsPart.GetCurPetFormation();

        for (int i = 0; i < myPets.Count; ++i)
        {
            if (myPets[i].GetString(enProp.guid) == heroPetFormation.GetPetGuid(enPetPos.pet1Main))
            {
                pet1 = myPets[i];
                continue;
            }
            if (myPets[i].GetString(enProp.guid) == heroPetFormation.GetPetGuid(enPetPos.pet2Main))
            {
                pet2 = myPets[i];
                continue;
            }
        }
        if (pet1 != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(pet1.Cfg.mod, pet1.Cfg.petAniGroupId, OnLoadPet1Mod, false);
        else
            UIMgr.instance.Get<UIMainCity>().SetRoleMod(null,1);

        if (pet2 != null)
            GameObjectPool.GetPool(GameObjectPool.enPool.Role).Get(pet2.Cfg.mod, pet2.Cfg.petAniGroupId, OnLoadPet2Mod, false);
        else
            UIMgr.instance.Get<UIMainCity>().SetRoleMod(null,2);

    }

    IEnumerator CoPlayHeroAni()
    {
        if (heroAni == null) yield return 0;

        do
        {
            //播放待机动作1
            heroAni.Play(Ani_zhuCheng_DaiJi_01, WrapMode.Loop,0,1);
            yield return new WaitForSeconds(Random.Range(4, 7));

            //播放过度动作
            heroAni.Play(Ani_zhuCheng_DaiJiGuoDu, WrapMode.ClampForever, 0, 1);
            while(heroAni.CurSt != null && heroAni.CurSt.normalizedTime < 1f)
                yield return 0;

            //播放待机动作2
            heroAni.Play(Ani_zhuCheng_DaiJi_02, WrapMode.Loop);
            yield return new WaitForSeconds(Random.Range(4, 7));

            //播放过度动作
            heroAni.Play(Ani_zhuCheng_DaiJiGuoDu, WrapMode.PingPong, 0, 1);
            heroAni.CurSt.normalizedTime = 1;

            while(heroAni.CurSt != null && heroAni.CurSt.normalizedTime  < 2)
            {
                yield return 0;
            }

        } while (true);
    }

    IEnumerator PlayPetAni(AniFxMgr ani,int petAniGroupId)
    {
        if (ani == null)  yield return 0;

        PetAniGroupCfg petAniGroupCfg = PetAniGroupCfg.m_cfgs[petAniGroupId];
        
        if (petAniGroupCfg.startAni != "-1")
        {
            ani.Play(petAniGroupCfg.startAni, WrapMode.Loop,0);
            while (ani.CurSt != null && ani.CurSt.normalizedTime < 1f)               
                yield return 0;            
        }
        List<List<string>> specialAni = petAniGroupCfg.specialAni;
        if (specialAni == null)
            specialAni = new List<List<string>>();

        float[] freezeTimes = new float[specialAni.Count];
        for (int i = 0; i < freezeTimes.Length; ++i)
            freezeTimes[i] = 0;

        float[] curTimes = new float[specialAni.Count];

        for (int i = 0; i < curTimes.Length; ++i)
            curTimes[i] = 0;


        while (true)
        {            
            int randNum = Random.Range(0, 10000);
            int randTotal = 0;
            bool isPlaySpecial = false;
            for(int i=0;i<specialAni.Count;++i)
            {
                int curRand = int.Parse(specialAni[i][1]);
                string aniName = specialAni[i][0];                
                randTotal += curRand;

                if (randNum <= randTotal && TimeMgr.instance.logicTime >= curTimes[i] + freezeTimes[i])
                {
                    freezeTimes[i] = float.Parse(specialAni[i][2]);
                    isPlaySpecial = true;
                    ani.Play(aniName, WrapMode.Loop,0);
                    curTimes[i] = TimeMgr.instance.logicTime;
                    break;
                }                
            }
            if(!isPlaySpecial)
            {
                ani.Play(petAniGroupCfg.defaultAni, WrapMode.Loop,0);
            }
            while (ani.CurSt != null && ani.CurSt.normalizedTime < 1f)
                yield return 0;
        }        
    }

    public override void OnUpdate()
    {
        if(pet1Ani!=null)
        {
            //Debug.Log(pet1Ani.CurSt.normalizedTime);
        }
    }

    void PlayShiNv1Ani()
    {
        if (shinv1Ani == null) return;

        shinv1Ani.Play(Ani_zhuCheng_DaiJi, WrapMode.Loop);
    }

    void PlayShiNv2Ani()
    {
        if (shinv2Ani == null) return;

        shinv2Ani.Play(Ani_zhuCheng_DaiJi, WrapMode.Loop);
    }

    //检查一些回城弹窗
    void CheckOpen()
    {
        //TimeMgr.instance.AddTimer(1, CheckOpen2);
        CheckOpen2(); //策划决定不延时 有需求冲突那就是策划脑残
    }

    void CheckOpen2()
    {
        
        //检测是否需要弹出别人赠送体力的提示
        RoleMgr.instance.Hero.SocialPart.CheckStamDlg();

        //检测是否需要弹出神器抢夺提示
        ActivityMgr.instance.CheckTreasureRobDlg();

        //如果是勇士试炼回来的就打开界面
        if (RoleMgr.instance.Hero.ActivityPart.warrIndex > 0)
        {
            UIMgr.instance.Open<UIWarriorsTried>();
            RoleMgr.instance.Hero.ActivityPart.warrIndex = 0;
        }

        
        if (UIHeroUpgrade.CheckOpen())
            return;
        if (UIArena.CheckOpen())
            return;
        if (UITreasureRob.CheckOpen())
            return;
        if (UIProphetTowerLevel.CheckOpen())
            return;
    }
}
