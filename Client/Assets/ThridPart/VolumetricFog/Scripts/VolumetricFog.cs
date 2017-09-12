using System.Diagnostics;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VolumetricFog : MonoBehaviour
{
    public enum ShapeType
    {
        Cube,
        Cylinder
    }

    public enum FogMode
    {
        Exp,
        Linear
    }

    public enum FadeMode
    {
        Smooth,
        Linear
    }

    public enum ViewMode
    {
        FirstPerson,
        ThirdPerson
    }

    private const string FirstPersonFogShader = "Hidden/J3Tech/Volumetric Fog/First Person Volumetric Fog";
    private const string FirstPersonFogCullOffShader = "Hidden/J3Tech/Volumetric Fog/First Person Volumetric Fog Cull Off";
    private const string ThirdPersonFogShader = "Hidden/J3Tech/Volumetric Fog/Third Person Volumetric Fog";

    public ShapeType Type;
    public float Visibility = 10.0f;
    public Color FogColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public FogMode Mode = FogMode.Exp;
    public Vector3 Size = Vector3.one * 50f;
    public float StartFade = 0;
    public float EndFade = 1;
    public FadeMode Fade = FadeMode.Smooth;
    public ViewMode View = ViewMode.FirstPerson;

    public Transform ThirdPerson;
    public bool CullOff = true;
    private Vector3 _vectorOne = Vector3.one;
    private Transform _transform;
    private Material _material;

    public Material Mat
    {
        get
        {
            if (!_material)
            {
                _material = new Material(Shader.Find((View == ViewMode.FirstPerson) ? 
                    (CullOff ? FirstPersonFogCullOffShader :FirstPersonFogShader) : 
                    ThirdPersonFogShader));
            }
            return _material;
        }
        set { _material = value; }
    }

    [Conditional("UNITY_EDITOR")]
    private void ResetScale()
    {
        if (!_transform)
        {
            _transform = transform;
        }

        if (_transform.lossyScale != _vectorOne)
        {
            _transform.localScale = new Vector3(
                    _transform.localScale.x / _transform.lossyScale.x,
                    _transform.localScale.y / _transform.lossyScale.y, 
                    _transform.localScale.y / _transform.lossyScale.y);
        }
    }

    void OnEnable()
    {
        if (Camera.current)
        {
            Camera.current.depthTextureMode |= DepthTextureMode.Depth;
        }
        if (Camera.main)
        {
            Camera.main.depthTextureMode |= DepthTextureMode.Depth;
        }
        Renderer render = GetComponent<Renderer>();
        render.castShadows = false;
        render.receiveShadows = false;
        _transform = transform;
        UpdateVolumetricFog();
    }

    void Update()
    {
        ResetScale();
        if (View == ViewMode.ThirdPerson)
        {
            if (ThirdPerson)
            {
                _material.SetVector("_Center", ThirdPerson.position);
            }
        }
    }

    public Mesh CreateMesh()
    {
        switch (Type)
        {
            case ShapeType.Cube:
                return Cube.GetMesh(Size);
            case ShapeType.Cylinder:
                return Cylinder.GetMesh(Size);
            default:
                return Cube.GetMesh(Size);
        }
    }

    public void UpdateMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter)
        {
            Vector3 halfSize = Size * 0.5f;
            meshFilter.sharedMesh = CreateMesh();
            Bounds bounds = new Bounds();
            bounds.SetMinMax(-halfSize, halfSize);
            meshFilter.sharedMesh.bounds = bounds;
        }        
    }

    public void UpdateMaterial()
    {
        ResetScale();
        Renderer render = GetComponent<Renderer>();
        if (render)
        {
            Mat.shader = Shader.Find((View == ViewMode.FirstPerson) ?
                    (CullOff ? FirstPersonFogCullOffShader : FirstPersonFogShader) :
                    ThirdPersonFogShader);
            Mat.SetFloat("_Visibility", Visibility);
            Mat.SetColor("_Color", FogColor);
            float half = Size.y * 0.5f;
            Mat.SetFloat("_Start", Mathf.Clamp(StartFade * Size.y - half, -half, half));
            Mat.SetFloat("_End", Mathf.Clamp(EndFade * Size.y - half, -half, half));

            Mat.SetVector("_Size", Size * 0.5f);

            if (Mode == FogMode.Exp)
            {
                Mat.EnableKeyword("EXP");
                Mat.DisableKeyword("LINEAR");
            }
            else
            {
                Mat.EnableKeyword("LINEAR");
                Mat.DisableKeyword("EXP");
            }

            if (Type == ShapeType.Cube)
            {
                Mat.EnableKeyword("CUBE");
                Mat.DisableKeyword("CYLINDER");
            }
            else
            {
                Mat.EnableKeyword("CYLINDER");
                Mat.DisableKeyword("CUBE");
            }

            if (Fade == FadeMode.Smooth)
            {
                Mat.EnableKeyword("SMOOTH_FADE");
                Mat.DisableKeyword("LINEAR_FADE");
            }
            else
            {
                Mat.EnableKeyword("LINEAR_FADE");
                Mat.DisableKeyword("SMOOTH_FADE");
            }
            render.material = Mat;
        }
    }

    public  void UpdateVolumetricFog()
    {
        UpdateMesh();
        UpdateMaterial();
    }

    public void AddCamera(Camera cam)
    {
        cam.depthTextureMode |= DepthTextureMode.Depth;
    }

#if !VF_DontDynamicSetCamera
    private void OnWillRenderObject()
    {
        if (Camera.current.depthTextureMode== DepthTextureMode.None)
        {
            Camera.current.depthTextureMode |= DepthTextureMode.Depth;
        }
    }
#endif
}

public static class Cube 
{
    private static readonly Vector3[] _verts = new Vector3[8]
    {
        new Vector3(-0.5f,-0.5f,-0.5f), 
        new Vector3(0.5f,-0.5f,-0.5f), 
        new Vector3(0.5f,0.5f,-0.5f), 
        new Vector3(0.5f,-0.5f,0.5f), 
        new Vector3(0.5f,0.5f,0.5f), 
        new Vector3(-0.5f,0.5f,-0.5f), 
        new Vector3(-0.5f,0.5f,0.5f), 
        new Vector3(-0.5f,-0.5f,0.5f)
    };

    private static readonly int[] _indices = new int[36]
    {
        0, 2, 1,
        0, 5, 2,
        3, 6, 7,
        3, 4, 6,
        1, 4, 3,
        1, 2, 4,
        7, 5, 0,
        7, 6, 5,
        7, 1, 3,
        7, 0, 1,
        5, 4, 2,
        5, 6, 4
    };

    public static Mesh GetMesh(Vector3 scale)
    {
        Mesh mesh = new Mesh();
        Vector3[] scaledVertices = new Vector3[_verts.Length];
        for (int i = 0; i < _verts.Length; i++)
        {
            scaledVertices[i] = Vector3.Scale(_verts[i], scale);
        }
        mesh.vertices = scaledVertices;
        mesh.triangles = _indices;
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "Cube Fog";
        return mesh;
    }
}

public static class Cylinder
{
    private static readonly Vector3[] _verts = new Vector3[88]
    {
	    new Vector3(-0.476f, -0.500f, -0.155f),
	    new Vector3(-0.405f, -0.500f, -0.294f),
	    new Vector3(-0.294f, -0.500f, -0.405f),
	    new Vector3(-0.155f, -0.500f, -0.476f),
	    new Vector3(0.000f, -0.500f, -0.500f),
	    new Vector3(0.155f, -0.500f, -0.476f),
	    new Vector3(0.294f, -0.500f, -0.405f),
	    new Vector3(0.405f, -0.500f, -0.294f),
	    new Vector3(0.476f, -0.500f, -0.155f),
	    new Vector3(0.500f, -0.500f, 0.000f),
	    new Vector3(0.476f, -0.500f, 0.155f),
	    new Vector3(0.405f, -0.500f, 0.294f),
	    new Vector3(0.294f, -0.500f, 0.405f),
	    new Vector3(0.155f, -0.500f, 0.476f),
	    new Vector3(0.000f, -0.500f, 0.500f),
	    new Vector3(-0.155f, -0.500f, 0.476f),
	    new Vector3(-0.294f, -0.500f, 0.405f),
	    new Vector3(-0.405f, -0.500f, 0.294f),
	    new Vector3(-0.476f, -0.500f, 0.155f),
	    new Vector3(-0.500f, -0.500f, 0.000f),
	    new Vector3(-0.476f, 0.500f, -0.155f),
	    new Vector3(-0.405f, 0.500f, -0.294f),
	    new Vector3(-0.294f, 0.500f, -0.405f),
	    new Vector3(-0.155f, 0.500f, -0.476f),
	    new Vector3(0.000f, 0.500f, -0.500f),
	    new Vector3(0.155f, 0.500f, -0.476f),
	    new Vector3(0.294f, 0.500f, -0.405f),
	    new Vector3(0.405f, 0.500f, -0.294f),
	    new Vector3(0.476f, 0.500f, -0.155f),
	    new Vector3(0.500f, 0.500f, 0.000f),
	    new Vector3(0.476f, 0.500f, 0.155f),
	    new Vector3(0.405f, 0.500f, 0.294f),
	    new Vector3(0.294f, 0.500f, 0.405f),
	    new Vector3(0.155f, 0.500f, 0.476f),
	    new Vector3(0.000f, 0.500f, 0.500f),
	    new Vector3(-0.155f, 0.500f, 0.476f),
	    new Vector3(-0.294f, 0.500f, 0.405f),
	    new Vector3(-0.405f, 0.500f, 0.294f),
	    new Vector3(-0.476f, 0.500f, 0.155f),
	    new Vector3(-0.500f, 0.500f, 0.000f),
	    new Vector3(0.000f, -0.500f, 0.000f),
	    new Vector3(0.000f, 0.500f, 0.000f),
	    new Vector3(0.500f, -0.500f, 0.000f),
	    new Vector3(0.500f, 0.500f, 0.000f),
	    new Vector3(-0.500f, -0.500f, 0.000f),
	    new Vector3(-0.476f, 0.500f, -0.155f),
	    new Vector3(-0.476f, -0.500f, -0.155f),
	    new Vector3(-0.500f, 0.500f, 0.000f),
	    new Vector3(-0.405f, -0.500f, -0.294f),
	    new Vector3(-0.476f, -0.500f, -0.155f),
	    new Vector3(-0.294f, -0.500f, -0.405f),
	    new Vector3(-0.155f, -0.500f, -0.476f),
	    new Vector3(0.000f, -0.500f, -0.500f),
	    new Vector3(0.155f, -0.500f, -0.476f),
	    new Vector3(0.294f, -0.500f, -0.405f),
	    new Vector3(0.405f, -0.500f, -0.294f),
	    new Vector3(0.476f, -0.500f, -0.155f),
	    new Vector3(0.500f, -0.500f, 0.000f),
	    new Vector3(0.476f, -0.500f, 0.155f),
	    new Vector3(0.405f, -0.500f, 0.294f),
	    new Vector3(0.294f, -0.500f, 0.405f),
	    new Vector3(0.155f, -0.500f, 0.476f),
	    new Vector3(0.000f, -0.500f, 0.500f),
	    new Vector3(-0.155f, -0.500f, 0.476f),
	    new Vector3(-0.294f, -0.500f, 0.405f),
	    new Vector3(-0.405f, -0.500f, 0.294f),
	    new Vector3(-0.476f, -0.500f, 0.155f),
	    new Vector3(-0.500f, -0.500f, 0.000f),
	    new Vector3(-0.476f, 0.500f, -0.155f),
	    new Vector3(-0.405f, 0.500f, -0.294f),
	    new Vector3(-0.294f, 0.500f, -0.405f),
	    new Vector3(-0.155f, 0.500f, -0.476f),
	    new Vector3(0.000f, 0.500f, -0.500f),
	    new Vector3(0.155f, 0.500f, -0.476f),
	    new Vector3(0.294f, 0.500f, -0.405f),
	    new Vector3(0.405f, 0.500f, -0.294f),
	    new Vector3(0.476f, 0.500f, -0.155f),
	    new Vector3(0.500f, 0.500f, 0.000f),
	    new Vector3(0.476f, 0.500f, 0.155f),
	    new Vector3(0.405f, 0.500f, 0.294f),
	    new Vector3(0.294f, 0.500f, 0.405f),
	    new Vector3(0.155f, 0.500f, 0.476f),
	    new Vector3(0.000f, 0.500f, 0.500f),
	    new Vector3(-0.155f, 0.500f, 0.476f),
	    new Vector3(-0.294f, 0.500f, 0.405f),
	    new Vector3(-0.405f, 0.500f, 0.294f),
	    new Vector3(-0.476f, 0.500f, 0.155f),
	    new Vector3(-0.500f, 0.500f, 0.000f)
    };
    private static readonly int[] _indices = new int[240]
    {
	    0, 21, 1, 
	    0, 20, 21, 
	    1, 21, 22, 
	    1, 22, 2, 
	    2, 22, 23, 
	    2, 23, 3, 
	    3, 23, 24, 
	    3, 24, 4, 
	    4, 24, 25, 
	    4, 25, 5, 
	    5, 25, 26, 
	    5, 26, 6, 
	    6, 26, 27, 
	    6, 27, 7, 
	    7, 27, 28, 
	    7, 28, 8, 
	    8, 28, 29, 
	    8, 29, 9, 
	    42, 30, 10, 
	    42, 43, 30, 
	    10, 30, 31, 
	    10, 31, 11, 
	    11, 31, 32, 
	    11, 32, 12, 
	    12, 32, 33, 
	    12, 33, 13, 
	    13, 33, 34, 
	    13, 34, 14, 
	    14, 34, 35, 
	    14, 35, 15, 
	    15, 35, 36, 
	    15, 36, 16, 
	    16, 36, 37, 
	    16, 37, 17, 
	    17, 37, 38, 
	    17, 38, 18, 
	    18, 38, 39, 
	    18, 39, 19, 
	    44, 45, 46, 
	    44, 47, 45, 
	    48, 40, 49, 
	    50, 40, 48, 
	    51, 40, 50, 
	    52, 40, 51, 
	    53, 40, 52, 
	    54, 40, 53, 
	    55, 40, 54, 
	    56, 40, 55, 
	    57, 40, 56, 
	    58, 40, 57, 
	    59, 40, 58, 
	    60, 40, 59, 
	    61, 40, 60, 
	    62, 40, 61, 
	    63, 40, 62, 
	    64, 40, 63, 
	    65, 40, 64, 
	    66, 40, 65, 
	    67, 40, 66, 
	    49, 40, 67, 
	    68, 41, 69, 
	    69, 41, 70, 
	    70, 41, 71, 
	    71, 41, 72, 
	    72, 41, 73, 
	    73, 41, 74, 
	    74, 41, 75, 
	    75, 41, 76, 
	    76, 41, 77, 
	    77, 41, 78, 
	    78, 41, 79, 
	    79, 41, 80, 
	    80, 41, 81, 
	    81, 41, 82, 
	    82, 41, 83, 
	    83, 41, 84, 
	    84, 41, 85, 
	    85, 41, 86, 
	    86, 41, 87, 
	    87, 41, 68
    };

    public static Mesh GetMesh(Vector3 scale)
   {
       Mesh mesh = new Mesh();
       Vector3[] scaledVertices = new Vector3[_verts.Length];
       for (int i = 0; i < _verts.Length; i++)
       {
           scaledVertices[i] = Vector3.Scale(_verts[i], scale);
       }
       mesh.vertices = scaledVertices;
       mesh.triangles = _indices;
       mesh.hideFlags = HideFlags.DontSave;
       mesh.name = "Cylinder Fog";
       return mesh;
    }
}
