using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public class AspectRatioFitterEx : AspectRatioFitter
{
    protected override void Start()
    {
        if (this.aspectMode != AspectRatioFitter.AspectMode.EnvelopeParent)
            this.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        var img = this.GetComponent<Image>();
        if (img != null)
        {
            if (img.overrideSprite == null)
                SetAspectRatio(1.0f);
            else
                SetAspectRatio(img.overrideSprite.rect.width / img.overrideSprite.rect.height);
        }
    }

    public void SetAspectRatio(float ratio)
    {
        if (this.aspectRatio != ratio)
            this.aspectRatio = ratio;
    }
}
