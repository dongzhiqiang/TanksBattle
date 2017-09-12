using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AnimatorHandle), true)]
public class AnimatorHandleEditor : Editor
{
    AnimatorHandle m_cur;

    void OnEnable()
    {
        m_cur = target as AnimatorHandle;
    }
    
    public override void OnInspectorGUI ()
	{
        //base.OnInspectorGUI();
        m_cur.m_playOnEnable = EditorGUILayout.Toggle("enable时运行", m_cur.m_playOnEnable);        
        m_cur.m_ani = (Animator)EditorGUILayout.ObjectField("动作组件", m_cur.m_ani,typeof(Animator),true);
        Animator ani = m_cur.m_ani;
        List<string> l = new List<string>();
        if(ani != null)
        {
            AnimationClip[] clips =ani.runtimeAnimatorController.animationClips;
            if(clips != null)
            {
                foreach (var clip in clips)
                {
                    if (clip != null && !l.Contains(clip.name)&&!string.IsNullOrEmpty(clip.name))
                        l.Add(clip.name);

                }
            }
        }
        if(l.Count!=0)
        {
            var aniNames = l.ToArray();
            int idx = System.Array.IndexOf(aniNames, m_cur.m_curAni);
            int idxNew = EditorGUILayout.Popup("动作", idx, aniNames);
            if (idx != idxNew)
            {
                m_cur.m_curAni = aniNames[idxNew];
            }
        }

        if (l.Count != 0&&Application.isPlaying)
        {
            using (new AutoEditorDisabledGroup(string.IsNullOrEmpty(m_cur.m_curAni)))
            {
                if(GUILayout.Button("播放"))
                {
                    m_cur.Play();
                }
            }
        }

    }
}
