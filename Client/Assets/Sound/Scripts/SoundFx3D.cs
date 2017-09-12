using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SoundFx3D : MonoBehaviour
{
    #region Fields
    public AudioSource m_source;
    #endregion

    #region Mono Frame

    void Update()
    {
    }

    void OnDisable()
    {
        if (SoundMgr.instance != null)
            SoundMgr.instance.Stop(this);
    }
    #endregion

    #region Private Methods
    #endregion


}
