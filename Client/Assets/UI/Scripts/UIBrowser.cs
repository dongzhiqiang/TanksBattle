using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// UI浏览器
/// </summary>
public class UIBrowser : MonoBehaviour 
{
    //存储所有的Prefab
    public List<GameObject> allPrefabs = new List<GameObject>();

    //CanvasRoot
    public GameObject uiRoot;
    //CanvasRootHight
    public GameObject uiRootHight;

    void Start()
    {

    }
}
