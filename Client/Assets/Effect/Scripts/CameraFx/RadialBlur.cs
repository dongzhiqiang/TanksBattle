using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
//[AddComponentMenu("Image Effects/扩展/RadialBlur")]
public class RadialBlur : PostEffectsBase
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Art/特效/Image Effects/RadialBlur")]
    static void AddRadialBlur()
    {
        if (UnityEditor.Selection.activeGameObject != null)
        {
            RadialBlur comp = UnityEditor.Selection.activeGameObject.AddComponentIfNoExist<RadialBlur>();
            comp.overlayShader = Shader.Find("Xffect/PP/radial_blur");
            comp.target = UnityEditor.Selection.activeGameObject.transform;
            EditorUtil.SetDirty(comp);
        }
    }
#endif
    public static Vector2 s_center = new Vector2(0.5f, 0.5f);

    public float sampleDist = 0.3f;
    public float strength = 1f;
    public Shader overlayShader = null;
    public Transform target = null;
    public Vector3 offset = Vector3.zero;

    private Material overlayMaterial = null;


    public override bool CheckResources()
    {
        CheckSupport(false);

        overlayMaterial = CheckShaderAndCreateMaterial(overlayShader, overlayMaterial);

        if (!isSupported)
            ReportAutoDisable();
        return isSupported;
    }

    Vector2 GetCenter()
    {
        if (target == null)
            return s_center;


        Camera c = null;
#if UNITY_EDITOR
        c = Camera.main;
#else
            c = CameraMgr.instance==null?null:CameraMgr.instance.CurCamera;
#endif
        if (c == null)
        {
            return s_center;
        }
        //Vector3 v3 = c.WorldToViewportPoint(target.position);
        //v3.y = 1 - v3.y;
        //Debuger.LogError(v3.ToString());
        Vector3 pos;
        if(offset == Vector3.zero)
            pos = c.WorldToScreenPoint(target.position);
        else
            pos = c.WorldToScreenPoint(target.TransformPoint(offset));
         
        pos.x = pos.x / Screen.width;
        pos.y = pos.y / Screen.height;
        return pos;

    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        overlayMaterial.SetFloat("_SampleDist", sampleDist);
        overlayMaterial.SetFloat("_SampleStrength", strength);
        Vector2 v2 = GetCenter();
        Vector4 v4 = Vector4.zero;
        v4.x = v2.x;
        v4.y = v2.y;
        overlayMaterial.SetVector("_Center", v4);
        Graphics.Blit(source, destination, overlayMaterial);
    }
}
