using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SoundTest))]
public class SoundTestEditor : Editor
{
#if !ART_DEBUG
    public CapsuleCollider[] enemys;

    SoundTest m_st;
    int m_UISoundId = 3;
    int m_bgmId = 1;
    int m_2dSoungId = 2;
    int m_3dSoundId = 4;
    List<int> m_3dSoundIds = new List<int>();
    bool isPlayingBGM;
    bool isStopBGM;

    public override void OnInspectorGUI()
    {
        m_st = (SoundTest)target;
        CheckInit();
        using (new AutoEditorDisabledGroup(!Application.isPlaying))  //运行模式才可编辑
            DrawDisplay();
    }
    void CheckInit()
    {
        if (enemys != null)
            return;
        enemys = GameObject.FindObjectsOfType<CapsuleCollider>();
  
    }
    void DrawDisplay()
    {
        using (new AutoBeginHorizontal())
        {
            string m = EditorGUILayout.TextField("UI音效 id：", m_UISoundId.ToString(), GUILayout.Height(22));
            m_UISoundId = int.Parse(m);
            if (GUILayout.Button("播UI音效", GUILayout.Height(20), GUILayout.Width(60)))
                SoundMgr.instance.Play2DSound(Sound2DType.ui, m_UISoundId);

        }
        GUILayout.Space(8f);

        using (new AutoBeginHorizontal())
        {
            string m = EditorGUILayout.TextField("2D音效 id：", m_2dSoungId.ToString(), GUILayout.Height(22));
            m_2dSoungId = int.Parse(m);
            if (GUILayout.Button("播2D音效", GUILayout.Height(20), GUILayout.Width(60)))
                SoundMgr.instance.Play2DSound(Sound2DType.other, m_2dSoungId);

        }
        GUILayout.Space(8f);

        using (new AutoBeginHorizontal())
        {
            string m = EditorGUILayout.TextField("BGM id：", m_bgmId.ToString(), GUILayout.Height(22));
            m_bgmId = int.Parse(m);
            if (!isPlayingBGM && GUILayout.Button("播放BGM", GUILayout.Height(20), GUILayout.Width(60)))
            {
                SoundMgr.instance.Play2DSound(Sound2DType.bgm, m_bgmId);
                isPlayingBGM = true;
            }

            if (isPlayingBGM && !isStopBGM && GUILayout.Button("停止BGM", GUILayout.Height(20), GUILayout.Width(60)))
            {
                SoundMgr.instance.Pause2DSound(Sound2DType.bgm, true);
                isStopBGM = true;
            }
            if (isStopBGM)
            {
                if(GUILayout.Button("继续BGM", GUILayout.Height(20), GUILayout.Width(60)))
                {
                    SoundMgr.instance.Pause2DSound(Sound2DType.bgm, false);
                    isStopBGM = false;
                }
            }
        }
        
        GUILayout.Space(8f);

        GUILayout.Label("测试3D音效");
        int len = enemys.Length;
        for(int i=0;i<len;i++)
        {
            if (enemys[i].name == "Enemy")
            {
                m_3dSoundIds.Add(5);
                using (new AutoBeginHorizontal())
                {
                    string m = EditorGUILayout.TextField(" 3d音效 id：", m_3dSoundIds[i].ToString(), GUILayout.Height(22));
                    m_3dSoundIds[i] = int.Parse(m);

                    if (GUILayout.Button("测试", GUILayout.Height(20), GUILayout.Width(60)))
                        SoundMgr.instance.Play3DSound(m_3dSoundIds[i], enemys[i].transform);
                }
            }
        }
        GUILayout.Space(8f);
        using (new AutoChangeBkColor(Color.green))
            if (GUILayout.Button("停止所有音效", GUILayout.Height(20), GUILayout.Width(120)))
            {
                SoundMgr.instance.StopAllSounds();
                isPlayingBGM = false;
            }

    }
#endif
}
