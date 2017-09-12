using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteContentEditor : EditorWindow
{

    public static QteContentEditor instance;

    QteEventEditor m_eventEditor;

    public static void ShowWindow()
    {
        instance = (QteContentEditor)EditorWindow.GetWindow(typeof(QteContentEditor));
        instance.minSize = new Vector2(400, 300);
        instance.titleContent = new GUIContent("事件内容编辑");
        instance.autoRepaintOnSceneChange = true;
    }

    void OnGUI()
    {
        if (m_eventEditor == null)
            return;

        using (new AutoLabelWidth(50))
        {
            //事件时间
            using (new AutoBeginHorizontal())
            {
                float time = EditorGUILayout.FloatField("时长:", m_eventEditor.m_event.duration, GUILayout.ExpandWidth(false));
                if (time != m_eventEditor.m_event.duration)
                {
                    if (time < 0.1f)
                        time = 0.1f;

                    m_eventEditor.m_event.duration = time;
                    if (m_eventEditor.EndTime > m_eventEditor.StageEndTime)
                        m_eventEditor.m_event.duration = m_eventEditor.StageEndTime - m_eventEditor.StartTime;

                }
                EditorGUILayout.LabelField(m_eventEditor.m_event.name);
            }
            m_eventEditor.DrawCustom();
        }

        GUILayout.Space(200);
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.GraphBoxStyle, GUILayout.Width(20)))
        {
            foreach (QteEvent e in m_eventEditor.m_stageEditor.m_info.winInfo.events)
            {
                if (e.name == m_eventEditor.m_event.name)
                {
                    if (EditorUtility.DisplayDialog("删除事件", "确定要删除事件" + m_eventEditor.m_event.name + "吗？", "确定", "取消"))
                    {
                        m_eventEditor.m_stageEditor.m_info.winInfo.events.Remove(e);

                        if (e.m_type == enQteEventType.Qte_Operate)
                        {
                            BigQteCfg cfg = QteEditor.instance.CurQte.Cfg;
                            cfg.stages[m_eventEditor.m_stageEditor.m_info.idx + 1].loseInfo = null;
                        }

                        m_eventEditor.m_stageEditor.UpdateEventEditor();
                        QteEditor.instance.Repaint();
                        m_eventEditor = null;
                        return;
                    }
                }
            }
        }
    }

    public static void ShowEvent(QteEventEditor editor)
    {
        if (instance == null)
            ShowWindow();

        instance.m_eventEditor = editor;
        instance.Repaint();
    }
}
