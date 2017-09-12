#region Header
/**
 * 名称：value
 
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
    public class ValueType
    {
        public Type value;
        public Type shareValue;
        public enValueType type;
        public Action<ValueBase> onCreate;

        public ValueType(enValueType type, Type value, Type shareValue, Action<ValueBase> onCreate = null)
        {
            this.value = value;
            this.shareValue = shareValue;
            this.type = type;
            this.onCreate = onCreate;
        }

    }


    public enum enValueType
    {
        Int,
        Float,
        Bool,
        String,
        Vector3,
        GameObject,
        Role,
        max
    }
    
    public class ValueMgrCfg
    {
        public static string[] ValueTypeName = new string[] { "整数", "浮点数",  "布尔", "字符串","Vector3","游戏对象","角色" };
        public static Dictionary<enValueType, ValueType> s_typeIdx = new Dictionary<enValueType, ValueType>();
        public static Dictionary<Type, ValueType> s_valueIdx = new Dictionary<Type, ValueType>();
        public static Dictionary<Type, ValueType> s_shareValueIdx = new Dictionary<Type, ValueType>();

        public List<ValueBase> m_value = new List<ValueBase>();
        
        Dictionary<string, ValueBase> m_nameIdx = new Dictionary<string, ValueBase>();
        Dictionary<enValueType, string[]> m_typeNames = new Dictionary<enValueType, string[]>();
        
        string m_addName = "";
        
        static ValueMgrCfg()
        {
            List<ValueType> l = new List<ValueType>();
            l.Add(new ValueType(enValueType.Float, typeof(Value<float>),typeof(ShareValue<float>)));
            l.Add(new ValueType(enValueType.Int, typeof(Value<int>), typeof(ShareValue<int>)));
            l.Add(new ValueType(enValueType.Bool, typeof(Value<bool>), typeof(ShareValue<bool>)));
            l.Add(new ValueType(enValueType.String, typeof(Value<string>), typeof(ShareValue<string>)));
            l.Add(new ValueType(enValueType.Vector3, typeof(Value<Vector3>), typeof(ShareValue<Vector3>)));
            l.Add(new ValueType(enValueType.GameObject, typeof(ValueGameObject), typeof(ShareValueGameObject)));
            l.Add(new ValueType(enValueType.Role, typeof(ValueRole), typeof(ShareValueRole)));

            foreach (var n in l)
            {
                s_typeIdx[n.type] = n;
                s_valueIdx[n.value] = n;
                s_shareValueIdx[n.shareValue] = n;
            }
        }


        public static ValueBase CreateValue(enValueType type)
        {
            var valueType = s_typeIdx.Get(type);
            if (valueType ==null)
            {
                Debuger.LogError("未知的类型，不能创建:{0}", type);
                return null;
            }

            ValueBase v = (ValueBase)Activator.CreateInstance(valueType.value);
            v.type = valueType.type;
           
            return v;
        }
        
        public static ShareValueBase<T> CreateShareValue<T>(ValueBase<T> val)
        {
            var valueType = s_typeIdx.Get(val.type);
            if (valueType == null)
            {
                Debuger.LogError("未知的类型，不能创建:{0}", val.type);
                return null;
            }
            
            ShareValueBase<T> v = Activator.CreateInstance(valueType.shareValue) as ShareValueBase<T>;
            if (v == null)
            {
                Debuger.LogError("行为树创建共享变量时类型异常，变量类型:{0},共享变量类型:{1}", val.GetType(), typeof(ShareValueBase<T>));
                return null;
            }
            v.name = val.name;
            v.InitVal = val; 
            v.Reset();
           
            return v;
        }


        public void Reset()
        {
            m_nameIdx.Clear();
            Dictionary<enValueType, List<string>> tem= new Dictionary<enValueType, List<string>>();
            
            for (int i = 0, j = m_value.Count; i < j; ++i)
            {
                var v = m_value[i];
                m_nameIdx[v.name] = v;
                tem.GetNewIfNo(v.type).Add(v.name);
            }

            foreach (var pair in tem)
            {
                m_typeNames[pair.Key] = pair.Value.ToArray();
            }
        }

        public string[] GetNames(enValueType t)
        {
            string[] a;
            if (m_typeNames.TryGetValue(t, out a))
                return a;
            a = new string[] { };
            m_typeNames[t] = a;
            return a;
        }

        public ValueBase Add(enValueType type, string name)
        {
            //检错下
            if (string.IsNullOrEmpty(name))
            {
                Debuger.LogError("创建共享变量失败，名字不能为空");
                return null;
            }
            if (m_nameIdx.ContainsKey(name))
            {
                Debuger.LogError("创建共享变量失败，名字重复了:{0}", name);
                return null;
            }

            ValueBase v = CreateValue(type);
            if (v == null)
                return null;
            
            v.name = name;
            m_value.Add(v);
            Reset();
            return v;
        }

        public void Remove(string name)
        {
            var v = m_nameIdx.Get(name);
            if (v == null)
                return;
            m_value.Remove(v);
            Reset();
        }

        public ValueBase<T> Get<T>(string name)
        {
            ValueBase v = m_nameIdx.Get(name);
            if (v == null)
                return null;
            ValueBase<T> vT = v as ValueBase<T>;
            if (vT == null)
            {
                //Debuger.LogError("行为树变量类型异常，实际类型:{0},目标类型:{1}", v.GetType(), typeof(ValueBase<T>));\
                return null;
            }
            return vT;
        }
#if UNITY_EDITOR
        public bool Draw(ValueMgr mgr)
        {
            using (new AutoBeginVertical(EditorUtil.AreaStyle(new Color(0.20f, 0.20f, 0.20f, 1))))
            {
                using (new AutoBeginHorizontal())
                {
                    //if (GUILayout.Button(rootContent, EditorStyles.toolbarButton, GUILayout.Width(20)))
                    //  return true;
                    GUILayout.Button(GUIContent.none, EditorStyles.toolbarButton, GUILayout.Width(20));
                    GUILayout.Button("类型", EditorStyles.toolbarButton, GUILayout.Width(60));
                    GUILayout.Button("名字", EditorStyles.toolbarButton, GUILayout.Width(80));
                    GUILayout.Button("初始值", EditorStyles.toolbarButton);
                    if(mgr!= null)
                    {
                        GUILayout.Button("运行值", EditorStyles.toolbarButton, GUILayout.Width(90));
                    }
                    //GUILayout.Button("运行值", EditorStyles.toolbarButton, GUILayout.Width(80));
                }
                for (int i = 0, j = m_value.Count; i < j; ++i)
                {
                    var v = m_value[i];
                    if (v.DrawShare(mgr))
                    {
                        Remove(v.name);
                        break;
                    }

                }
                using (new AutoBeginHorizontal())
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GenericMenu genericMenu = new GenericMenu();

                        for (int i = 0; i < ValueTypeName.Length; ++i)
                        {
                            int curIdx = i;
                            string name = ValueTypeName[i];
                            genericMenu.AddItem(new GUIContent(name), false, () =>
                            {
                                Add((enValueType)curIdx, m_addName);
                            });
                        }
                        genericMenu.ShowAsContext();
                    }
                    using (new AutoStyleMargin(EditorStyles.toolbarTextField,new RectOffset(0,0,0,0), new RectOffset(0, 0, 0, 0)))
                    {
                        m_addName = EditorGUILayout.TextField(GUIContent.none, m_addName, EditorStyles.textField);
                    }
                        


                }
            }

            return false;            
        }
#endif
    }





}