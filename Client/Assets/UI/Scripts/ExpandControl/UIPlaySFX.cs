using UnityEngine;
using System.Collections;

/// <summary>
/// 特殊音效脚本
/// </summary>
public class UIPlaySFX : MonoBehaviour
{
#if !ART_DEBUG
    public int soundId = 3;
    public bool isLoop;

    void Awake()
    {
        StateHandle stateHandle = this.GetComponent<StateHandle>();
        if (stateHandle != null)
            stateHandle.AddClick(playUIsound);
    }

    void playUIsound()
    {
        SoundMgr.instance.Play2DSound(Sound2DType.ui, soundId);
    }
#endif
}
