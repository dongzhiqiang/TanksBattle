using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Mask))]
public class UIParticleMask : MonoBehaviour
{
    private const string SHADER_NAME = "Particles/UIParticleMask";
    private RectTransform rectTransform;
    private Vector2 lastMin;
    private Vector2 lastMax;    

    void Start()
    {
        rectTransform = transform as RectTransform;
        UpdateLimitRect();
    }

    void Update()
    {
        Vector2 min = rectTransform.TransformPoint(rectTransform.rect.min);
        Vector2 max = rectTransform.TransformPoint(rectTransform.rect.max);
        if (lastMin != min || lastMax != max)
        {
           UpdateLimitRect();
        }
    }

    public void UpdateLimitRect()
    {
        Vector2 min = rectTransform.TransformPoint(rectTransform.rect.min);
        Vector2 max = rectTransform.TransformPoint(rectTransform.rect.max);
        lastMin = min;
        lastMax = max;
        float minX = min.x;
        float minY = min.y;
        float maxX = max.x;
        float maxY = max.y;
        ParticleSystem[] pss = transform.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem o in pss)
        {
            var temp = o.GetComponent<Renderer>();
            Material mat = temp.sharedMaterial;
            //如果本身就是副本，那就直接用sharedMaterial，如果不是那就得用副本模式
            if (!mat.name.EndsWith("(Clone)"))
                mat = temp.material;
            if (mat.shader.name != SHADER_NAME)
                mat.shader = Shader.Find(SHADER_NAME);
            mat.SetFloat("_MinX", minX);
            mat.SetFloat("_MinY", minY);
            mat.SetFloat("_MaxX", maxX);
            mat.SetFloat("_MaxY", maxY);
        }
        MaterialFx[] mfs = transform.GetComponentsInChildren<MaterialFx>(true);
        foreach (MaterialFx o in mfs)
        {
            var temp = o.GetComponent<Renderer>();
            //如果本身就是副本，那就直接用sharedMaterial，如果不是那就得用副本模式
            Material mat = temp.sharedMaterial;
            if (!mat.name.EndsWith("(Clone)"))
                mat = temp.material;
            if (mat.shader.name != SHADER_NAME)
                mat.shader = Shader.Find(SHADER_NAME);
            mat.SetFloat("_MinX", minX);
            mat.SetFloat("_MinY", minY);
            mat.SetFloat("_MaxX", maxX);
            mat.SetFloat("_MaxY", maxY);
        }
    }
}
