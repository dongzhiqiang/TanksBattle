using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class QteStageEditor
{
    #region Fields
    Rect WinStage_Rect;
    Rect LoseStage_Rect;
    Rect Handle_Rect;
    Vector2 m_scroll;
    bool m_bWinDragStage;
    bool m_bLoseDragStage;
    QteEditor m_qteEditor;
    float LoseArea_H = 220.0f;

    List<QteEventEditor> m_winEventEditors = new List<QteEventEditor>();
    List<QteEventEditor> m_loseEventEditors = new List<QteEventEditor>();

    public QteStageInfo m_info;
    #endregion

    #region Properties
    QteEditor qteEditor
    {
        get
        {
            if (m_qteEditor == null)
                m_qteEditor = QteEditor.instance;
            return m_qteEditor;
        }
    }

    public float WinEndTime
    {
        get
        {
            return qteEditor.CurQte.GetStageStartTime(m_info.idx) + m_info.winInfo.duration;
        }
    }

    public float LoseEndTime
    {
        get
        {
            return qteEditor.CurQte.GetStageStartTime(m_info.idx) + m_info.loseInfo.duration;
        }
    }

    public float StartTime
    {
        get
        {
            return qteEditor.CurQte.GetStageStartTime(m_info.idx);
        }
    }
    public float WinDuration
    {
        get
        {
            return m_info.winInfo.duration;
        }
        set
        {
            m_info.winInfo.duration = value;
        }

    }
    public float LoseDuration
    {
        get
        {
            return m_info.loseInfo.duration;
        }
        set
        {
            m_info.loseInfo.duration = value;
        }

    }
    #endregion

    public void Init(QteStageInfo info)
    {
        m_info = info;
        UpdateEventEditor();
    }

    public void UpdateEventEditor()
    {
        m_winEventEditors.Clear();
        m_loseEventEditors.Clear();
        if (m_info == null)
            return;

        foreach (QteEvent e in m_info.winInfo.events)
        {
            QteEventEditor editor = QteEventEditor.CreateEventEditor(e, StartTime);
            editor.m_stageEditor = this;
            editor.StageEndTime = WinEndTime;
            m_winEventEditors.Add(editor);
        }

        if (m_info.loseInfo != null)
        {
            foreach (QteEvent e in m_info.loseInfo.events)
            {
                QteEventEditor editor = QteEventEditor.CreateEventEditor(e, StartTime);
                editor.m_stageEditor = this;
                editor.StageEndTime = LoseEndTime;
                m_loseEventEditors.Add(editor);
            }
        }

        qteEditor.Repaint();
    }
    
    public void OnGUIWin()
    {
        if (m_info == null || qteEditor == null)
            return;
        float width = qteEditor.TimeToContentX(WinEndTime) - qteEditor.TimeToContentX(m_qteEditor.CurQte.GetStageStartTime(m_info.idx));
        GUILayout.Box("", EditorStyleEx.PixelBoxStyle, GUILayout.Width(width), GUILayout.ExpandHeight(true));

        if (Event.current.type == EventType.Repaint)
        {
            if (WinStage_Rect != GUILayoutUtility.GetLastRect())
            {
                WinStage_Rect = GUILayoutUtility.GetLastRect();
            }
        }

        using (new AutoBeginArea(WinStage_Rect))
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), EditorStyleEx.ButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < QteEvent.TypeName.Length; i++)
                {
                    int t = i;
                    menu.AddItem(new GUIContent(QteEvent.TypeName[i]), false, () =>
                    {
                        if (t == (int)enQteEventType.Qte_Operate)
                        {
                            int nextIdx = m_info.idx + 1;
                            QteStageInfo stageInfo;
                            if (nextIdx < QteEditor.instance.CurQte.Cfg.stages.Count)
                                stageInfo = QteEditor.instance.CurQte.Cfg.stages[nextIdx];
                            else
                                stageInfo = QteEditor.instance.AddStage();

                            if (stageInfo.loseInfo != null)
                            {
                                Debuger.LogError("已经有输入操作事件，不能重复添加");
                                return;
                            }
                            stageInfo.loseInfo = new QteStageEventsInfo();
                            stageInfo.loseInfo.duration = 0.5f;
                        }

                        QteEvent qteEvent = QteEvent.CreateEvent((enQteEventType)t, qteEditor.CurQte);
                        qteEvent.startTime = 0.0f;//Random.Range(0.0f, m_info.winInfo.duration);
                        qteEvent.duration = 0.5f;//Random.Range(0.0f, (m_info.duration - qteEvent.startTime));
                        if ((enQteEventType)t == enQteEventType.Qte_Ani)
                            qteEvent.duration = m_info.winInfo.duration;
                        qteEvent.name = m_info.winInfo.events.Count + ":" + QteEvent.TypeName[t];
                        m_info.winInfo.events.Add(qteEvent);
                        UpdateEventEditor();
                        qteEditor.Repaint();
                    });
                }
                menu.ShowAsContext();
                qteEditor.Repaint();
            }
            using (AutoBeginScrollView sv = new AutoBeginScrollView(m_scroll, "PreferencesSectionBox", GUILayout.ExpandWidth(true), GUILayout.Height(QteEditor.WinStage_H)))
            {
                m_scroll = sv.Scroll;
                using (new AutoBeginVertical())
                {
                    for (int i = 0; i < m_winEventEditors.Count; i++)
                    {
                        m_winEventEditors[i].OnGUI();
                    }
                }
            }

            if (GUI.Button(new Rect(5, QteEditor.WinStage_H - 25, 25, 25), EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.GraphBoxStyle))
            {
                QteEditor.instance.m_removeStages.Add(m_info);
            }
        }

        float totleEndPosX = qteEditor.MaxPosX;
        float endPosX = qteEditor.TimeToContentX(WinEndTime);
        float startPosX = qteEditor.TimeToContentX(StartTime);

        //时间显示
        Rect textRect = new Rect(endPosX - 42, 0, 34, 17);
        GUI.TextField(textRect, WinDuration.ToString("#.##"));

        //拖动柄
        Handle_Rect = new Rect(endPosX - 8.0f, 0, 8.0f, 15);
        using (new AutoBeginArea(Handle_Rect, "", "box"))
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                m_bWinDragStage = true;

            if (m_bWinDragStage && Event.current.type == EventType.MouseDrag)
            {
                endPosX += Event.current.delta.x;
                if (endPosX < startPosX)
                    endPosX = startPosX + 5;
                if (endPosX >= totleEndPosX)
                    endPosX = totleEndPosX;

                float endTime = qteEditor.ContentXToTime(endPosX);
                WinDuration = endTime - StartTime;
                UpdateEventEditor();
                Event.current.Use();
                qteEditor.Repaint();
            }
        }

        if (Event.current.type == EventType.MouseUp)
            m_bWinDragStage = false;
    }

    public void OnGUILose()
    {
        if (m_info == null || qteEditor == null)
            return;
        if (m_info.loseInfo == null)
            return;

        float width = qteEditor.TimeToContentX(LoseEndTime) - qteEditor.TimeToContentX(m_qteEditor.CurQte.GetStageStartTime(m_info.idx));
        //Rect boxRect = new Rect(qteEditor.TimeToContentX(StartTime), 0, width, 100);
        //GUI.Box(boxRect, "", EditorStyleEx.PixelBoxStyle);
        using (new AutoBeginHorizontal())
        {
            if (StartTime > 0)
                GUILayout.Space(qteEditor.TimeToContentX(StartTime));
            GUILayout.Box("", EditorStyleEx.PixelBoxStyle, GUILayout.Width(width), GUILayout.Height(LoseArea_H));
            if (Event.current.type == EventType.Repaint)
            {
                if (LoseStage_Rect != GUILayoutUtility.GetLastRect())
                {
                    LoseStage_Rect = GUILayoutUtility.GetLastRect();
                }
            }
        }

        using (new AutoBeginArea(LoseStage_Rect))
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), EditorStyleEx.ButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                GenericMenu menu = new GenericMenu();
                //失败的阶段没有输入操作
                for (int i = 1; i < QteEvent.TypeName.Length; i++)
                {
                    int t = i;
                    menu.AddItem(new GUIContent(QteEvent.TypeName[i]), false, () =>
                    {
                        QteEvent qteEvent = QteEvent.CreateEvent((enQteEventType)t, qteEditor.CurQte);
                        qteEvent.startTime = 0.0f;//Random.Range(0.0f, m_info.winInfo.duration);
                        qteEvent.duration = 0.5f;//Random.Range(0.0f, (m_info.duration - qteEvent.startTime));
                        if ((enQteEventType)t == enQteEventType.Qte_Ani)
                            qteEvent.duration = m_info.loseInfo.duration;
                        qteEvent.name = m_info.loseInfo.events.Count + ":" + QteEvent.TypeName[t];
                        m_info.loseInfo.events.Add(qteEvent);
                        UpdateEventEditor();
                    });
                }
                menu.ShowAsContext();
                qteEditor.Repaint();
            }
            using (AutoBeginScrollView sv = new AutoBeginScrollView(m_scroll, "PreferencesSectionBox", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                m_scroll = sv.Scroll;
                using (new AutoBeginVertical())
                {
                    for (int i = 0; i < m_loseEventEditors.Count; i++)
                    {
                        m_loseEventEditors[i].OnGUI();
                    }
                }
            }

        }

        float totleEndPosX = qteEditor.MaxPosX;
        float endPosX = qteEditor.TimeToContentX(LoseEndTime);
        float startPosX = qteEditor.TimeToContentX(StartTime);

        //时间显示
        Rect textRect = new Rect(endPosX - 42, 0, 34, 17);
        GUI.TextField(textRect, LoseDuration.ToString("#.##"));

        //拖动柄
        Handle_Rect = new Rect(endPosX - 8.0f, 0, 8.0f, 15);
        using (new AutoBeginArea(Handle_Rect, "", "box"))
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                m_bLoseDragStage = true;

            if (m_bLoseDragStage && Event.current.type == EventType.MouseDrag)
            {
                endPosX += Event.current.delta.x;
                if (endPosX < startPosX)
                    endPosX = startPosX + 5;
                if (endPosX >= totleEndPosX)
                    endPosX = totleEndPosX;

                float endTime = qteEditor.ContentXToTime(endPosX);
                LoseDuration = endTime - StartTime;
                UpdateEventEditor();
                Event.current.Use();
                qteEditor.Repaint();
            }
        }

        if (Event.current.type == EventType.MouseUp)
            m_bLoseDragStage = false;
    }
}
