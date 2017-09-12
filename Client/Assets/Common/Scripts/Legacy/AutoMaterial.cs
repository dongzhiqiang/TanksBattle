using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoMaterial
{
    static Dictionary<Material, List<Material>> sMaterialList = new Dictionary<Material, List<Material>>();

    public static void Clear()
    {
        sMaterialList.Clear();
    }

    public static void Update()
    {
        foreach (KeyValuePair<Material, List<Material>> itor in sMaterialList)
        {
            foreach (Material mat in itor.Value)
            {
                if (mat != null)
                {
                    int queue = mat.renderQueue;
                    mat.shader = itor.Key.shader;
                    mat.CopyPropertiesFromMaterial(itor.Key);
                    mat.renderQueue = queue;
                }
            }
        }
    }

    // 得到与此材质相差多少渲染层级的材质
    static public Material GetMaterial(Material mat, int diff)
    {
        if (mat == null)
            return null;

        int renderQueue = mat.renderQueue + diff;
        return GetMaterialQueue(mat, renderQueue);
    }

    static public Material GetMaterialQueue(Material mat, int renderQueue)
    {
        if (mat == null)
            return null;
        if (mat.renderQueue == renderQueue)
            return mat;

        List<Material> Mats = null;
        if (sMaterialList.TryGetValue(mat, out Mats))
        {
            if (Mats != null)
            {
                foreach (Material m in Mats)
                {
                    if ((m != null) && (m.renderQueue == renderQueue))
                        return m;
                }
            }
            else
            {
                Mats = new List<Material>();
                sMaterialList[mat] = Mats;
            }
        }
        else
        {
            Mats = new List<Material>();
            sMaterialList.Add(mat, Mats);
        }

        Material copy_mat = new Material(mat);
        copy_mat.name = mat.name + renderQueue;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(mat);
            if (string.IsNullOrEmpty(path))
                copy_mat.name = mat.name;
            else
            {
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                    copy_mat.name = guid;
                else
                    copy_mat.name = mat.name;
            }
        }
#endif
        copy_mat.renderQueue = renderQueue;
        Mats.Add(copy_mat);
        return copy_mat;
    }

    static public Material GetSrcMat(Material mat)
    {
        if (mat == null)
            return null;

        foreach (KeyValuePair<Material, List<Material>> itor in sMaterialList)
        {
            if (itor.Value.Contains(mat))
                return itor.Key;
        }

        return null;
    }

    static public Material[] GetMaterials(Material[] mats, int diff)
    {
        if (mats == null)
            return null;

        if (mats.Length == 0)
        {
            return new Material[0];
        }
        
        Material[] dsts = new Material[mats.Length];
        for (int i = 0; i < mats.Length; ++i)
        {
            dsts[i] = GetMaterial(mats[i], diff);
        }

        return dsts;
    }

    static public Material[] GetMaterialsQueue(Material[] mats, int renderQueue)
    {
        if (mats == null || mats.Length == 0)
            return null;
        Material[] dsts = new Material[mats.Length];
        for (int i = 0; i < mats.Length; ++i)
        {
            dsts[i] = GetMaterialQueue(mats[i], renderQueue);
        }

        return dsts;
    }

    static public Material[] GetSrcMats(Material[] mats)
    {
        if (mats == null)
            return null;

        Material[] dsts = new Material[mats.Length];
        for (int i = 0; i < mats.Length; ++i)
        {
            dsts[i] = GetSrcMat(mats[i]);
        }

        return dsts;
    }

    static public int[] GetMatQueue(Material[] mats)
    {
        if (mats == null || mats.Length == 0)
            return null;

        int[] dsts = new int[mats.Length];
        for (int i = 0; i < mats.Length; ++i)
        {
            if (mats[i] != null)
                dsts[i] = mats[i].renderQueue;
            else
                dsts[i] = 0;
        }

        return dsts;
    }
}