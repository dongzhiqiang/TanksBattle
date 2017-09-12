#region Header
/**
 * 名称：mono类模板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetCore;

namespace TestNetCore
{
    public class TestClient : MonoBehaviour
    {
        #region Fields
        public static TestClient instance;
        TestRole m_hero ;
        TestClientSocket m_socket =null;
        #endregion


        #region Properties
        
        #endregion
        
        #region Mono Frame
        void Start()
        {
            instance = this;

            TestClientSocket.Add(TestMSG.TMSG_ROLE, TMSG_ROLE.LOGIN, OnLogin);
            TestClientSocket.Add(TestMSG.TMSG_ROLE, TMSG_ROLE.SYNC, OnSync);
            TestClientSocket.Add(TestMSG.TMSG_ROLE, TMSG_ROLE.RENAME, OnRename);
            TestClientSocket.Add(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, OnLevelFight);
            TestClientSocket.Add(TestMSG.TMSG_TASK, TMSG_TASK.GET_REWARD, OnTaskGetReward);
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        string inputName = "名字七个字";
        Vector2 bagScroll = Vector2.zero;
        Vector2 taskScroll = Vector2.zero;
        Vector2 levelScroll = Vector2.zero;
        void OnGUI()
        {
            if (m_socket == null)
            {
                if (GUILayout.Button("登录"))
                {
                    m_socket = TestClientSocket.Connect();
                    IoBuffer s =m_socket .GetStream();
                    m_socket.Send(TestMSG.TMSG_ROLE, TMSG_ROLE.LOGIN, s);
                }
                return;
            }
            using (new AutoChangeColor(Color.green))
            {
                //绘制玩家信息
                DrawInfo();

                //绘制玩家操作
                DrawHandle();
            }
        }
        #endregion
   


        #region Private Methods
        //绘制玩家的相关信息
        void DrawInfo()
        {
            TestPropertyPart prop = m_hero.m_propPart;
            TestBagPart bag = m_hero.m_bagPart;
            TestTaskPart task = m_hero.m_taskPart;
            TestLevelPart level = m_hero.m_levelPart;

            using (new AutoBeginHorizontal(GUILayout.Height(150)))
            {
                //属性
                using (new AutoBeginVertical())
                {
                    GUILayout.Label(string.Format("名字:{0}", prop.GetStr(enTestProp.name)));
                    GUILayout.Label(string.Format("经验:{0}", prop.GetInt(enTestProp.exp)));
                    GUILayout.Label(string.Format("金币:{0}", prop.GetInt(enTestProp.gold)));
                }

                GUILayout.Space(20);
                //背包
                using (new AutoBeginVertical())
                {
                    using (new AutoBeginScrollView(bagScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        GUILayout.Label("背包");
                        foreach (TestGoods g in bag.m_goods)
                        {
                            GUILayout.Label(string.Format("{0} x{1}", g.Name.PadRight(8,' '), g.num));
                        }
                    }
                }

                GUILayout.Space(20);
                //任务
                using (new AutoBeginVertical())
                {
                    using (new AutoBeginScrollView(taskScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        GUILayout.Label("任务:");
                        foreach (TestTask t in task.m_tasks)
                        {
                            GUILayout.Label(string.Format("id:{0} 状态:{1}", t.taskId, TestTask.StateName[t.state]));
                        }
                    }
                }

                GUILayout.Space(20);
                //关卡
                using (new AutoBeginVertical())
                {
                    using (new AutoBeginScrollView(levelScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        GUILayout.Label("关卡:");
                        foreach (TestLevelInfo l in level.m_levelInfos.Values)
                        {
                            GUILayout.Label(string.Format("id:{0} 进入次数:{1} 最高星星数：{2}", l.levelId, l.EnterCount, l.star));
                        }
                    }
                }
            }   
        }

        //玩家操作
        void DrawHandle()
        {
            TestPropertyPart prop = m_hero.m_propPart;
            TestBagPart bag = m_hero.m_bagPart;
            TestTaskPart task = m_hero.m_taskPart;
            TestLevelPart level = m_hero.m_levelPart;

            
            using (new AutoBeginHorizontal(GUILayout.Height(150)))
            {
                //重命名
                using (new AutoBeginVertical())
                {
                    inputName = GUILayout.TextField(inputName, GUILayout.Width(150));
                    if (GUILayout.Button("改名字", GUILayout.Width(50)))
                    {
                        IoBuffer s = m_socket.GetStream();
                        s.Write(inputName);
                        m_socket.Send(TestMSG.TMSG_ROLE, TMSG_ROLE.RENAME, s);
                    }
                }

                //任务操作
                

                //关卡操作
                using (new AutoBeginVertical())
                {
                    if (GUILayout.Button("挑战关卡1", GUILayout.Width(70)))
                    {
                        IoBuffer s = m_socket.GetStream();
                        s.Write(1);
                        m_socket.Send(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, s);
                    }
                    if (GUILayout.Button("挑战关卡2", GUILayout.Width(70)))
                    {
                        IoBuffer s = m_socket.GetStream();
                        s.Write(2);
                        m_socket.Send(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, s);
                    }
                }
            }
        }

        


        #endregion

        #region net
        void OnLogin(IoBuffer s)
        {
            bool ret = s.ReadBool();
            Debuger.LogError("登录{0}", ret ? "成功" : "失败");
            m_hero = new TestRole();
            m_hero.Init();

            bool needDeserialize = s.ReadBool();
            if(needDeserialize)
                m_hero.Deserialize(s);
        }
        void OnSync(IoBuffer s)
        {
            m_hero.Deserialize(s);
            Debuger.LogError("同步玩家数据");
        }
        void OnRename(IoBuffer s)
        {
            bool ret = s.ReadBool();
            Debuger.LogError("重命名{0}", ret ? "成功" : "失败"); 
        }

        void OnLevelFight(IoBuffer s)
        {
            bool ret = s.ReadBool();
            Debuger.LogError("关卡挑战{0}", ret?"成功":"失败");
        }

        void OnTaskGetReward(IoBuffer s)
        {
            bool ret = s.ReadBool();
            Debuger.LogError("领取奖励{0}", ret ? "成功" : "失败");
        }

        #endregion

    }
}