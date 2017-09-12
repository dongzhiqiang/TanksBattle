using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VolumetricFog))]
public class VolumetricFogInspector : Editor
{
    private void OnEnable()
    {
        VolumetricFog fog = target as VolumetricFog;
        fog.UpdateVolumetricFog();
        SceneView.RepaintAll();
    }

    private void SetDontDynamicSetCamera(bool en)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (en)
        {
            if (!symbols.Contains("VF_DontDynamicSetCamera"))
            {
                symbols += ";VF_DontDynamicSetCamera";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
            }
        }
        else
        {
            if (symbols.Contains("VF_DontDynamicSetCamera"))
            {
                symbols = symbols.Replace("VF_DontDynamicSetCamera", "");
                symbols = symbols.Replace(";;", ";");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
            }
        }
    }

    private bool GetDontDynamicSetCamera()
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        return symbols.Contains("VF_DontDynamicSetCamera");
    }

    private float GetHandleSize(Vector3 pos)
    {
        return HandleUtility.GetHandleSize(pos) * 0.1f;
    }

    private void OnSceneGUI()
    {
        VolumetricFog fog = target as VolumetricFog;
        Color c = Handles.color;
        
        Vector3 newVector, delta;

        Matrix4x4 mat = fog.transform.localToWorldMatrix;
        Vector3 upDir = mat.MultiplyVector(Vector3.up);
        Vector3 downDir = mat.MultiplyVector(Vector3.down);
        Vector3 leftDir = mat.MultiplyVector(Vector3.left);
        Vector3 rightDir = mat.MultiplyVector(Vector3.right);
        Vector3 forwardDir= mat.MultiplyVector(Vector3.forward);
        Vector3 backDir = mat.MultiplyVector(Vector3.back);

        #region Draw Scale Handles;

        Handles.color = Color.green;

        Vector3 left = new Vector3(-0.5f * fog.Size.x,0, 0);
        left = mat.MultiplyVector(left) + fog.transform.position;
        newVector = Handles.Slider2D(left, leftDir, rightDir, forwardDir, GetHandleSize(left), Handles.CubeCap, 1f);
        delta = newVector - left;
        if (!delta.x.Equals(0))
        {
            fog.transform.localPosition += new Vector3(delta.x * 0.5f, 0, 0);
            fog.Size.x += -delta.x;
            if (fog.Size.x < 0)
            {
                fog.Size.x = 0;
            }
            fog.UpdateVolumetricFog();
        }

        Vector3 right = new Vector3(0.5f * fog.Size.x, 0, 0);
        right = mat.MultiplyVector(right) + fog.transform.position;
        newVector = Handles.Slider2D(right, rightDir, rightDir, forwardDir, GetHandleSize(right), Handles.CubeCap, 1f);
        delta = newVector - right;
        if (!delta.x.Equals(0))
        {
            fog.transform.localPosition += new Vector3(delta.x * 0.5f, 0, 0);
            fog.Size.x += delta.x;
            if (fog.Size.x < 0)
            {
                fog.Size.x = 0;
            }
            fog.UpdateVolumetricFog();
        }

        Vector3 forward = new Vector3(0, 0, 0.5f * fog.Size.z);
        forward = mat.MultiplyVector(forward) + fog.transform.position;
        newVector = Handles.Slider2D(forward, forwardDir, rightDir, forwardDir, GetHandleSize(forward), Handles.CubeCap, 1f);
        delta = newVector - forward;
        if (!delta.z.Equals(0))
        {
            fog.transform.localPosition += new Vector3(0, 0, delta.z * 0.5f);
            fog.Size.z += delta.z;
            if (fog.Size.z < 0)
            {
                fog.Size.z = 0;
            }
            fog.UpdateVolumetricFog();
        }

        Vector3 back = new Vector3(0, 0, -0.5f * fog.Size.z);
        back = mat.MultiplyVector(back) + fog.transform.position;
        newVector = Handles.Slider2D(back, backDir, rightDir, forwardDir, GetHandleSize(back), Handles.CubeCap, 1f);
        delta = newVector - back;
        if (!delta.z.Equals(0))
        {
            fog.transform.localPosition += new Vector3(0, 0, delta.z * 0.5f);
            fog.Size.z += -delta.z;
            if (fog.Size.z < 0)
            {
                fog.Size.z = 0;
            }
            fog.UpdateVolumetricFog();
        }

        Vector3 up = new Vector3(0, 0.5f * fog.Size.y, 0);
        up = mat.MultiplyVector(up) + fog.transform.position;
        newVector = Handles.Slider2D(up, upDir, upDir, forwardDir, GetHandleSize(up), Handles.CubeCap, 1f);
        delta = newVector - up;
        if (!delta.y.Equals(0))
        {
            fog.transform.localPosition += new Vector3(0, delta.y * 0.5f, 0);
            fog.Size.y += delta.y;
            if (fog.Size.y < 0)
            {
                fog.Size.y = 0;
            }
            fog.UpdateVolumetricFog();
        }

        Vector3 down = new Vector3(0, -0.5f * fog.Size.y, 0);
        down = mat.MultiplyVector(down) + fog.transform.position;
        newVector = Handles.Slider2D(down, downDir, upDir, forwardDir, GetHandleSize(down), Handles.CubeCap, 1f);
        delta = newVector - down;
        if (!delta.y.Equals(0))
        {
            fog.transform.localPosition += new Vector3(0, delta.y * 0.5f, 0);
            fog.Size.y += -delta.y;
            if (fog.Size.y < 0)
            {
                fog.Size.y = 0;
            }
            fog.UpdateVolumetricFog();
        }

        #endregion

        #region Draw Bound Line;

        Vector3 upLeftForward = Vector3.Scale(fog.Size, new Vector3(-1, 1, 1)) * 0.5f;
        Vector3 downLeftForward = Vector3.Scale(fog.Size, new Vector3(-1, -1, 1)) * 0.5f;

        Vector3 upRightForward = Vector3.Scale(fog.Size, new Vector3(1, 1, 1)) * 0.5f;
        Vector3 downRightForward = Vector3.Scale(fog.Size, new Vector3(1, -1, 1)) * 0.5f;

        Vector3 upLeftBack = Vector3.Scale(fog.Size, new Vector3(-1, 1, -1)) * 0.5f;
        Vector3 downLeftBack = Vector3.Scale(fog.Size, new Vector3(-1, -1, -1)) * 0.5f;

        Vector3 upRightBack = Vector3.Scale(fog.Size, new Vector3(1, 1, -1)) * 0.5f;
        Vector3 downRightBack = Vector3.Scale(fog.Size, new Vector3(1, -1, -1)) * 0.5f;

        upLeftForward = mat.MultiplyVector(upLeftForward) + fog.transform.position;
        downLeftForward = mat.MultiplyVector(downLeftForward) + fog.transform.position;
        upRightForward = mat.MultiplyVector(upRightForward) + fog.transform.position;
        downRightForward = mat.MultiplyVector(downRightForward) + fog.transform.position;
        upLeftBack = mat.MultiplyVector(upLeftBack) + fog.transform.position;
        downLeftBack = mat.MultiplyVector(downLeftBack) + fog.transform.position;
        upRightBack = mat.MultiplyVector(upRightBack) + fog.transform.position;
        downRightBack = mat.MultiplyVector(downRightBack) + fog.transform.position;

        Handles.color = Color.cyan;

        Handles.DrawLine(upLeftBack, upLeftForward);
        Handles.DrawLine(upLeftForward, upRightForward);
        Handles.DrawLine(upRightForward, upRightBack);
        Handles.DrawLine(upRightBack, upLeftBack);

        Handles.DrawLine(downLeftBack, downLeftForward);
        Handles.DrawLine(downLeftForward, downRightForward);
        Handles.DrawLine(downRightForward, downRightBack);
        Handles.DrawLine(downRightBack, downLeftBack);

        Handles.DrawLine(upLeftForward, downLeftForward);
        Handles.DrawLine(upRightForward, downRightForward);
        Handles.DrawLine(upLeftBack, downLeftBack);
        Handles.DrawLine(upRightBack, downRightBack);

        #endregion

        #region Draw Fade Handles

        
        Vector3[] posStart =
        {
            new Vector3(-0.5f*fog.Size.x, (fog.StartFade - 0.5f)*fog.Size.y, 0.5f*fog.Size.z),
            new Vector3(0.5f*fog.Size.x, (fog.StartFade - 0.5f)*fog.Size.y, 0.5f*fog.Size.z),
            new Vector3(-0.5f*fog.Size.x, (fog.StartFade - 0.5f)*fog.Size.y, -0.5f*fog.Size.z),
            new Vector3(0.5f*fog.Size.x, (fog.StartFade - 0.5f)*fog.Size.y, -0.5f*fog.Size.z),
        };
        Vector3[] posEnd =
        {
            new Vector3(-0.5f*fog.Size.x, (fog.EndFade - 0.5f)*fog.Size.y, 0.5f*fog.Size.z),
            new Vector3(0.5f*fog.Size.x, (fog.EndFade - 0.5f)*fog.Size.y, 0.5f*fog.Size.z),
            new Vector3(-0.5f*fog.Size.x, (fog.EndFade - 0.5f)*fog.Size.y, -0.5f*fog.Size.z),
            new Vector3(0.5f*fog.Size.x, (fog.EndFade - 0.5f)*fog.Size.y, -0.5f*fog.Size.z),
        };

        Handles.color = Color.red;
        for (int i = 0; i < 4; i++)
        {
            posStart[i] = mat.MultiplyVector(posStart[i]) + fog.transform.position;
            posEnd[i] = mat.MultiplyVector(posEnd[i]) + fog.transform.position;
            Handles.DrawLine(posStart[i], posEnd[i]);
        }

        Handles.color = Color.yellow;

        for (int i = 0; i < 4; i++)
        {
            newVector = Handles.Slider2D(posStart[i], upDir, upDir, forwardDir, GetHandleSize(posStart[i]), Handles.CylinderCap, 1f);
            delta = newVector - posStart[i];
            if (!delta.y.Equals(0))
            {
                fog.StartFade += delta.y / fog.Size.y;
                if (fog.StartFade > fog.EndFade)
                {
                    fog.StartFade = fog.EndFade - 0.000001f;
                }
                if (fog.StartFade < 0)
                {
                    fog.StartFade = 0;
                }
                fog.UpdateMaterial();
            }

            newVector = Handles.Slider2D(posEnd[i], upDir, upDir, forwardDir, GetHandleSize(posEnd[i]), Handles.CylinderCap, 1f);
            delta = newVector - posEnd[i];
            if (!delta.y.Equals(0))
            {
                fog.EndFade += delta.y / fog.Size.y;
                if (fog.EndFade < fog.StartFade)
                {
                    fog.EndFade = fog.StartFade + 0.000001f;
                }
                if (fog.EndFade > 1)
                {
                    fog.EndFade = 1;
                }
                fog.UpdateMaterial();
            }
        }

        #endregion

        Handles.color = c;
    }

    public override void OnInspectorGUI()
    {
        VolumetricFog fog = target as VolumetricFog;

        GUILayout.Space(5);
        GUILayout.Label("", "ShurikenLine");
        GUILayout.Space(5);
        VolumetricFog.ShapeType type = (VolumetricFog.ShapeType)EditorGUILayout.EnumPopup("Shape Type", fog.Type, "ToolbarPopup");
        if (type != fog.Type)
        {
            fog.Type = type;
            fog.UpdateMesh();
        }

        GUILayout.Space(5);

        float visibility = EditorGUILayout.FloatField("Visibility", fog.Visibility);
        if (!Equals(visibility, fog.Visibility))
        {
            fog.Visibility = visibility;
            fog.UpdateMaterial();
        }

        GUILayout.Space(5);

        VolumetricFog.FogMode mode = (VolumetricFog.FogMode)EditorGUILayout.EnumPopup("Fog Mode", fog.Mode, "ToolbarPopup");
        if (mode != fog.Mode)
        {
            fog.Mode = mode;
            fog.UpdateMaterial();
        }

        GUILayout.Space(5);

        Color color = EditorGUILayout.ColorField("Fog Color",fog.FogColor);
        if (!color.Equals(fog.FogColor))
        {
            fog.FogColor = color;
            fog.UpdateMaterial();
        }

        GUILayout.Space(5);

        float min = fog.StartFade, max = fog.EndFade;
        EditorGUILayout.MinMaxSlider(new GUIContent("Vertical Fade"), ref min, ref max, 0, 1);
        if (!Equals(min, fog.StartFade) || !Equals(max, fog.EndFade))
        {
            fog.StartFade = min;
            fog.EndFade = max;
            fog.UpdateMaterial();
        }

        GUILayout.Space(5);

        VolumetricFog.FadeMode fade = (VolumetricFog.FadeMode)EditorGUILayout.EnumPopup("Fade Type", fog.Fade, "ToolbarPopup");
        if (fade != fog.Fade)
        {
            fog.Fade = fade;
            fog.UpdateMaterial();
        }

        GUILayout.Space(5);

        Vector3 size = EditorGUILayout.Vector3Field("Size", fog.Size);
        if (!size.Equals(fog.Size))
        {
            fog.Size = size;
            fog.UpdateVolumetricFog();
        }

        GUILayout.Space(5);

        VolumetricFog.ViewMode view = (VolumetricFog.ViewMode)EditorGUILayout.EnumPopup("View Mode", fog.View, "ToolbarPopup");
        if (view != fog.View)
        {
            fog.View = view;
            fog.UpdateMaterial();
        }

        if (fog.View == VolumetricFog.ViewMode.ThirdPerson)
        {
            GUILayout.Space(5);
            fog.ThirdPerson = EditorGUILayout.ObjectField("Third Person", fog.ThirdPerson, typeof (Transform),true) as Transform;
        }
        else
        {
            GUILayout.Space(5);
            bool culloff= EditorGUILayout.Toggle(new GUIContent("Cull Off", "Render frontface and backface of this volume."), fog.CullOff);
            if (culloff != fog.CullOff)
            {
                fog.CullOff = culloff;
                fog.UpdateMaterial();
            }
        }

        GUILayout.Space(5);

        bool dontDynamicSetCamera = GetDontDynamicSetCamera();
        bool newValue = EditorGUILayout.Toggle("Don't Dynamic detection camera", dontDynamicSetCamera);
        if (newValue != dontDynamicSetCamera)
        {
            SetDontDynamicSetCamera(newValue);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Update", "toolbarbutton"))
        {
            fog.UpdateVolumetricFog();
        }

        GUILayout.Space(5);
        GUILayout.Label("", "ShurikenLine");
        GUILayout.Space(5);
    }

    [MenuItem("GameObject/Create Other/Volumetric Fog")]
    static public void CreateVolumetricFog()
    {
        GameObject newObject = new GameObject("Volumetric Fog");
        if (SceneView.currentDrawingSceneView) SceneView.currentDrawingSceneView.MoveToView(newObject.transform);
        newObject.AddComponent<VolumetricFog>();
    }
}
