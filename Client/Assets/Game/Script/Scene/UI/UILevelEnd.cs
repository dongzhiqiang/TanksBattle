using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UILevelEnd : UIPanel
{
    public Animation aniEnd;
    public List<GameObject> mStars = new List<GameObject>();

    public List<TextEx> mTextList = new List<TextEx>();

    public StateGroup mRewardGroup;
    public StateGroup mExpGroup;
    public StateHandle mBtnEnd;

    string shengliAni = "shengli01:单次:-1|shengli02:循环:-1";

    bool isCanClose = false;

    //相机信息
    public CameraInfo m_cameraInfo;

    public override void OnInitPanel()
    {
        mBtnEnd.AddClick(() => { if (isCanClose) LevelMgr.instance.GotoMaincity(); });
    }

    public override void OnOpenPanel(object param)
    {
        //RenderSettings.fog = false;

        LevelEndResVo info = param as LevelEndResVo;
        if (info.isWin)
        {
            RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(info.roomId);

            LevelScene ls = LevelMgr.instance.CurLevel as LevelScene;
            Dictionary<int, ItemVo> items = ls.GetAllDropRewards();
            mRewardGroup.SetCount(items.Count);
            int i = 0;
            foreach (ItemVo item in items.Values)
            {
                mRewardGroup.Get<UIItemIcon>(i).Init(item.itemId, item.num);
                mRewardGroup.Get<UIItemIcon>(i).isSimpleTip = true;
                i++;
            }

            List<SceneTrigger> triList = SceneEventMgr.instance.conditionTriggerList;
            bool isReach = false;

            if (triList.Count < 3)
            {
                Debug.LogError("没有配置通关条件");
                return;
            }

            for (int idx = 0; idx < mStars.Count; idx++)
            {
                isReach = triList[idx].bReach();
                mStars[idx].gameObject.SetActive(isReach);
                if (isReach)
                    mTextList[idx].text= string.Format("<color=green>{0}</color>", triList[idx].GetConditionCfg().endDesc);
                else
                    mTextList[idx].text = triList[idx].GetConditionCfg().endDesc;
            }

            mExpGroup.SetCount(3);
            UILevelExpItem expItem1 = mExpGroup.Get<UILevelExpItem>(0);
            Role hero = RoleMgr.instance.Hero;
            if (hero != null && info.heroExp > 0)
                expItem1.Init(hero.Cfg.icon, info.heroExp);
            else
                expItem1.gameObject.SetActive(false);

            PetFormation petFormation = hero.PetFormationsPart.GetCurPetFormation();

            string guid1 = petFormation.GetPetGuid(enPetPos.pet1Main);
            string guid2 = petFormation.GetPetGuid(enPetPos.pet2Main);

            Role pet1 = null;
            UILevelExpItem expItem2 = mExpGroup.Get<UILevelExpItem>(1);
            if (!string.IsNullOrEmpty(guid1))
                pet1 = hero.PetsPart.GetPet(guid1);

            if (pet1 != null && info.pet1Exp > 0)
                expItem2.Init(pet1.Cfg.icon, info.pet1Exp);
            else
                expItem2.gameObject.SetActive(false);

            Role pet2 = null;
            UILevelExpItem expItem3 = mExpGroup.Get<UILevelExpItem>(2);
            if (!string.IsNullOrEmpty(guid2))
                pet2 = hero.PetsPart.GetPet(guid2);

            if (pet2 != null && info.pet2Exp > 0)
                expItem3.Init(pet2.Cfg.icon, info.pet2Exp);
            else
                expItem3.gameObject.SetActive(false);
        }
        else
        {

        }

    }

    public override void OnOpenPanelEnd()
    {
        aniEnd.gameObject.SetActive(true);
        aniEnd.PlayQueued("chufa", QueueMode.PlayNow);
        aniEnd.PlayQueued("daiji", QueueMode.CompleteOthers);
    }

    public override void OnClosePanel()
    {
        TimeMgr.instance.ResetPause();
        aniEnd.gameObject.SetActive(false);
    }


    public override void OnUpdatePanel()
    {

    }

    public IEnumerator onLevelEnd(LevelEndResVo vo)
    {
        TimeMgr.instance.AddPause();

        //修改主角朝向
        Role hero = RoleMgr.instance.Hero;
        if (hero != null && hero.TranPart != null)
        {
            hero.TranPart.SetEuler(new Vector3(0, CameraMgr.instance.m_horizontalAngle + 160, 0));
        }

        SimpleAnimationsCxt cxt = SimpleAnimationsCxt.Parse(shengliAni);
        if (hero != null && hero.AniPart != null)
            hero.AniPart.Play(cxt);

        //切换相机
        UILevelEnd ui = UIMgr.instance.Get<UILevelEnd>();
        ui.m_cameraInfo.horizontalAngle = CameraMgr.instance.m_horizontalAngle;
        ui.m_cameraInfo.durationPriority = 50;//优先级要比普通镜头高
        ui.m_cameraInfo.priority = 50;//优先级要比普通镜头高
        CameraMgr.instance.Add(ui.m_cameraInfo);
        yield return new WaitForSeconds(ui.m_cameraInfo.durationSmooth);

        isCanClose = false;

        GetComponent<UIPanelBase>().Open(vo);

        yield return new WaitForSeconds(2);

        isCanClose = true;
    }
}
