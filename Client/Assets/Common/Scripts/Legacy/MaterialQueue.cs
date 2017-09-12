using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MaterialQueue : MonoBehaviour
#if UNITY_EDITOR && !USE_RESOURCESEXPORT
    , ISerializationCallbackReceiver
#endif
{
    [HideInInspector]
    [SerializeField]
    public int Queue = 0;

    [HideInInspector]
    [SerializeField]
    public int mLayer = 0;

    [SerializeField]
    List<string> mMatGUIDs = new List<string>(); // 材质的GUID

#if UNITY_EDITOR
    [System.NonSerialized]
    public Material[] mDynamicMat;
    [System.NonSerialized]
    public Renderer mRenderer;
    [System.NonSerialized]
    public Material[] mMaterial;

    [UnityEditor.MenuItem("Art/Legacy/MaterialQueue")]
    static void AddMaterialQueue()
    {
        List<GameObject> activeGameOjbect = new List<GameObject>(UnityEditor.Selection.gameObjects);
        foreach (GameObject go in activeGameOjbect)
        {
            if(go.GetComponent<MaterialQueue>() == null)
                go.AddComponent<MaterialQueue>();
        }
    }

#endif

    
    public int CurrentQueue
    {
        get
        {
            return Queue + mLayer;
        }

        set
        {
            if (CurrentQueue != value)
            {
                Queue = 0;
                mLayer = value;
                Set();
            }
        }
    }

    // Use this for initialization
	void Start ()
    {
#if UNITY_EDITOR
        mRenderer = GetComponent<Renderer>();
        if (mRenderer != null)
        {
            if (!Application.isPlaying)
            {
                SetMaterials(mRenderer.sharedMaterials);
            }
            else
            {
                mMaterial = mRenderer.sharedMaterials;
            }
        }
#else
        mMatGUIDs = null;
#endif

        if (Queue == 0)
        {
            Queue = 1000;
        }

        Set();
	}

#if UNITY_EDITOR
    void OnDisable()
    {
        if (Application.isPlaying)
            return;

        if (mRenderer != null)
        {
            mRenderer.sharedMaterials = MaterialReplace.Replace(mRenderer.sharedMaterials);
        }

        mDynamicMat = null;
        mMaterial = null;
    }

    public void OnBeforeSerialize()
    {
        if (UnityEditor.PrefabUtility.GetPrefabType(gameObject) == UnityEditor.PrefabType.Prefab)
        {
            Recover();
        }
    }

    public void OnAfterDeserialize()
    {

    }

    void SetMaterials(Material[] mats)
    {
        mMaterial = MaterialReplace.Replace(mats);
        for (int i = 0; i < mMaterial.Length; ++i)
        {
            if (mMaterial[i] == null && mMatGUIDs.Count > i)
            {
                string guid = mMatGUIDs[i];
                mMaterial[i] = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid), typeof(Material));
            }
        }

        mMatGUIDs.Clear();
        foreach (Material mat in mMaterial)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(mat);
            mMatGUIDs.Add(UnityEditor.AssetDatabase.AssetPathToGUID(path));
        }
    }

    public void Recover()
    {
        mRenderer = GetComponent<Renderer>();
        if (mRenderer != null)
        {
            SetMaterials(mRenderer.sharedMaterials);
            mRenderer.sharedMaterials = mMaterial;
        }
    }

    void OnEnable()
    {
        Start();
    }
#endif

    public void Set()
    {
#if UNITY_EDITOR
        if (mRenderer == null)
            return;

        if (CurrentQueue == 0)
        {
            mRenderer.sharedMaterials = mMaterial;
            mDynamicMat = null;
        }
        else
        {
            if (mMaterial == null)
            {
                mDynamicMat = null;
                return;
            }

            mDynamicMat = AutoMaterial.GetMaterialsQueue(mMaterial, CurrentQueue);
            mRenderer.sharedMaterials = mDynamicMat;
        }
#else
        if (CurrentQueue != 0)
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterials = AutoMaterial.GetMaterialsQueue(r.sharedMaterials, CurrentQueue);
            }
        }
        
        Object.Destroy(this);
#endif
    }
}
