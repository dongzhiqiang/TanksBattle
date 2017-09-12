using UnityEngine;
using System;
using System.Collections;


public class AutoStyleMargin : IDisposable
{
    GUIStyle style;
    RectOffset margin;//边缘，在GUILayout类函数下起作用，和其他控件的距离
    RectOffset overflow;//溢出区域，也就是在margin(和其他控件的距离)固定的情况下，背景部分再画多出去多少
    RectOffset padding;//内容和控件大小(也就是背景)的距离

    public AutoStyleMargin(GUIStyle style, RectOffset margin) : this(style, margin, style.padding, style.overflow){}

    public AutoStyleMargin(GUIStyle style, RectOffset margin, RectOffset padding) : this(style, margin, padding, style.overflow){}

    public AutoStyleMargin(GUIStyle style, RectOffset margin, RectOffset padding, RectOffset overflow)
    {
        this.style = style;
        this.margin = margin;
        this.padding = padding;
        this.overflow = overflow;
        style.margin = this.margin;
        style.padding = this.padding;
        style.overflow = this.overflow;
    }

    public void Dispose()
    {
        style.margin = this.margin;
        style.padding = this.padding;
        style.overflow = this.overflow;
    }
}