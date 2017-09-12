#region Header
/**
 * 名称：进度条
 
 * 日期：2016.2.29
 * 描述：支持多条,支持渐变
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UILevelPetHead : MonoBehaviour
{
   public UISmoothProgress progress;
   public ImageEx head;

   public Role Pet
   {
       get { return m_pet; }
       set { m_pet = value; }
   }

   Role m_pet;
   int m_observer;
   int m_observer2;
   int m_observer3;   

   public void Init(Role pet){
       if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
       if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
       if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }
        
        m_pet = pet;

        if (m_pet == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        head.Set(m_pet.Cfg.icon);

        m_observer = m_pet.Add(MSG_ROLE.BORN, OnRoleBorn);
        if (m_pet.State == Role.enState.alive)
            OnRoleBorn();

        this.gameObject.SetActive(true);
   }

   public void Close()
   {
       if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
       if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
       if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }
   }

   void OnRoleBorn()
   {
       float f = m_pet.GetInt(enProp.hp) / m_pet.GetFloat(enProp.hpMax);
       progress.SetProgress(f, true);

       if (m_observer2 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer2); m_observer2 = EventMgr.Invalid_Id; }
       if (m_observer3 != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer3); m_observer3 = EventMgr.Invalid_Id; }

       m_observer2 = m_pet.AddPropChange(enProp.hp, OnHp);
       m_observer3 = m_pet.AddPropChange(enProp.hpMax, OnHp);

   }

   void OnHp()
   {
       float f = m_pet.GetInt(enProp.hp) / m_pet.GetFloat(enProp.hpMax);
       progress.SetProgress(f, false);
   }
}
