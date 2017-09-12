using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DonDestroyRoot:MonoBehaviour
{
    public static Transform s_root;
    public static Dictionary<string, Transform> s_pathGo = new Dictionary<string, Transform>();
    public static void AddChild(GameObject go)
    {
        if (s_root == null)
        {
            s_root = new GameObject("DontDestroyOnLoad").transform;
            Object.DontDestroyOnLoad(s_root.gameObject);
        }

        go.transform.SetParent(s_root);
    }

    
    public static void Add(string path,GameObject go)
    {
        Transform t = s_pathGo.Get(path);
        if(t== null)
        {
            t = new GameObject(path).transform;
            s_pathGo[path] = t;
            AddChild(t.gameObject);
            t.position = Vector3.zero;
        }
        
        go.transform.SetParent(t);
    }
}

public class Singleton<T> where T : new()
{
    static protected T sInstance;
    static protected bool IsCreate = false;

    public static T instance
    {
        get
        {
            if (IsCreate == false)
            {
                CreateInstance();
            }

            return sInstance;
        }
    }

    public static void CreateInstance()
    {
        if (IsCreate == true)
            return;

        IsCreate = true;
        sInstance = new T();
    }

    public static void ReleaseInstance()
    {
        sInstance = default(T);
        IsCreate = false;
    }
}

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    protected static T s_instance = null;
    protected static bool IsCreate = false;
    public static bool s_debugDestroy = false;

    public static T instance
    {
        get
        {
            if (s_debugDestroy)
            {
                //Debuger.LogError("销毁的时候调用了单例:{0}", typeof(T));

                return null;
            }
            CreateInstance();
            
            return s_instance;
        }
    }

    public static bool IsExist
    {
        get
        {
            if (s_debugDestroy|| s_instance==null)
                return false;
            return true;
        }
    }

    protected virtual void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this as T;
            IsCreate = true;

            //Init();
        }
    }

    //protected virtual void Init()
    //{

    //}

    protected virtual void OnDestroy()
    {
        s_instance = null;
        IsCreate = false;
    }

    private void OnApplicationQuit()
    {
        IsCreate = false;
        s_instance = null;
        s_debugDestroy = true;
    }

    public static void CreateInstance()
    {
        if (IsCreate == true)
            return;

        
        if (!Application.isPlaying)
        {
            if (s_instance!= null)
                Destroy(s_instance.gameObject);
            s_instance= null;
            return;
        }
            

        IsCreate = true;
        T[] managers = GameObject.FindObjectsOfType(typeof(T)) as T[];
        if (managers.Length != 0)
        {
            if (managers.Length == 1)
            {
                s_instance = managers[0];
                s_instance.gameObject.name = typeof(T).Name;
                DonDestroyRoot.AddChild(s_instance.gameObject);
                return;
            }
            else
            {
                foreach (T manager in managers)
                {
                    Destroy(manager.gameObject);
                }
            }
        }

        GameObject gO = new GameObject(typeof(T).Name, typeof(T));
        s_instance = gO.GetComponent<T>();
        DonDestroyRoot.AddChild(gO);
    }

    public static void ReleaseInstance()
    {
        if (s_instance != null)
        {
            Destroy(s_instance.gameObject);
            s_instance = null;
            IsCreate = false;
        }
    }


}