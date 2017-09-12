using UnityEngine;
using System.Collections;

public class SoundTest : MonoBehaviour
{
#if !ART_DEBUG

    void OnEnable()
    {
        SoundCfg.CheckInit();
        SoundUICfg.CheckInit();
    }

    void Update()
    {
    }


#endif
}
