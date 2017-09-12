#region Header
/**
 * 名称：GameObjectValue
 
 * 日期：2016.5.13
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Simple.BehaviorTree
{
   

    //原始的值类型，int、float这些
    public class ValueGameObject : ValueBase<GameObject> 
    {
        public string path="";
        bool cache = false;
        GameObject v;

        public override GameObject Val
        {
            get {
                if (!cache)
                {

                    cache = true;
                    v = GameObject.Find(path);
                    if (!string.IsNullOrEmpty(path) && v == null)
                    {
                        Debuger.Log("行为树找不到游戏对象值里设定的游戏对象，请确定场景里有这个游戏对象:{0}", path);
                    }
                    //if(v != null)
                    //    Debuger.Log("行为树成功找到游戏对象:{0}", path);
                }
                return v;

            }
        }

        //之后从json序列化创建
        public ValueGameObject()
        {
            this.type = enValueType.GameObject;
        }

       

#if UNITY_EDITOR
        public override void OnDrawShare(ValueMgr mgr)
        {
           
            //值
            GameObject go = (GameObject)EditorGUILayout.ObjectField(Val, typeof(GameObject), true);
            if (go != null && go != Val)
            {
                path = Util.GetGameObjectPath(go);
                v = go;
            }
            if (!string.IsNullOrEmpty(path) && GUILayout.Button("清空", EditorStyles.toolbarButton, GUILayout.Width(35)))
            {
                path = "";
            }

            if (mgr != null)
            {
                do
                {
                    ShareValueBase<GameObject> share = mgr.GetValue<GameObject>(this.name);
                    if (share != null)
                    {
                        share.ShareVal = (GameObject)EditorGUILayout.ObjectField(share.ShareVal, typeof(GameObject), true);
                        break;
                    }
                    EditorGUILayout.LabelField("找不到", GUILayout.Width(90));
                } while (false);
            }

            

        }

        public override void OnDraw(string fieldName, NodeCfg nodeCfg, Node n)
        {

            //变量的话要设置变量名，常量的话要设置值
            if (region == enValueRegion.constant)
            {
                GameObject go = (GameObject)EditorGUILayout.ObjectField(Val, typeof(GameObject), true);
                if (go != null && go != Val)
                {
                    path = Util.GetGameObjectPath(go);
                    v = go;
                }
                if (!string.IsNullOrEmpty(path) && GUILayout.Button("清空", EditorStyles.toolbarButton, GUILayout.Width(35)))
                {
                    path = "";
                }
            }
            else
                DrawShareValueName(nodeCfg);

            if (n!=null&& region != enValueRegion.constant)
            {
                do
                {
                    if (!string.IsNullOrEmpty(this.name))
                    {

                        ShareValueBase<GameObject> share = n.GetShareValue<GameObject>(this);
                        if (share != null)
                        {
                            share.ShareVal = (GameObject)EditorGUILayout.ObjectField(share.ShareVal, typeof(GameObject), true, GUILayout.Width(90));
                            break;
                        }
                    }
                    EditorGUILayout.LabelField("找不到", GUILayout.Width(90));
                } while (false);
            }
            

            
        }

#endif
    }

    
    //原始的值类型，int、float这些
    public class ShareValueGameObject : ShareValueBase<GameObject>
    {
        public GameObject share ;

        public override GameObject ShareVal
        {
            get { return share; }
            set { share = value; }
        }
    }
}