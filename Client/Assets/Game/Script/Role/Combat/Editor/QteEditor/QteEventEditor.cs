using UnityEngine;
using UnityEditor;
using System.Collections;

public class QteEventEditor
{
    Rect Event_Rect;
    bool m_bHandleDragEvent;
    float Event_H = 100;
    bool m_bShow = false;
    public float StageStartTime = 0;
    public float StageEndTime = 0;
    public QteEvent m_event;
    public QteStageEditor m_stageEditor;
    public float Duration
    {
        get { return m_event.duration; }
    }
    public float StartTime
    {
        get { return StageStartTime + m_event.startTime; }
    }

    public float EndTime
    {
        get { return StageStartTime + m_event.startTime + m_event.duration; }
    }

    public void Init(QteEvent e)
    {
        m_event = e;
    }

    public void OnGUI()
    {

        GUILayout.Box("", EditorStyleEx.BoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(Event_H));

        if (Event.current.type == EventType.Repaint)
        {
            if (Event_Rect != GUILayoutUtility.GetLastRect())
                Event_Rect = GUILayoutUtility.GetLastRect();
        }

        using (new AutoBeginArea(Event_Rect))
        {
            //if (GUI.Button(new Rect(5, 5, 30, 30),  "", m_bShow ? EditorStyleEx.DownArrow : EditorStyleEx.UpArrow ))
            //{
            //    m_bShow = !m_bShow;
            //}

            if (m_bShow)
            {
                if (GUI.Button(new Rect(3, Event_Rect.height - 15, 25, 25), EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyleEx.GraphBoxStyle))
                {
                    foreach(QteEvent e in m_stageEditor.m_info.winInfo.events)
                    {
                        if (e.name == m_event.name)
                        {
                            if (EditorUtility.DisplayDialog("删除事件", "确定要删除事件"+ m_event.name + "吗？", "确定", "取消"))
                            {
                                m_stageEditor.m_info.winInfo.events.Remove(e);

                                if (e.m_type == enQteEventType.Qte_Operate)
                                {
                                    BigQteCfg cfg = QteEditor.instance.CurQte.Cfg;
                                    cfg.stages[m_stageEditor.m_info.idx + 1].loseInfo = null;
                                }
                                
                                m_stageEditor.UpdateEventEditor();
                                QteEditor.instance.Repaint();
                                return;
                            }
                        }
                    }
                }
            }
            Event_H = 20;
            float posx = QteEditor.instance.TimeToContentX(StartTime);
            float startPosx = QteEditor.instance.TimeToContentX(m_event.startTime);
            float endPosx = QteEditor.instance.TimeToContentX(m_event.startTime + m_event.duration);
            float stageStartPosx = QteEditor.instance.TimeToContentX(StageStartTime);
            float stageEndPosx = QteEditor.instance.TimeToContentX(StageEndTime - Duration);
            //右击
            //if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            //{
            //    GenericMenu menu = new GenericMenu();
            //    string[] qtes = { "" };
            //    foreach (string q in qtes)
            //    {
            //        string t = q;
            //        menu.AddItem(new GUIContent(q), false, () =>
            //        {
            //        });
            //    }
            //    menu.ShowAsContext();
            //    Event.current.Use();
            //    QteEditor.instance.Repaint();
            //}

            if (m_bShow)
            {
                Event_H = 85;
                Rect eventRect = new Rect(startPosx, 0, endPosx - startPosx, Event_H);
                GUI.Box(eventRect, "", EditorStyleEx.GraphBoxStyle);
                //绘制自定义编辑内容
                using (new AutoBeginArea(eventRect))
                {

                    using (new AutoLabelWidth(32))
                    {
                        //事件时间
                        using (new AutoBeginHorizontal())
                        {
                            float time = EditorGUILayout.FloatField("时长:", m_event.duration, GUILayout.ExpandWidth(false));
                            if (time != m_event.duration)
                            {
                                if (time < 0.1f)
                                    time = 0.1f;

                                m_event.duration = time;
                                if (EndTime > StageEndTime)
                                    m_event.duration = StageEndTime - StartTime;

                            }
                            EditorGUILayout.LabelField(m_event.name);
                        }
                        this.DrawCustom();
                    }
                }
                QteEditor.instance.Repaint();
            }
            else
            {
                Rect eventRect = new Rect(startPosx, 0, endPosx - startPosx, Event_H);
                GUI.Box(eventRect, "", EditorStyleEx.GraphBoxStyle);
                //绘制自定义编辑内容
                using (new AutoBeginArea(eventRect))
                {
                    using (new AutoLabelWidth(32))
                    {
                        EditorGUILayout.LabelField(m_event.name);
                    }
                }
                QteEditor.instance.Repaint();
            }

            //点击
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                QteContentEditor.ShowEvent(this);
                Event.current.Use();
                QteEditor.instance.Repaint();
                return;
            }

            //拖动
            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
                m_bHandleDragEvent = true;

            if (m_bHandleDragEvent && Event.current.type == EventType.MouseDrag)
            {
                posx += Event.current.delta.x;
                if (posx < stageStartPosx)
                    posx = stageStartPosx;
                if (posx >= stageEndPosx)
                    posx = stageEndPosx;

                float startTime = QteEditor.instance.ContentXToTime(posx);
                m_event.startTime = startTime - StageStartTime;
                Event.current.Use();
                QteEditor.instance.Repaint();
            }

           
        }

        if (Event.current.type == EventType.MouseUp)
            m_bHandleDragEvent = false;
    }

    public virtual void DrawCustom()
    {
    }

    public static QteEventEditor CreateEventEditor(QteEvent e, float startTime)
    {
        QteEventEditor editor = null;
        switch (e.m_type)
        {
            case enQteEventType.Qte_Ani:
                editor = new QteEventAniEditor();
                break;
            case enQteEventType.Qte_TimeScale:
                editor = new QteEventTimeScaleEditor();
                break;
            case enQteEventType.Qte_Operate:
                editor = new QteEventOperateEditor();
                break;
            case enQteEventType.Qte_Blur:
                editor = new QteEventBlurEditor();
                break;
            case enQteEventType.Qte_EventGroup:
                editor = new QteEventGroupEditor();
                break;
            case enQteEventType.Qte_Fx:
                editor = new QteEventFxEditor();
                break;
        }

        if (editor == null)
        {
            Debuger.LogError("没有找到相应类型的编辑器 type: " + e.m_type);
            return null;
        }

        editor.Init(e);
        editor.StageStartTime = startTime;
        return editor;
    }

}
