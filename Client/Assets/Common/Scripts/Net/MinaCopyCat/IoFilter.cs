#region Header
/**
 * 名称：IoFilter
 
 * 日期：2015.11.3
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NetCore
{
    public interface IoFilter
    {
        //true 表明已经生成至少一个obj了 ，false 则表明一个也没有
        void decode(IoBuffer inBuffer, List<Message> outObjs);

        void encode(Message inObj, IoBuffer ouBuffer);

        //重置，在重新建立链接的时候会调用
        void reset();
    }

}