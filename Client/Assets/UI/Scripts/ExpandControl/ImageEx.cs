using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

public class ImageEx : Image
{
    static Material s_grey;
    static Sprite s_transparent;
    public bool m_grey = false;

    public static Material GreyMaterial
    {
        get
        {
            if (s_grey == null)
            {
                s_grey = Resources.Load<Material>("UIGrey");
            }

            return s_grey;
        }
    }

    public static Sprite Transparent{
        get
        {
            if (s_transparent == null)
            {
                s_transparent = UIMgr.GetSprite("ui_tongyong_icon_transparent");
            }

            return s_transparent;
        }
    }

    public bool IsGrey { get { return m_grey; } }

    public void Set(string spriteName)
    {
        if (this.overrideSprite == null && string.IsNullOrEmpty(spriteName) )
            return;
        if (this.overrideSprite != null && spriteName == this.overrideSprite.name)
            return;

        if(string.IsNullOrEmpty(spriteName)){

            this.overrideSprite = Transparent;
            return;
        }

        //color = Color.white;
        this.overrideSprite = UIMgr.GetSprite(spriteName);

    }

    public void SetGrey(bool grey)
    {
        if (m_grey == grey)
            return;
        m_grey= grey;
        this.material = grey ? GreyMaterial : null;
        EditorUtil.SetDirty(this);
    }

    public override void SetAllDirty()
    {
        base.SetAllDirty();

        var aspectFitter = this.GetComponent<AspectRatioFitterEx>();
        if (aspectFitter != null)
        {
            if (overrideSprite == null)
                aspectFitter.SetAspectRatio(1.0f);
            else
                aspectFitter.SetAspectRatio(overrideSprite.rect.width / overrideSprite.rect.height);
        }
    }
}

