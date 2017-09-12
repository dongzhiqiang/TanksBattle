using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;


public class UIPetBrief : MonoBehaviour
{
    public ImageEx m_icon;
    public Text m_name;
    public Text m_type;
    public Text m_level;
    public StateHandle m_button;
    public DragControl m_dragIcon;
    public StateGroup m_stars;
    public StateHandle m_battleState;
    public ImageEx m_pieceBar;
    public Text m_pieceNum;
    public StateHandle m_piece;
    public StateHandle m_recruit;
    public GameObject m_operation;
    private PetInfo m_petInfo;
    private bool m_eventAdded = false;
    private Role m_pet;
    private string m_petId;

    public Role Pet
    {
        get { return m_pet; }
        set { m_pet = value; }
    }

    public string PetId
    {
        get { return m_petId; }
        set { m_petId = value; }
    }

    public void Init(Role pet, string petId)
    {
        Pet = pet;
        PetId = petId;
        if (pet != null)
        {
            m_level.text = "Lv." + pet.PropPart.GetInt(enProp.level);
            PetAdvLvPropRateCfg cfg = PetAdvLvPropRateCfg.m_cfgs[pet.PropPart.GetInt(enProp.advLv)];
            string name = pet.Cfg.name;
            if (cfg.qualityLevel > 0)
            {
                name = name + "+" + cfg.qualityLevel;
            }
            m_name.text = name;
            m_name.color = QualityCfg.GetColor(cfg.quality);
            m_type.text = PetTypeCfg.m_cfgs[pet.Cfg.subType].name;
            m_icon.Set(pet.Cfg.icon);

            m_dragIcon.m_data = new PetIconData(pet, enPetPos.pet1Main, false);
            m_stars.SetCount(pet.GetInt(enProp.star));
            if (Pet.PetsPart.Owner.PetsPart.IsMainPet(Pet.GetString(enProp.guid)))
            {
                m_battleState.SetState(0);
            }
            else if (Pet.PetsPart.Owner.PetsPart.IsSubPet(Pet.GetString(enProp.guid)))
            {
                m_battleState.SetState(1);
            }
            else
            {
                m_battleState.SetState(2);
            }

            m_icon.SetGrey(false);

            m_dragIcon.m_canDrag = true;

            if (m_operation != null)
            {
                m_operation.SetActive(pet.PetsPart.IsBattle() && (pet.PetsPart.CanOperate() || pet.EquipsPart.HasEquipCanOperate()));
            }
        }
        else
        {
            PetAdvLvPropRateCfg cfg = PetAdvLvPropRateCfg.m_cfgs[1];
            RoleCfg petCfg = RoleCfg.Get(petId);
            string name = petCfg.name;
            if (cfg.qualityLevel > 0)
            {
                name = name + "+" + cfg.qualityLevel;
            }
            m_name.text = name;
            m_name.color = QualityCfg.GetColor(cfg.quality);
            m_type.text = PetTypeCfg.m_cfgs[petCfg.subType].name;
            m_icon.Set(petCfg.icon);

            m_stars.SetCount(petCfg.initStar);
            int pieceNum = RoleMgr.instance.Hero.ItemsPart.GetItemNum(petCfg.pieceItemId);
            if (pieceNum >= petCfg.pieceNum)
            {
                m_battleState.SetState(3);
            }
            else
            {
                if(m_battleState != null)
                {
                    m_battleState.SetState(4);
                }
            }
            float percent = (float)pieceNum / petCfg.pieceNum;
            if (percent > 1) percent = 1;
            m_pieceBar.fillAmount = percent;
            m_pieceNum.text = pieceNum + "/" + petCfg.pieceNum;

            m_icon.SetGrey(true);

            m_dragIcon.m_canDrag = false;

            if(m_operation != null)
            {
                m_operation.SetActive(false);
            }
        }

        if (!m_eventAdded)
        {
            m_button.AddClick(OnClick);
            m_eventAdded = true;
            m_dragIcon.m_initCopy = InitCopyObj;
            m_dragIcon.m_onDrag = OnDrag;
            m_dragIcon.m_onEndDrag = OnEndDrag;
            if(m_piece!=null)
            {
                m_piece.AddClick(OnPiece);
            }
            if(m_recruit!=null)
            {
                m_recruit.AddClick(OnRecruit);
            }
        }
    }

    void OnDrag()
    {
        m_icon.SetGrey(true);
    }

    void OnEndDrag()
    {
        m_icon.SetGrey(false);
    }

    void OnClick()
    {
        if(Pet != null)
        {
            UIMgr.instance.Open<UIPet>(Pet);
            return;
        }
        else
        {
            if (m_petInfo==null)
            {
                m_petInfo = new PetInfo();
            }
            m_petInfo.petId = PetId;
            m_petInfo.star = m_petInfo.Cfg.initStar;
            UIMgr.instance.Open<UIPetInfo>(m_petInfo);
        }
    }

    void OnPiece()
    {
        RoleCfg petCfg = RoleCfg.Get(PetId);
        UIMgr.instance.Open<UIItemInfo>(petCfg.pieceItemId);
    }

    void OnRecruit()
    {
        NetMgr.instance.PetHandler.SendRecruitPet(PetId);
    }

    void InitCopyObj(GameObject gameObject)
    {
        /*
        foreach(ImageEx imageEx in gameObject.GetComponentsInChildren<ImageEx>())
        {
            if(imageEx != gameObject.GetComponent<ImageEx>())
            {
                imageEx.Set(m_pet.Cfg.icon);
            }
        }*/
        gameObject.GetComponent<ImageEx>().Set(Pet.Cfg.icon);
    }

}
