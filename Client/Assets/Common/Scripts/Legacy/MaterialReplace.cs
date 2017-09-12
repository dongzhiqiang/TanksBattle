using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialReplace
{
#if UNITY_EDITOR
    static Dictionary<string, List<Material>> MaterialList;

    [UnityEditor.MenuItem("Art/Legacy/RestoreMaterial")]
    static void RestoreMaterial()
    {
        MaterialList = null;
        AutoMaterial.Update();
        List<GameObject> activeGameOjbect = new List<GameObject>(UnityEditor.Selection.gameObjects);
        foreach (GameObject go in activeGameOjbect)
        {
            RestoreGameObject(go);
        }
    }

    public static void RestoreGameObject(GameObject go)
    {
        if (go.hideFlags == HideFlags.DontSave || go.hideFlags == HideFlags.HideAndDontSave)
            return;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterials = Replace(renderer.sharedMaterials);
        }

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            RestoreGameObject(go.transform.GetChild(i).gameObject);
        }
    }

    public static Material[] Replace(Material[] ms)
    {
        List<Material> mats = new List<Material>(ms);
        Material m;
        for (int i = 0; i < mats.Count; ++i)
        {
            if (string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(mats[i])))
            {
                if (mats[i] == null)
                    continue;

                m = FindMaterial(mats[i]);
                if (m == null)
                {
                    try
                    {
                        string matpath = UnityEditor.AssetDatabase.GetAssetOrScenePath(mats[i]);
                        if (string.IsNullOrEmpty(matpath))
                            continue;

                        System.IO.Directory.CreateDirectory(Application.dataPath + "/TmpMaterial/");
                        if (mats[i].mainTexture != null)
                        {
                            UnityEditor.AssetDatabase.CreateAsset(mats[i], "Assets/TmpMaterial/" + mats[i].mainTexture.name + ".mat");
                        }
                        else
                        {
                            UnityEditor.AssetDatabase.CreateAsset(mats[i], "Assets/TmpMaterial/" + mats[i].name + ".mat");
                        }
                        UnityEditor.AssetDatabase.SaveAssets();
                        UnityEditor.AssetDatabase.Refresh();

                        Debuger.Log(string.Format("mat:{0} 失败!", mats[i].name)); // 材质替换下
                    }
                    catch(System.Exception ex)
                    {

                    }
                }
                else
                {
                    mats[i] = m;
                    if (!Application.isPlaying)
                    {
                        //Debuger.Log(string.Format("mat:{0}成功!", m.name)); // 材质替换成功
                    }
                }
            }
        }

        return mats.ToArray();
    }

    // 查找被此材质复制的材质
    static Material FindMaterial(Material copyMat)
    {
        if (MaterialList == null)
        {
            MaterialList = new Dictionary<string, List<Material>>();
            string[] files = System.IO.Directory.GetFiles(Application.dataPath, "*.mat", System.IO.SearchOption.AllDirectories);
            int startIndex = Application.dataPath.Length - "Assets".Length;
            foreach (string file in files)
            {
                string s = file.Substring(startIndex);
                Material m = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(s, typeof(Material));
                if (m == null)
                {
                    Debuger.Log(string.Format("load mat:{0} null", s));
                }
                else
                {
                    List<Material> mats = null;
                    if (!MaterialList.TryGetValue(m.name, out mats))
                    {
                        mats = new List<Material>();
                        MaterialList.Add(m.name, mats);
                    }

                    mats.Add(m);
                }
            }
        }

        string name = copyMat.name;
        string srcName = name;
        // 名字有2种可能性
        // a材质名(有可能同名)
        // b资源的guid
        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(name);
        Material src_mat = null;
        if (string.IsNullOrEmpty(path) || (src_mat = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Material))))
        {
            List<Material> mats = null;
            int index = 0;
            do
            {
                if (MaterialList.TryGetValue(name, out mats))
                {
                    if (mats.Count == 1)
                        return mats[0];

                    foreach (Material mat in mats)
                    {
                        if (mat.mainTexture == copyMat.mainTexture && mat.shader == mat.shader)
                        {
                            bool has_mat = mat.HasProperty("_Color");
                            bool has_copy_Mat = copyMat.HasProperty("_Color");
                            if (has_mat == true && has_mat == has_copy_Mat)
                            {
                                if (mat.color == copyMat.color)
                                    return mat;
                            }
                            else if (has_mat == false && has_mat == has_copy_Mat)
                            {
                                return mat;
                            }
                        }
                    }
                }
                if (srcName.Contains(" "))
                {
                    if (index == 0)
                    {
                        name = srcName.Replace(' ', '_');
                        index++;
                    }
                    else if (index == 1)
                    {
                        index++;
                        name = srcName.Replace(" ", "");
                    }
                    else
                    {
                        break;
                    }
                }
                else
                    break;
            } while (true);
        }

        return src_mat;
    }
#endif
}
