using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PetIconData
{
    public PetIconData(Role pet, enPetPos pos, bool canDropOut)
    {
        this.pet = pet;
        this.pos = pos;
        this.canDropOut = canDropOut;
    }
    public Role pet;
    public enPetPos pos;
    public bool canDropOut;
}

public class UIPetBattleIcon : MonoBehaviour
{
    public ImageEx m_icon;
    public StateHandle m_button;
    public DropControl m_drop;
    public DragControl m_dragIcon;
    public TextEx m_limitText;
    public StateGroup m_stars;
    private enPetPos m_pos;
    private Role m_pet;
    private enPetFormation m_petFormationId = enPetFormation.normal;

    public void Init(enPetPos pos)
    {
        m_pos = pos;
        m_button.AddClick(OnClick);
        m_dragIcon.m_initCopy = InitCopyObj;
        m_dragIcon.m_onDrag = OnDrag;
        m_dragIcon.m_onEndDrag = OnEndDrag;
        m_drop.m_onDrop = OnDropPet;
    }

    public void SetFormationId(enPetFormation petFormationId)
    {
        m_petFormationId = petFormationId;
    }

    void OnDrag()
    {
        m_icon.SetGrey(true);
    }

    void OnEndDrag()
    {
        m_icon.SetGrey(false);
    }

    public void UpdatePet()
    {
        Role hero = RoleMgr.instance.Hero;
        PetFormation petFormation = hero.PetFormationsPart.GetPetFormation(m_petFormationId);
        string guid = petFormation.GetPetGuid(m_pos);
        Role pet = null;
        if (string.IsNullOrEmpty(guid))
        {
            m_icon.Set("ui_tongyong_icon_transparent");
            m_dragIcon.m_canDrag = false;
            if(m_stars!=null)
            {
                m_stars.SetCount(0);
            }
        }
        else
        {
            pet = hero.PetsPart.GetPet(guid);
            if (pet == null)
            {
                m_icon.Set("ui_tongyong_icon_transparent");
                m_dragIcon.m_canDrag = false;
            }
            else
            {
                m_icon.Set(pet.Cfg.icon);
                m_dragIcon.m_canDrag = true;
            }
            if (m_stars != null)
            {
                m_stars.SetCount(pet.GetInt(enProp.star));
            }
        }
        
        m_pet = pet;
        m_dragIcon.m_data = new PetIconData(pet, m_pos, true);

        PetPosCfg cfg = PetPosCfg.m_cfgs[(int)m_pos];
        if (hero.GetInt(enProp.level)>=cfg.level)
        {
            m_limitText.gameObject.SetActive(false);
        }
        else
        {
            m_limitText.gameObject.SetActive(true);
            m_limitText.text = "Lv."+cfg.level + "解锁";
        }
    }

    void OnClick()
    {
        if (m_pet == null) return;
        UIMgr.instance.Open<UIPet>(m_pet);
    }

    void InitCopyObj(GameObject gameObject)
    {
        gameObject.GetComponent<ImageEx>().Set(m_pet.Cfg.icon);
    }

    void OnDropPet(object data)
    {

        PetIconData iconData = data as PetIconData;
        if(iconData == null)
        {
            return;
        }

        Role hero = RoleMgr.instance.Hero;
        PetPosCfg cfg = PetPosCfg.m_cfgs[(int)m_pos];
        if (hero.GetInt(enProp.level) < cfg.level)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_PET, RESULT_CODE_PET.PET_POS_NEED_LEVEL));
            return;
        }

        PetFormation petFormation = hero.PetFormationsPart.GetPetFormation(m_petFormationId);
        string guid = petFormation.GetPetGuid(m_pos);
        if (guid == iconData.pet.GetString(enProp.guid))
        {
            return;
        }
        UIPowerUp.SaveOldProp(RoleMgr.instance.Hero);

        NetMgr.instance.PetHandler.SendChoosePet(iconData.pet.GetString(enProp.guid), m_petFormationId, m_pos);
    }

}
