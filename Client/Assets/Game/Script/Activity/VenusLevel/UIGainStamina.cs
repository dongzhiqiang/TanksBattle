using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIGainStamina : UIPanel
{
    public List<UIStaminaButton> m_buttons;
    public TextEx m_soul;
    public ImageEx m_percentCover;
    public GameObject m_a;
    public GameObject m_s;
    public GameObject m_ss;
    public GameObject m_sss;
    public GameObject m_upFx;
    public GameObject m_grid;
    public GameObject m_aFx;
    public GameObject m_sFx;
    public GameObject m_ssFx;
    public GameObject m_sssFx;

    private List<UIStaminaButton> m_freeButtons = new List<UIStaminaButton>();
    private bool m_started = false;
    private float m_startTime;
    private int m_buttonIndex;
    private System.Random m_random = new System.Random();
    private float m_percentage;
    private int m_evaluation;
    private int m_soulVal;
    private Dictionary<int, GameObject> m_evalObjs;
    private Dictionary<int, string> m_evalAnis;
    private Dictionary<int, GameObject> m_evalFxs;
    private VenusLevelScene m_scene;

    public override void OnInitPanel()
    {
        for (int i = 0; i < 14; i++ )
        {
            GameObject copyed = GameObject.Instantiate(m_buttons[0].gameObject);
            copyed.transform.SetParent(m_grid.transform);
            copyed.transform.localPosition = Vector3.zero;
            copyed.transform.localScale = Vector3.one;
            m_buttons.Add(copyed.GetComponent<UIStaminaButton>());
        }
        foreach (UIStaminaButton button in m_buttons)
        {
            button.SetParent(this);
        }
        m_evalObjs = new Dictionary<int, GameObject>();
        m_evalObjs[3] = m_a;
        m_evalObjs[4] = m_s;
        m_evalObjs[5] = m_ss;
        m_evalObjs[6] = m_sss;
        m_evalAnis = new Dictionary<int, string>();
        m_evalAnis[3] = "ui_a";
        m_evalAnis[4] = "ui_s";
        m_evalAnis[5] = "ui_ss";
        m_evalAnis[6] = "ui_sss";
        m_evalFxs = new Dictionary<int, GameObject>();
        m_evalFxs[3] = m_aFx;
        m_evalFxs[4] = m_sFx;
        m_evalFxs[5] = m_ssFx;
        m_evalFxs[6] = m_sssFx;
        
    }

    public override void OnOpenPanel(object param)
    {
        m_scene = param as VenusLevelScene;
        foreach(UIStaminaButton button in m_buttons)
        {
            button.Clear();
        }
        foreach (GameObject fx in m_evalFxs.Values)
        {
            fx.SetActive(false);
        }
        m_started = false;
        m_percentage = 0;
    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
   
    }

    void SetPercentage(float percentage)
    {
        if(percentage>100)
        {
            percentage = 100;
        }
        m_percentage = percentage;
        m_percentCover.fillAmount = 1 - m_percentage / 100;
    }

    void SetSoul(int soul)
    {
        int addedSoul = soul - m_soulVal;
        SetSoulBase(soul);
    }

    void SetSoulBase(int soul)
    {
        m_soulVal = soul;
        m_soul.text = m_soulVal.ToString();
    }

    public void StartPlay()
    {
        SetPercentage(0);
        SetSoulBase(0);
        m_evaluation = VenusLevelRewardCfg.minEvaluate;
        m_startTime = Time.time;
        m_started = true;
        m_buttonIndex = 0;
        m_freeButtons.Clear();
        foreach (UIStaminaButton button in m_buttons)
        {
            m_freeButtons.Add(button);
        }
    }

    UIStaminaButton GetFreeButton()
    {
        if (m_freeButtons.Count==0)
        {
            return null;
        }
        UIStaminaButton button = m_freeButtons[m_random.Next(m_freeButtons.Count)];
        m_freeButtons.Remove(button);
        return button;
    }


    void Update()
    {
        if(m_started)
        {
            if(VenusLevelButtonCfg.m_cfgs.Count>m_buttonIndex)
            {
                VenusLevelButtonCfg buttonCfg = VenusLevelButtonCfg.Get(m_buttonIndex);
                if(Time.time-m_startTime>=buttonCfg.time)
                {
                    for(int i=0; i<buttonCfg.num; i++)
                    {
                        UIStaminaButton button = GetFreeButton();
                        if(button != null)
                        {
                            button.StartFx(buttonCfg.fx);
                        }
                    }
                    m_buttonIndex++;
                }
            }
            else
            {
                if(m_freeButtons.Count == m_buttons.Count)
                {
                    Result();
                }
            }
        }
    }

    void Result()
    {
        // 结算
        foreach (UIStaminaButton button in m_buttons)
        {
            button.Clear();
        }
        m_started = false;
        m_scene.Result(m_evaluation, m_percentage);
    }

    public void PutButton( UIStaminaButton button )
    {
        m_freeButtons.Add(button);
    }

    public void AddPercentage(bool perfect)
    {
        if(perfect)
        {
            AddPercentage(5);
        }
        else
        {
            AddPercentage(2.5f);
        }
        
    }

    void AddPercentage(float add)
    {
        SetPercentage(m_percentage + add);
        if(m_evaluation<VenusLevelRewardCfg.maxEvaluate)
        {
            VenusLevelRewardCfg nextReward = VenusLevelRewardCfg.m_cfgs[m_evaluation + 1];
            if(nextReward.minPercentage<=m_percentage)
            {
                SoundMgr.instance.Play2DSoundAutoChannel(304);
                m_evaluation++;
                SetSoul(nextReward.soul);
                /*
                m_upFx.transform.localPosition = m_evalObjs[m_evaluation].transform.localPosition;
                m_upFx.SetActive(false);
                m_upFx.SetActive(true);
                 */
                m_evalObjs[m_evaluation].GetComponent<Animator>().Play(m_evalAnis[m_evaluation]);
                m_evalFxs[m_evaluation].SetActive(true);
            }
        }
        m_scene.PlayEffect(m_evaluation);
        if(m_evaluation==6)
        {
            Result();
        }
    }
}
