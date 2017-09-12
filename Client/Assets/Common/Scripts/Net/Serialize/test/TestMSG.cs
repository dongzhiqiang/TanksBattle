#region Header
/**
 * 名称：mono类模板
 
 * 日期：201x.x.x
 * 描述：新建继承自mono的类的时候建议用这个模板
 **/
#endregion
//using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetCore;

namespace TestNetCore
{
    public class TestMSG
    {
        //GM
        public const int TMSG_GM =1;

        //角色
        public const int TMSG_ROLE = 2;

        //关卡
        public const int TMSG_LEVEL = 3;

        //任务
        public const int TMSG_TASK = 4;
    }

    //GM模块
    public class TMSG_GM
    {
        //改服务器时间
        public const int CHANGE_TIME =1;//CSC
    }

    //角色
    public class TMSG_ROLE
    {
        //登录
        public const int LOGIN = 1;//CSC

        //同步
        public const int SYNC = 2;//SC

        //重命名
        public const int RENAME = 3;//CSC
    }

    //关卡
    public class TMSG_LEVEL
    {
        //挑战完成
        public const int FIGHT_OVER = 1;//CSC
    }

    //任务
    public class TMSG_TASK
    {
        //领取任务
        public const int GET_REWARD= 1;//CSC
    }

   

}