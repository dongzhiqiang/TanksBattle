using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimplePool: MonoBehaviour {
    public GameObject mPrefab;//游戏对象或者预制体

    List<GameObject> mPool = null;
    public List<GameObject> All { get { return mPool; } }

    void OnEnable()
    {
        CheckInit();
    }

    void CheckInit()
    {
        if (mPool == null)
        {   
            if(mPrefab!=null)
                mPrefab.SetActive(false);
            mPool = new List<GameObject>();
            if (!this.gameObject.activeSelf)
                this.gameObject.SetActive(true);
        }
    }

    public List<GameObject> GetUnsing()
    {
        List<GameObject> l = new List<GameObject>();
        CheckInit();
        foreach (GameObject poolObject in mPool)
        {
            if (poolObject != null && poolObject.activeSelf)
                l.Add(poolObject);
        }
        return l;
    }

    public void Clear()
    {
        CheckInit();
        foreach (var go in mPool)
        {
            go.SetActive(false);
        }
    }

    public GameObject Get(bool notActive= false)
    {
        CheckInit();
        foreach (GameObject poolObject in mPool)
        {
            if (poolObject != null && !poolObject.activeSelf)
            {
                if(!notActive)
                    poolObject.SetActive(true);
                return poolObject;
            }
        }
        GameObject go = (GameObject)Object.Instantiate(mPrefab);

        if (!notActive)
            go.SetActive(true);

        Transform t = go.transform;
        Transform t2 = mPrefab.transform;
        t.SetParent(this.transform,false);
        t.localPosition = t2.localPosition;
        t.localRotation = t2.localRotation;
        t.localScale = t2.localScale;
        go.layer = mPrefab.layer;

        mPool.Add(go);
        return go;
        
    }

}
