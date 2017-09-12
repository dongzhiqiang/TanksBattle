using UnityEngine;
using System;
using System.Collections;


public class AutoBeginZoomGroup : IDisposable
{
    private static Matrix4x4 prevGuiMatrix;
    

    public AutoBeginZoomGroup(Rect guiRect, float zoomScale)
    {
        Rect rect = guiRect.ScaleSizeBy(1f / zoomScale, guiRect.min);
        
        GUI.BeginGroup(rect);
        prevGuiMatrix = GUI.matrix;
        Matrix4x4 lhs = Matrix4x4.TRS(rect.min, Quaternion.identity, Vector3.one);
        Vector3 one = Vector3.one;
        one.y = zoomScale;
        one.x = zoomScale;
        Matrix4x4 rhs = Matrix4x4.Scale(one);
        GUI.matrix = lhs * rhs * lhs.inverse * GUI.matrix;
    }

 
    public void Dispose()
    {
        GUI.matrix = prevGuiMatrix;
        GUI.EndGroup();
    }
}
