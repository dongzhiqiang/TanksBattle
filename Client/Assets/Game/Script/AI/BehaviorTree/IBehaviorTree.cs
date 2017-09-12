#region Header
/**
 * 名称：IBehaviorTree
 
 * 日期：2016.5.13
 * 描述：创建和销毁不归行为树自己控制，见具体实现
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Simple.BehaviorTree
{
   
    public interface IBehaviorTree
    {
        BehaviorTreeCfg Cfg { get; }
        bool IsPlaying { get; }
        
        //播放，如果已经在播放中了会报错
        void Play(string file, string behavior);

        //停止
        void Stop();

        //重新初始化，简单来说相当于重新Init()和Play(),一般用于编辑器修改树的结构
        void ReCreate();

        //手动更新
        void CallUpdate();

        //暂停
        void Pause();

        //重新播放
        void RePlay(bool reset);



        //获取变量的值，可能是常量(配置的)、树变量、全局变量
        ShareValueBase<T> GetValue<T>(string name);
    }
}