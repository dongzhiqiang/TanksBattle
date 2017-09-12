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
    public class TestServer : MonoBehaviour
    {
        #region Fields
        public static TestServer instance;
        
        #endregion


        #region Properties
        
        #endregion
        
        #region Mono Frame
        void Awake(){
            instance = this;
            TestServerSocket.Add(TestMSG.TMSG_ROLE, TMSG_ROLE.LOGIN, OnLogin);
            TestServerSocket.Add(TestMSG.TMSG_ROLE, TMSG_ROLE.RENAME, OnRename);
            TestServerSocket.Add(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, OnLevelFight);
            TestServerSocket.Add(TestMSG.TMSG_TASK, TMSG_TASK.GET_REWARD, OnTaskGetReward);
        }
        void Start()
        {
            
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        
        void OnGUI()
        {
        
        }
        #endregion
   


        #region Private Methods
        
                
        #endregion
        

        #region net
        public TestServerSocket OnConnect(TestClientSocket clientSocket)
        {
            TestServerSocket serverSocket = new TestServerSocket();
            serverSocket.m_clientSocket = clientSocket;
            serverSocket.m_hero.Init();

            //从数据库获取数据初始化玩家
            //未实现

            return serverSocket;
        }
        void OnLogin(TestServerSocket socket,IoBuffer s)
        {
            IoBuffer outStream =socket.GetStream();
            outStream.Write(true);

            //发消息
            outStream.Write(socket.m_hero.ValueChange);
            if (socket.m_hero.ValueChange)
               socket.m_hero.SerializeChange(outStream);

            socket.Send(TestMSG.TMSG_ROLE, TMSG_ROLE.LOGIN, outStream);
        }
        
        void OnRename(TestServerSocket socket,IoBuffer s)
        {
            socket.m_hero.m_propPart.Set(enTestProp.name, s.ReadStr());
            IoBuffer outStream = socket.GetStream();
            outStream.Write(true);
            socket.Send(TestMSG.TMSG_ROLE, TMSG_ROLE.RENAME, outStream);
        }

        void OnLevelFight(TestServerSocket socket,IoBuffer s)
        {
            IoBuffer outStream = socket.GetStream();
            TestRole role = socket.m_hero;

            int levelId = s.ReadInt32();
            TestLevelInfo levelInfo = role.m_levelPart.m_levelInfos.Get(levelId);
            //检查开没开启
            if (levelInfo == null && levelId != 1 && role.m_levelPart.m_levelInfos.Get(levelId - 1) == null)
            {
                Debuger.LogError("关卡没有开启");
                outStream.Write(false);
                socket.Send(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, outStream);
                return;
            }

            //检查今天的挑战数
            if(levelInfo!= null && levelInfo.EnterCount >= 5)
            {
                Debuger.LogError("每天只能挑战5次");
                outStream.Write(false);
                socket.Send(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, outStream);
                return;
            }

            //挑战次数加一
            if (levelInfo == null)
            {
                levelInfo = new TestLevelInfo();
                levelInfo.levelId.Value = levelId;                
                role.m_levelPart.m_levelInfos[levelId] = levelInfo;
                
            }
            levelInfo.star.Value=Mathf.Max((int)levelInfo.star,Random.Range(1,3));
            levelInfo.Add();

            //给奖励,50金币，1把武器
            role.m_propPart.Add(enTestProp.gold, 50);
            role.m_bagPart.Add(Random.Range(0, 2), 1);
            
            outStream.Write(true);
            socket.Send(TestMSG.TMSG_LEVEL, TMSG_LEVEL.FIGHT_OVER, outStream);
        }

        void OnTaskGetReward(TestServerSocket socket,IoBuffer s)
        {
            //bool ret = s.ReadBool();
            //Debuger.LogError("领取奖励{0}", ret ? "成功" : "失败");
        }

        #endregion


    

    }
}