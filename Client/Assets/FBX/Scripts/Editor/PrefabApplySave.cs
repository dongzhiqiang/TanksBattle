#region Header
/**
 * 名称：预制体apply的时候真实存盘
 
 * 日期：2016.2.23
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class PrefabApplySave 
{
    static long s_lastApplyTime = -1;
    //[InitializeOnLoadMethod]
    //static void OnProjectLoadedInEditor()
    //{
    //    PrefabUtility.prefabInstanceUpdated = (GameObject go) =>//delegate
    //    {
    //        if (go != null && (s_lastApplyTime == -1 || System.DateTime.Now.Ticks - s_lastApplyTime > System.TimeSpan.TicksPerMillisecond * 500))
    //        {
    //            s_lastApplyTime = System.DateTime.Now.Ticks;
    //            Debuger.Log("预制体可能apply了，保存下：{0} type:{1}", go.name, PrefabUtility.GetPrefabType(go));
                
    //            AssetDatabase.SaveAssets();
                
    //        }


    //    };
    //    //Debug.Log("预制体apply脚本已加载");
    //}   
    

}
