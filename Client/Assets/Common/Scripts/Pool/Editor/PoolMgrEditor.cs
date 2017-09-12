using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoolMgr), true)]
public class PoolMgrEditor : Editor
{
    PoolMgr m_pool;
    string m_log;
    GUIStyle richHelpBox;
    void OnEnable()
    {
        m_pool = target as PoolMgr;
    }

    public override void OnInspectorGUI ()
	{
        m_pool.m_debugIdTypePool =EditorGUILayout.TextField("调试IdTypePool", m_pool.m_debugIdTypePool);

        if(GUILayout.Button("垃圾回收"))
        {
            System.GC.Collect();
        }

        if (GUILayout.Button("对象池回收"))
        {
            m_pool.Clear();
        }


        if(richHelpBox == null)
        {
            richHelpBox = new GUIStyle(EditorStyles.helpBox);
            richHelpBox.richText = true;
        }

        m_log = "";
        string[] ss = new string[m_pool.Pools.Count];
        int i = 0;
        foreach (var pair in m_pool.Pools)
        {
            var pool = pair.Value;
            var poolName = pair.Key;
            if (!string.IsNullOrEmpty(m_pool.m_debugIdTypePool) && !poolName.Contains(m_pool.m_debugIdTypePool))
                continue;


            var total = pool.TotalCount;
            var count = pool.Count;
            var useCount = total - count;
            string color;
            if (useCount < 10)
                color = "<color=green>";
            else if (useCount < 100)
                color = "<color=yellow>";
            else
                color = "<color=red>";

            

            ss[i++] = string.Format("{0}= {1}(池中) + {2}{3}</color>(使用中)  {4}",  total, count, color, useCount, poolName);
        }
        m_log = string.Join("\n", ss);
        EditorGUILayout.LabelField(m_log, richHelpBox);

        this.Repaint();
    }
    
}
