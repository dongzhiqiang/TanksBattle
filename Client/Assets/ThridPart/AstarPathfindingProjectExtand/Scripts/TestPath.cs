using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestPath : MonoBehaviour {
    public enum enType{
        touch,//点到哪里走到哪里
        multiOnce,//多点寻路，单次
        multiLoop,//多点寻路，多点寻路循环
        multiPingPong,//多点寻路，多点寻路循环
        twoPointAvoid,//面对面走，测试回避
        max
    }
    
    public static string[] TypeNames = {
        "点到哪里走到哪里",
        "多点寻路，单次",
        "多点寻路，多点寻路循环",
        "多点寻路，多点寻路来回",
        "面对面走，测试回避",
    };

    public Transform[] m_multiLoopTrans;
    public Transform[] m_multiPingPongTrans;
    public Transform m_avoidTran1;
    public Transform m_avoidTran2;

    enType m_type = enType.touch;
    SimpleRole[] m_roles;
    Camera m_ca;
    List<Vector3> m_loopPoss = new List<Vector3>();
    List<Vector3> m_pingPongPoss = new List<Vector3>();
    Vector3 m_avoid1;
    Vector3 m_avoid2;
    int m_avoidCounter=0;

	// Use this for initialization
	void Start () {
        m_roles = Object.FindObjectsOfType<SimpleRole>();
        m_ca= this.GetComponent<Camera>();
        if(m_ca== null)
            Debuger.LogError("找不到相机，这个脚本需要挂在相机上");

        if (m_multiLoopTrans == null || m_multiLoopTrans.Length <= 1)
            Debuger.LogError("必须设置多个循环路径点，否则不能测试多点寻路");
        else
        {
            foreach (var t in m_multiLoopTrans)
                if(t!= null)
                    m_loopPoss.Add(t.transform.position);
        }

        if (m_multiPingPongTrans == null || m_multiPingPongTrans.Length <= 1)
            Debuger.LogError("必须设置多个来回路径点，否则不能测试多点寻路");
        else
        {
            foreach (var t in m_multiPingPongTrans)
                if (t != null)
                    m_pingPongPoss.Add(t.transform.position);
        }

        if (m_avoidTran1 == null || m_avoidTran2 == null)
            Debuger.LogError("必须两个测试回避用的点都设置，不能测试回避");
        else
        {
            m_avoid1 = m_avoidTran1.transform.position;
            m_avoid2 = m_avoidTran2.transform.position;
        }

	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_STANDALONE_WIN ||UNITY_EDITOR
        //修改时间缩放
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Time.timeScale = Time.timeScale * 2f;
            if (Mathf.Abs(Time.timeScale - 1) < 0.001f)
                Time.timeScale = 1;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (Time.timeScale >= 0.00001)
                Time.timeScale /= 2f;
        }

        
#endif
        if(!Input.GetMouseButtonUp(0) || m_ca == null )
            return;

        if (m_type == enType.touch)
        {
            Vector3 screenPos = Input.mousePosition;
            Ray ray = m_ca.ScreenPointToRay(screenPos);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100, 1 << 0))// LayerMask.GetMask("Default")
            {
                foreach (SimpleRole r in m_roles)
                {
                    r.MovePos(hitInfo.point);
                }
            }
            else
                Debuger.Log("点击的点找不到地面");
        }
        else if (m_type == enType.multiOnce)
        {
            foreach (SimpleRole r in m_roles)
                r.RolePath.Move(m_pingPongPoss, RolePath.enPathType.once);
        }
        else if (m_type == enType.multiLoop)
        {
            foreach (SimpleRole r in m_roles)
                r.RolePath.Move(m_loopPoss, RolePath.enPathType.loop);
        }
        else if (m_type == enType.multiPingPong)
        {
            foreach (SimpleRole r in m_roles)
                r.RolePath.Move(m_pingPongPoss, RolePath.enPathType.pingPong);
        }
        else if (m_type == enType.twoPointAvoid)
        {
            if(m_roles.Length >=2)
            {
                ++m_avoidCounter;

                m_roles[0].MovePos(m_avoidCounter%2==0?m_avoid1: m_avoid2);
                m_roles[1].MovePos(m_avoidCounter % 2 == 0 ? m_avoid2 : m_avoid1);

            }
        
        }
	}

    void OnGUI()
    {
        m_type = (enType)GUI.SelectionGrid(new Rect(10, 10, 200, 30 * (int)enType.max), (int)m_type, TypeNames, 1);

        RolePath.s_debug = GUI.Toggle(new Rect(250, 10, 200, 30), RolePath.s_debug, "调试信息显示");
        RolePath.s_canStuckStop = GUI.Toggle(new Rect(250, 40, 200, 30), RolePath.s_canStuckStop, "目标点附近卡住暂停");
        RolePath.s_canStuckAvoid = GUI.Toggle(new Rect(250, 80, 200, 30), RolePath.s_canStuckAvoid, "走路回避");
    }
}
