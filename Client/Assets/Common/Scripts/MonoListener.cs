using UnityEngine;
using System.Collections;
using System;

public class MonoListener : MonoBehaviour
{
    static public MonoListener Get(GameObject go)
    {
        MonoListener listener = go.GetComponent<MonoListener>();
        if (listener == null) listener = go.AddComponent<MonoListener>();
        return listener;
    }

    static public MonoListener Get(Component com)
    {
        return Get(com.gameObject);
    }
    

    public Action onAwake;
    public Action onStart;
    public Action onUpdate;
    public Action onLateUpdate;
    public Action onEnable;
    public Action onDisable;
    public Action onGUI;

    void Awake(){if (onAwake != null) onAwake();}
    void Start(){ if (onStart != null) onStart();}
    void Update(){ if (onUpdate != null) onUpdate();}
    void LateUpdate(){ if (onLateUpdate != null) onLateUpdate();}
    void OnEnable(){ if (onEnable != null) onEnable();}
    void OnDisable(){ if (onDisable != null) onDisable();}
    void OnGUI() { if (onGUI != null) onGUI();}
        
        
}
