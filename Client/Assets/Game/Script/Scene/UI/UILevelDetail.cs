using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UILevelDetail : UIPanel
{
    public StateHandle mSweepLevel;
    public StateHandle mEnterBtn;

    public StateHandle mBtnPreLevel;
    public StateHandle mBtnNextLevel;

    public StateHandle mBtnPet1;
    public StateHandle mBtnPet2;


    public ImageEx mPetText1;
    public ImageEx mPetIcon1;
    public ImageEx mPetText2;
    public ImageEx mPetIcon2;
    public ImageEx mPetLock1;
    public ImageEx mPetLock2;
    public TextEx mPetLimitDesc;

    public Text mTitle;
    public TextEx mChallengeNum;
    public TextEx mCostNum;
    public TextEx mFightNum;
    public TextEx mLevelDesc;
    public List<ImageEx> mStars = new List<ImageEx>();
    public List<ImageEx> mStars2 = new List<ImageEx>();
    public TextEx mStory;
    public ImageEx mImage;
    public ImageEx mStarDi;
    public TextEx mCurIdx;
    public List<TextEx> mConditions = new List<TextEx>();

    public StateGroup mRewardGroup;

    public RoomCfg m_cfg;
    public LevelInfo m_info;

    public int m_curIdx;

    int m_pet1Ob;
    int m_pet2Ob;

    string[] petLimitDesc = { "本关禁止携带神侍出战", "本关只能携带一个神侍出战" };

    LevelsPart m_part;
    Role m_hero;
    public override void OnInitPanel()
    {
        mSweepLevel.AddClick(OnSweepLevel);
        mEnterBtn.AddClick(EnterRoom);

        mBtnPreLevel.AddClick(ShowPrevLevel);
        mBtnNextLevel.AddClick(ShowNextLevel);

        mBtnPet1.AddClick(ShowPetSelectWnd);
        mBtnPet2.AddClick(ShowPetSelectWnd);

    }

    public void OnSweepLevel()
    {
        if (m_part.CanEnterLevel(m_info))
        {
            UIMgr.instance.Open<UISweepLevel>(m_cfg.id);
        }
    }

    public void ShowPetSelectWnd()
    {
        UIMgr.instance.Open<UIChoosePet>();
    }

    public void onPet1MainChanged()
    {
        mPetText1.gameObject.SetActive(false);
        mPetIcon1.gameObject.SetActive(false);

        float power = (float)m_hero.GetInt(enProp.powerTotal) / m_cfg.powerNum;
        mFightNum.text = string.Format(PowerFactorCfg2.Get(power).desc, m_hero.GetInt(enProp.powerTotal), m_cfg.powerNum);

        if (m_cfg.petNum >= 1)
        {
            mBtnPet1.GetComponent<StateHandle>().enabled = true;
            mPetLock1.gameObject.SetActive(false);
        }
        else
        {
            mBtnPet1.GetComponent<StateHandle>().enabled = false;
            mPetLock1.gameObject.SetActive(true);
        }

        PetFormation petFormation = m_hero.PetFormationsPart.GetPetFormation(enPetFormation.normal);

        string guid = petFormation.GetPetGuid(enPetPos.pet1Main);
        if (guid == "")
            mPetText1.gameObject.SetActive(true);
        else
        {
            Role pet = m_hero.PetsPart.GetPet(guid);
            if (pet == null)
                mPetText1.gameObject.SetActive(true);
            else
            {
                mPetIcon1.Set(pet.Cfg.icon);
                mPetIcon1.gameObject.SetActive(true);
            }
        }

    }

    public void onPet2MainChanged()
    {
        mPetText2.gameObject.SetActive(false);
        mPetIcon2.gameObject.SetActive(false);

        float power = (float)m_hero.GetInt(enProp.powerTotal) / m_cfg.powerNum;
        mFightNum.text = string.Format(PowerFactorCfg2.Get(power).desc, m_hero.GetInt(enProp.powerTotal), m_cfg.powerNum);

        if (m_cfg.petNum >= 2)
        {
            mBtnPet2.GetComponent<StateHandle>().enabled = true;
            mPetLock2.gameObject.SetActive(false);
        }
        else
        {
            mBtnPet2.GetComponent<StateHandle>().enabled = false;
            mPetLock2.gameObject.SetActive(true);
        }

        PetFormation petFormation = m_hero.PetFormationsPart.GetPetFormation(enPetFormation.normal);

        string guid = petFormation.GetPetGuid(enPetPos.pet2Main);
        if (guid == "")
            mPetText2.gameObject.SetActive(true);
        else
        {
            Role pet = m_hero.PetsPart.GetPet(guid);
            if (pet == null)
                mPetText2.gameObject.SetActive(true);
            else
            {
                mPetIcon2.Set(pet.Cfg.icon);
                mPetIcon2.gameObject.SetActive(true);
            }
        }

    }

    public void ShowPrevLevel()
    {
        RoomCfg cfg = m_part.GetPrevLevel(m_cfg);
        if (cfg == null)
            return;
        m_cfg = cfg;
        UpdateDetail();
        mBtnNextLevel.gameObject.SetActive(true);
    }

    public void ShowNextLevel()
    {
        RoomCfg cfg = m_part.GetNextLevel(m_cfg);
        if (cfg == null)
        {
            UIMessage.Show(string.Format("请先通关{0}", m_cfg.roomName));
            return;
        }
        m_cfg = cfg;
        UpdateDetail();
        mBtnPreLevel.gameObject.SetActive(true);
        
    }

    private int GetCanCarrayPetNum()
    {
        var canCarryNum = 0;
        var heroLv = m_hero.GetInt(enProp.level);
        PetPosCfg cfg1 = PetPosCfg.m_cfgs[(int)enPetPos.pet1Main];
        if (heroLv >= cfg1.level)
            canCarryNum++;
        PetPosCfg cfg2 = PetPosCfg.m_cfgs[(int)enPetPos.pet2Main];
        if (heroLv >= cfg2.level)
            canCarryNum++;
        return Math.Min(canCarryNum, m_hero.PetsPart.Pets.Count);
    }
    
    private bool CheckPet()
    {
        if (!m_cfg.needPetTip)
            return true;

        //开启了神侍功能才检测是否携带神侍
        var errMsg = "";
        if (SystemMgr.instance.IsEnabled(enSystem.pet, out errMsg))
        {
            //检测是否可以携带更多的神侍
            var petsPart = m_hero.PetsPart;
            var pets = petsPart.GetMainPets();
            //比关卡允许携带的少几只？（有可能多）
            var canCarryMore1 = m_cfg.petNum - pets.Count;
            //比自身允许携带的少几只？
            var canCarryMore2 = GetCanCarrayPetNum() - pets.Count;
            if (canCarryMore1 > 0 && canCarryMore2 > 0)
            {
                var canCarryMore = Math.Min(canCarryMore1, canCarryMore2);
                var tipMsg = pets.Count <= 0 ? LanguageCfg.Get("level_no_pet_tip") : string.Format(LanguageCfg.Get("level_carry_more_pet_tip"), canCarryMore);
                UIMessageBox.Open(tipMsg, () =>
                {
                    if (!CheckPower())
                        return;

                    m_part.EnterLevel(m_info);
                }, () =>
                {
                    if (TeachMgr.instance.PlayNow)
                        UIMgr.instance.Open<UIChoosePet>();
                    else
                        TeachMgr.instance.PlayTeach("guanqiashenshi");
                }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"));
                return false;
            }
        }

        return true;
    }

    private bool CheckPower()
    {
        if (!m_cfg.needPowerTip)
            return true;

        var ratio = (float)m_hero.GetInt(enProp.powerTotal) / m_cfg.powerNum;
        var diff = PowerFactorCfg2.Get(ratio);
        if (diff.difficulty >= PowerDifficulty.danger)
        {
            var tipMsg = LanguageCfg.Get("level_power_tip");
            UIMessageBox.Open(tipMsg, () =>
            {
                m_part.EnterLevel(m_info);
            }, () =>
            {
            }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"));
            return false;
        }

        return true;
    }

    public void EnterRoom()
    {
        if (!CheckPet())
            return;

        if (!CheckPower())
            return;

        m_part.EnterLevel(m_info);
    }

    public void UpdateDetail()
    {
        m_info = m_part.GetLevelInfoById(m_cfg.id);
        int n = int.Parse(m_cfg.id);
        m_curIdx = n - (n / 10 * 10);
        m_curIdx = m_curIdx == 0 ? 10 : m_curIdx;

        mTitle.text = m_cfg.roomName;
        int times = m_part.GetEnterNum(m_info);
        mChallengeNum.text = string.Format("{0}/{1}", m_cfg.maxChallengeNum - times, m_cfg.maxChallengeNum);
        mCostNum.text = string.Format("{0}/{1}", m_hero.GetStamina(), m_cfg.staminaCost);
        float power = (float)m_hero.GetInt(enProp.powerTotal) / m_cfg.powerNum;
        mFightNum.text = string.Format(PowerFactorCfg2.Get(power).desc, m_hero.GetInt(enProp.powerTotal), m_cfg.powerNum);

        mStory.text = m_cfg.roomStory;
        mImage.Set(m_cfg.roomSprite);
        mCurIdx.text = m_curIdx.ToString();

        if (m_cfg.petNum >= 2)
            mPetLimitDesc.gameObject.SetActive(false);
        else if (m_cfg.petNum == 1)
        {
            mPetLimitDesc.gameObject.SetActive(true);
            mPetLimitDesc.text = petLimitDesc[1];
        }
        else if (m_cfg.petNum <= 0)
        {
            mPetLimitDesc.gameObject.SetActive(true);
            mPetLimitDesc.text = petLimitDesc[0];
        }

        onPet1MainChanged();
        onPet2MainChanged();
        
        mRewardGroup.SetCount(m_cfg.rewardShow.Length);
        for (int i = 0; i < m_cfg.rewardShow.Length; i++)
        {
            mRewardGroup.Get<UIItemIcon>(i).Init(m_cfg.rewardShow[i][0], m_cfg.rewardShow[i][1]);
            mRewardGroup.Get<UIItemIcon>(i).isSimpleTip = true;
        }

        List<int> taskIds = m_cfg.GetTaskIdList();
        if (taskIds.Count != 3)
            Debuger.LogError("配置的通关条件不是3个");
        else
        {
            for (int i = 0; i < mConditions.Count; i++)
            {
                mConditions[i].text = "";
                RoomConditionCfg conditionCfg = RoomConditionCfg.GetCfg(taskIds[i]);
                int num = 0;
                if (m_info.starsInfo.TryGetValue(conditionCfg.id.ToString() , out num) && num > 0)
                {
                    mStars2[i].gameObject.SetActive(true);
                    mConditions[i].text = string.Format("<color=green>{0}</color>", conditionCfg.endDesc);
                }
                else
                {
                    mConditions[i].text = conditionCfg.endDesc;
                    mStars2[i].gameObject.SetActive(false);
                }
            }
        }

        if (m_curIdx <= 1)
        {
            mBtnPreLevel.gameObject.SetActive(false);
            mBtnNextLevel.gameObject.SetActive(true);
        }
        else if (m_curIdx >= 10)
        {
            mBtnPreLevel.gameObject.SetActive(true);
            mBtnNextLevel.gameObject.SetActive(false);
        }
        else
        {
            mBtnPreLevel.gameObject.SetActive(true);
            mBtnNextLevel.gameObject.SetActive(true);
        }

        mLevelDesc.text = "容易";

    }

    public override void OnOpenPanel(object param)
    {
        m_cfg = param as RoomCfg;
        if (m_cfg == null)
        {
            Debug.LogError("打开错误 传入关卡配置为空");
            return;
        }
        
        m_hero = RoleMgr.instance.Hero;
        if (m_hero == null)
            return;

        m_part = m_hero.LevelsPart;
        m_info = m_part.GetLevelInfoById(m_cfg.id);
        UpdateDetail();
        m_pet1Ob = m_hero.Add(MSG_ROLE.PET_FORMATION_CHANGE, onPet1MainChanged);
        m_pet2Ob = m_hero.Add(MSG_ROLE.PET_FORMATION_CHANGE, onPet2MainChanged);
        onPet1MainChanged();
        onPet2MainChanged();


        Role hero = RoleMgr.instance.Hero;
        PetPosCfg cfg1 = PetPosCfg.m_cfgs[(int)enPetPos.pet1Main];
        PetPosCfg cfg2 = PetPosCfg.m_cfgs[(int)enPetPos.pet2Main];
        bool open1 = false;
        bool open2 = false;
        int heroLevel = hero.GetInt(enProp.level);
        if (heroLevel >= cfg1.level)
            open1 = true;
        if (heroLevel >= cfg2.level)
            open2 = true;

        string msg;
        bool bOpen = SystemMgr.instance.IsEnabled(enSystem.pet, out msg);
        mBtnPet1.gameObject.SetActive(bOpen && open1);
        mBtnPet2.gameObject.SetActive(bOpen && open2);
        if (!bOpen)
            mPetLimitDesc.gameObject.SetActive(false);
    }

    public override void OnClosePanel()
    {
        if (m_pet1Ob != EventMgr.Invalid_Id) { EventMgr.Remove(m_pet1Ob); m_pet1Ob = EventMgr.Invalid_Id; }
        if (m_pet2Ob != EventMgr.Invalid_Id) { EventMgr.Remove(m_pet2Ob); m_pet2Ob = EventMgr.Invalid_Id; }

    }
    
}
