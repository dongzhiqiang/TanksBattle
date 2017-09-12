using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MaterialQueue))]
public class MaterialQueueEditor : Editor
{
    public enum LayerType
    {
        Terrain = 1000, // 地表层
        Decal = 1100, // 贴花层
        Build = 2000, // 建筑层
        Light = 2100, // 灯光层
    }

    public static string[] Layers = new string[]
    {
        "地表层",//不导出中文
        "贴花层",//不导出中文
        "建筑层",//不导出中文
        "灯光层",//不导出中文
    };

    public static int[] LayersInt = new int[]
    {
        (int)LayerType.Terrain,
        (int)LayerType.Decal,
        (int)LayerType.Build,
        (int)LayerType.Light,
    };

    public static int CheckValue(int queue)
    {
        if (queue <= (int)LayerType.Terrain)
            return (int)LayerType.Terrain;

        if (queue <= (int)LayerType.Decal)
            return (int)LayerType.Decal;

        if (queue <= (int)LayerType.Build)
            return (int)LayerType.Build;

        if (queue <= (int)LayerType.Light)
            return (int)LayerType.Light;

        return queue;
    }

    public override void OnInspectorGUI()
    {
        MaterialQueue mq = target as MaterialQueue;
        int current = mq.CurrentQueue;
        base.OnInspectorGUI();
        mq.Queue = EditorGUILayout.IntPopup("类型", CheckValue(mq.Queue), Layers, LayersInt);
        mq.mLayer = EditorGUILayout.IntField("层级", mq.mLayer);

        if (PrefabUtility.GetPrefabType(mq.gameObject) == PrefabType.Prefab)
        {
            if (GUILayout.Button("恢复材质"))
            {
                mq.Recover();
            }
        }

        bool bChange = false;
        Material[] mats = mq.mMaterial;
        Material[] dynMats = mq.mDynamicMat;
        Material tmp = null;
        GUILayout.Label("Materials:");
        if (mats != null && mats.Length != 0)
        {
            for (int i = 0; i < mats.Length; ++i)
            {
                GUILayout.BeginHorizontal();
                tmp = (Material)EditorGUILayout.ObjectField("源材质", mats[i], typeof(Material), false);
                if (mats[i] != tmp)
                {
                    mats[i] = tmp;
                    bChange = true;
                }
                if (mats[i] != null)
                {
                    EditorGUILayout.IntField("Layer", mats[i].renderQueue);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (dynMats != null && dynMats.Length > i && dynMats[i] != null)
                {
                    EditorGUILayout.ObjectField("复制材质", dynMats[i], typeof(Material), false);
                    if (dynMats[i] != null)
                    {
                        EditorGUILayout.IntField("Layer", dynMats[i].renderQueue);
                    }
                    else
                    {
                        EditorGUILayout.IntField("Layer", 0);
                    }
                }
                else
                {
                    EditorGUILayout.ObjectField("复制材质", null, typeof(Material), false);
                    EditorGUILayout.IntField("Layer", 0);
                }

                GUILayout.EndHorizontal();
            }

            if (bChange == true)
            {
                if (mq.GetComponent<Renderer>() != null)
                {
                    mq.GetComponent<Renderer>().sharedMaterials = mats;
                }

                if (mq.enabled)
                {
                    mq.Set();
                }
            }
            else if (current != mq.CurrentQueue)
            {
                if (mq.enabled)
                    mq.Set();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(mq.gameObject);
            }
        }
    }
}
