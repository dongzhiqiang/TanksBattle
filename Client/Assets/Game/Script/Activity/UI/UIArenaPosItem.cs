using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIArenaPosItem : MonoBehaviour {
    public ImageEx icon;
    public TextEx powerTxt;
    public ImageEx kuang;
    public ImageEx shadow;
    public GameObject power;
    public Sprite petKuang;
    public Sprite heroKuang;
    public Sprite petShadow;
    public Sprite heroShadow;
    public DropControl drop;
    public DragControl dragIcon;
    bool isSetAction = false;
    Role role;
    int posIndex;

    public void Init(Role role,int posIndex)
    {
        this.posIndex = posIndex;
        if (dragIcon != null && drop != null)
        {
            dragIcon.m_data = posIndex;            
            SetAction();
        }
        
        if (role != null)
        {
            int heroId = role.GetInt(enProp.heroId);
            kuang.sprite = heroId != 0 ? heroKuang : petKuang;
            kuang.SetNativeSize();
            shadow.sprite = heroId != 0 ? heroShadow : petShadow;
            shadow.SetNativeSize();

            this.role = role;                        
            icon.gameObject.SetActive(true);
            power.SetActive(true);
            icon.Set(role.Cfg.icon);
            icon.SetNativeSize();           
            powerTxt.text = heroId != 0 ? role.GetInt(enProp.powerTotal).ToString(): role.GetInt(enProp.power).ToString();
        }
        else
        {
            kuang.sprite = petKuang;
            kuang.SetNativeSize();
            shadow.sprite = petShadow;
            shadow.SetNativeSize();
            icon.gameObject.SetActive(false);
            power.SetActive(false);
        }
    }

    void SetAction()
    {
        if(!isSetAction)
        {
            isSetAction = true;
            dragIcon.m_initCopy = InitCopyObj;
            dragIcon.m_onDrag = OnDrag;
            dragIcon.m_onEndDrag = OnEndDrag;
            drop.m_onDrop = OnDrop;
        }
    }

    void OnDrag()
    {
       
    }

    void OnEndDrag()
    {
       
    }
       
    void OnClick()
    {
       
    }

    void InitCopyObj(GameObject gameObject)
    {
        gameObject.GetComponent<ImageEx>().Set(role.Cfg.icon);
    }

    void OnDrop(object data)
    {
        int newPosIndex = (int)data;        
        Role hero = RoleMgr.instance.Hero;
        string ArenaPos = hero.ActivityPart.GetString(enActProp.arenaPos);
        string curPos = ArenaPos == "" ? "1,0,2" : ArenaPos;

        List<int> pos = ArenaBasicCfg.GetArenaPos(curPos);             
        int temp = -1;
        int indexOld=0, indexNew=0;
        for (int i = 0; i < pos.Count; ++i)
        {
            if (pos[i] == posIndex)
                indexOld = i;
            if (pos[i] == newPosIndex)
                indexNew = i;
        }
        temp = pos[indexOld];
        pos[indexOld] = pos[indexNew];
        pos[indexNew] = temp;

        string newPos = string.Format("{0},{1},{2}", pos[0], pos[1], pos[2]);       

        NetMgr.instance.ActivityHandler.SendReqSetArenaPos(newPos);




    }

}


