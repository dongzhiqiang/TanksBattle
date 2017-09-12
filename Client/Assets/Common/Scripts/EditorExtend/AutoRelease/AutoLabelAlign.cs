using UnityEngine;
using System;
using System.Collections;

public class AutoLabelAlign: IDisposable
{
    TextAnchor old;
    public AutoLabelAlign(TextAnchor a)
    {
        old = GUI.skin.label.alignment;
        GUI.skin.label.alignment = a;
    }

    public void Dispose()
    {
        GUI.skin.label.alignment = old;
    }
}
