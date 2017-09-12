using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class GradientEffect : BaseMeshEffect
{
    [SerializeField]
    private Color32
        topColor = Color.white;
    [SerializeField]
    private Color32
        bottomColor = Color.black;
    [SerializeField]
    private bool m_UseGraphicAlpha = false;
    [SerializeField]
    private bool m_UseRGB = false;

     
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> output = TypePool<List<UIVertex>>.Get();
        vh.GetUIVertexStream(output);

        

        int count = output.Count;
        if (count > 0)
        {
            float bottomY = output[0].position.y;
            float topY = output[0].position.y;

            for (int i = 1; i < count; i++)
            {
                float y = output[i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }
            UIVertex v;
            float uiElementHeight = topY - bottomY;
            for (int i = 0; i < count; i++)
            {
                v = output[i];
                
                if (!m_UseGraphicAlpha&&!m_UseRGB)
                {
                    v.color = Color32.Lerp(bottomColor, topColor, (output[i].position.y - bottomY) / uiElementHeight);
                }
                else
                {
                    Color32 c = Color32.Lerp(bottomColor, topColor, (output[i].position.y - bottomY) / uiElementHeight);
                    if (m_UseRGB)
                    {
                        c.r = (byte)((int)c.r + (int)v.color.r - 255);
                        c.g = (byte)((int)c.g + (int)v.color.g - 255);
                        c.b = (byte)((int)c.b + (int)v.color.b - 255);
                    }
                    if(m_UseGraphicAlpha){
                        c.a = (byte)((c.a * v.color.a) / 255);
                    }
                    v.color =c;
                }
                    
                output[i] = v;
            }
            
        }


        vh.Clear();
        vh.AddUIVertexTriangleStream(output);
        output.Clear();
        TypePool<List<UIVertex>>.Put(output);
    }


    
}