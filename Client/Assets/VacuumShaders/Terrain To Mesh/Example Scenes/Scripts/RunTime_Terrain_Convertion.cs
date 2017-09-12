/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！

该资源为CGSOSO团购资源，请勿在其他渠道购买，以免无法获取后续更新！

请勿倒卖分享，保障参团者利益！分享者，收不到后续更新，请谅解！
*/

using UnityEngine;
using System.Collections;

using VacuumShaders.TerrainToMesh;


[AddComponentMenu("VacuumShaders/Terrain To Mesh/Example/Runtime Converter")]
public class RunTime_Terrain_Convertion : MonoBehaviour 
{
    //////////////////////////////////////////////////////////////////////////////
    //                                                                          // 
    //Variables                                                                 //                
    //                                                                          //               
    //////////////////////////////////////////////////////////////////////////////
    public Terrain sourceTerrain;

    public TerrainConvertInfo convertInfo;

    public bool generateBasemap;
    //////////////////////////////////////////////////////////////////////////////
    //                                                                          // 
    //Functions                                                                 //                
    //                                                                          //               
    //////////////////////////////////////////////////////////////////////////////
	void Start () 
    {
        if (sourceTerrain != null)
        {
            //Convert Terrain
            Mesh[] generatedMeshes = TerrainToMeshConverter.Convert(sourceTerrain, convertInfo);


            //Setup game object
            if (generatedMeshes != null)
            {

                //Create shared material
                Material sharedMat = null;
                if (generateBasemap)
                    sharedMat = GenerateMaterial_Basemap();
                else
                    sharedMat = GenerateMaterial_Splatmap();


                if (generatedMeshes.Length == 1)
                {
                    //Setup MeshFilter
                    MeshFilter mf = gameObject.GetComponent<MeshFilter>();
                    if (mf == null)
                        mf = gameObject.AddComponent<MeshFilter>();
                    mf.sharedMesh = generatedMeshes[0];


                    //Setup MeshRenderer
                    MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                    if (mr == null)
                        mr = gameObject.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = sharedMat;
                }
                else
                {
                    //Create full hierarchy
                    for (int i = 0; i < generatedMeshes.Length; i++)
                    {
                        //Create child object
                        GameObject terraMeshObj = new GameObject(generatedMeshes[i].name);
                        terraMeshObj.transform.parent = gameObject.transform;
                        terraMeshObj.transform.localPosition = Vector3.zero;


                        //Add mesh filter
                        MeshFilter mf = terraMeshObj.AddComponent<MeshFilter>();
                        mf.sharedMesh = generatedMeshes[i];


                        //Add mesh renderer 
                        MeshRenderer mr = terraMeshObj.AddComponent<MeshRenderer>();
                        mr.sharedMaterial = sharedMat;
                    }
                }
            }
        }   
	}

    Material GenerateMaterial_Basemap()
    {
        //Export basemaps (diffuse and normal)
        Texture2D basemapDiffuse = null;
        Texture2D basemapNormal = null;

        TerrainToMeshConverter.ExtractBasemap(sourceTerrain, out basemapDiffuse, out basemapNormal, 1024, 1024);


        Material newMaterial = new Material(Shader.Find(basemapNormal != null ? "Legacy Shaders/Bumped Diffuse" : "Legacy Shaders/Diffuse"));

        newMaterial.mainTexture = basemapDiffuse;
        if (basemapNormal != null)
            newMaterial.SetTexture("_BumpMap", basemapNormal);

        return newMaterial;
    }

    Material GenerateMaterial_Splatmap()
    {
        Material newMaterial = null;


        //Export terrain splatmaps
        Texture2D[] splatMap = TerrainToMeshConverter.ExtractSplatmaps(sourceTerrain);
        if (splatMap == null || splatMap.Length == 0)
            return newMaterial;


        //Export diffuse/normal textures
        Texture2D[] diffuseTextures;
        Texture2D[] normalTextures;
        Vector2[] uvScale;
        Vector2[] uvOffset;


        int usedTexturesCount = TerrainToMeshConverter.ExtractTexturesInfo(sourceTerrain, out diffuseTextures, out normalTextures, out uvScale, out uvOffset);
        if (usedTexturesCount == 0 || diffuseTextures == null)
        {
            //Problems with terrain
            Debug.LogWarning("usedTexturesCount == 0");

            return newMaterial;
        }
        else if (usedTexturesCount == 1)
        {
            //There is no need to use TerrainToMesh shaders with one texture
            Shader shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader != null)
            {
                newMaterial = new Material(shader);

                //Texture
                newMaterial.mainTexture = diffuseTextures[0];

                //Scale & Offset
                newMaterial.mainTextureScale = uvScale[0];
                newMaterial.mainTextureOffset = uvOffset[0];
            }

            return newMaterial;
        }


        //Terrain To Mesh shaders support max 8 textures blend
        usedTexturesCount = Mathf.Clamp(usedTexturesCount, 2, 8);


        //Select proper shader - One Directional Light
        Shader ttmShader = Shader.Find(string.Format("VacuumShaders/Terrain To Mesh/One Directional Light/Diffuse/{0} Textures", usedTexturesCount));
        if (ttmShader == null)
        {
            Debug.LogWarning("Shader not found: " + string.Format("VacuumShaders/Terrain To Mesh/Standard/Diffuse/{0} Textures", usedTexturesCount));

            return newMaterial;
        }


        //Select shader
        newMaterial = new Material(ttmShader);

        //Set up controll textures
        if (splatMap.Length == 1)
        {
            newMaterial.SetTexture("_V_T2M_Control", splatMap[0]);
        }
        else
        {
            if (splatMap.Length > 2)
                Debug.Log("TerrainToMesh shaders support max 2 control textures. Current terrain uses " + splatMap.Length);

            newMaterial.SetTexture("_V_T2M_Control", splatMap[0]);
            newMaterial.SetTexture("_V_T2M_Control2", splatMap[1]);
        }


        //Assign textures
        for (int i = 0; i < usedTexturesCount; i++)
        {
            //Texture
            newMaterial.SetTexture(string.Format("_V_T2M_Splat{0}", i + 1), diffuseTextures[i]);

            //Scale
            newMaterial.SetFloat(string.Format("_V_T2M_Splat{0}_uvScale", i + 1), uvScale[i].x);
        }


        return newMaterial;
    }
}
