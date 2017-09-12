using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class SimpleBlur : PostEffectsBase
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Art/特效/Image Effects/SimpleBlur")]
    static void Add()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            SimpleBlur comp = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<SimpleBlur>();
            comp.overlayShader = Shader.Find("Hidden/SimpleBlur");
            
            EditorUtil.SetDirty(comp);
        }
    }
#endif
    public static Vector2 s_center = new Vector2(0.5f, 0.5f);
    [Range(0,1)]
    public float sampleDist = 0.3f;
    
    public Shader overlayShader = null;
    

    private Material overlayMaterial = null;


    public override bool CheckResources()
    {
        CheckSupport(false);

        overlayMaterial = CheckShaderAndCreateMaterial(overlayShader, overlayMaterial);

        if (!isSupported)
            ReportAutoDisable();
        return isSupported;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        overlayMaterial.SetFloat("_SampleDist", sampleDist);
        
        Graphics.Blit(source, destination, overlayMaterial);
    }
}
