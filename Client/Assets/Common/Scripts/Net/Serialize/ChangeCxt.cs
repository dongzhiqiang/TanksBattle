#region Header
/**
 * 名称: 序列化类
 
 * 日期：2015.10.10
 * 描述：
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace NetCore
{
    public class ChangeCxt//变化的上下文，用于记录对子节点的操作
    {
        public enSerializeChangeType type;
        public int idx = -1;
        public string key = string.Empty;
        public ISerializableObject obj = null;

    }
}