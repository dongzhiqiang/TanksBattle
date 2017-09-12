#region Header
/**
 * 名称：消息处理
 
 * 日期：2015.11.3
 * 描述：消息处理类。和编解码器(MessageDecoder、MessageEncoder)一起处于MinaCopyCat框架的上层
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;

namespace NetCore
{
    public class ModuleDefinition
    {
        public byte     module;
        public object   facade;
        public Type     type;
        public Dictionary<int, CommandDefinition> cmdDefs = new Dictionary<int, CommandDefinition>();
    }

    public class CommandDefinition{
        public ModuleDefinition parent;
        public int              command;
        public MethodInfo       methodInfo;
        public Type             bodyType;
        public bool             hasErrorCode;
        public bool             hasErrorMsg;
        public void Process(int errCode, string errMsg, object bodyObj)
        {
            object[] args = null;
            if (bodyType == null)
            {
                if (hasErrorCode)
                {
                    if (hasErrorMsg)
                        args = new object[] { errCode, errMsg };
                    else
                        args = new object[] { errCode };
                }
                else
                {
                    args = new object[] { };
                }
            }
            else
            {
                if (hasErrorCode)
                {
                    if (hasErrorMsg)
                        args = new object[] { errCode, errMsg, bodyObj };
                    else
                        args = new object[] { errCode, bodyObj };
                }
                else
                {
                    args = new object[] { bodyObj };
                }
            }
            methodInfo.Invoke(parent.facade, args);
        }
    }

    public class MessageHandle : IoHandler
    {
        #region Fields
        public static bool s_prettyPrint = false;

        System.Func<int, int, CommandDefinition> m_commandRegister;
        System.Action m_onConnectOK;
        System.Action m_onConnectFail;
        System.Action<SocketError, string> m_onConnError;
        System.Func<int, int, bool> m_msgFilter;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public MessageHandle()
        {    
        }
        #endregion

        #region IoHandler
        public void OnConnectOK()
        {
            if (m_onConnectOK != null)
                m_onConnectOK();
        }

        public void OnConnectFail()
        {
            if (m_onConnectFail != null)
                m_onConnectFail();
        }

        public void OnConnError(SocketError error, string info)
        {
            if (m_onConnError != null)
                m_onConnError(error, info);
        }

        public void MessageReceived(Message message)
        {
            if (message == null)
            {
                Debuger.LogError("message == null");
                return;
            }

            int module = message.Module;
            int command = message.Command;
            bool isReponse = message.IsResponse();
            int errorCode = message.ErrorCode;
            string errorMsg = message.ErrorMsg;

            try
            {
                //如果filter返回false表示这个消息暂时不让处理
                if (!m_msgFilter(module, command))
                    return;

                CommandDefinition cmdDef = m_commandRegister(module, command);
                if (cmdDef == null)
                {
                    Debuger.LogError("找不到模块或命令定义，模块：{0} 命令：{1}", module, command);
                    return;
                }

                object bodyObj = null;
                try
                {
                    bodyObj = cmdDef.bodyType == null ? null : message.GetBodyObject(cmdDef.bodyType);

#if UNITY_EDITOR
                    if (!(module == MODULE.MODULE_ACCOUNT && command == MODULE_ACCOUNT.CMD_PING))
                    {
                        if (bodyObj == null)
                            Debuger.Log("recvmsg {0} {1}  isResponse: {2}  errorCode: {3}  errorMsg: {4} \n ", message.Module, message.Command, message.IsResponse(), errorCode, errorMsg);
                        else
                        {
                            if (errorCode != 0)
                            {
                                Debuger.Log("recvmsg {0} {1}  isResponse: {2}  errorCode: {3}  errorMsg: {4} ", message.Module, message.Command, message.IsResponse(), errorCode, errorMsg);
                            }
                            else
                            {
                                Debuger.Log("recvmsg {0} {1} {2}", message.Module, message.Command, LitJson.JsonMapper.ToJson(bodyObj, s_prettyPrint));
                            }
                        }
                    }   
#endif
                }
                catch (Exception err)
                {
                    Debuger.LogError("解析网络数据失败，模块：{0} 命令：{1}，错误：\r\n{2}", module, command, err);

                    errorCode = RESULT_CODE.PARSE_ERROR;
                    errorMsg = null;
                    bodyObj = null;
                }

                if (!isReponse)
                {
                    if (cmdDef.hasErrorCode)
                    {
                        Debuger.LogError("消息处理方法要回复类消息，但下发的不是回复类消息，模块：{0} 命令：{1}", module, command); 
                        return;
                    }
                }
                else
                {
                    //如果是回复类消息，错误码不为0（也就是有错误），但消息处理方法又不接收错误码，那就这里通用处理，提示一下就行了
                    //什么？消息体有内容要处理？那就设置hasErrorCode为true吧
                    if (errorCode != 0 && !cmdDef.hasErrorCode)
                    {
                        if (string.IsNullOrEmpty(errorMsg))
                            errorMsg = ErrorCodeCfg.GetErrorDesc(module, errorCode, errorMsg);
                        UIMessage.Show(errorMsg);
                        if (errorCode > 0)
                            Debuger.Log("Module:{0}, Command:{1}, errorCode:{2}, errorMsg:{3}", module, command, errorCode, errorMsg);
                        else
                            Debuger.LogError("Module:{0}, Command:{1}, errorCode:{2}, errorMsg:{3}", module, command, errorCode, errorMsg);
                        return;
                    }
                }

                try
                {
                    cmdDef.Process(errorCode, errorMsg, bodyObj);
                }
                catch (Exception err)
                {
                    //Debuger.LogError("执行网络数据处理函数失败，模块：{0} 命令：{1}，错误：\r\n{2}", module, command, err);
                    throw err;
                }
            }
            finally
            {
                //放回对象池 
                Message.put(message);
            }
        }
        public Message MessageSend(byte module, int command, object obj)
        {
            ProtocolCoder.instance.Indent = 0;
            ProtocolCoder.instance.Log.Remove(0, ProtocolCoder.instance.Log.Length);
#if UNITY_EDITOR
            if (!(module == MODULE.MODULE_ACCOUNT && command == MODULE_ACCOUNT.CMD_PING))
            {
                if (obj == null)
                    ProtocolCoder.instance.Log.AppendFormat("sendmsg {0} {1} null", module, command);
                else
                    ProtocolCoder.instance.Log.AppendFormat("sendmsg {0} {1} {2}", module, command, LitJson.JsonMapper.ToJson(obj, false));
            }            
#endif

            TimeCheck check = new TimeCheck();
            Message msgObj = Message.NewRequest(module, command, obj);
            if (msgObj == null)
            {
                return null;
            }
            else
            {
#if UNITY_EDITOR
                if (!(module == MODULE.MODULE_ACCOUNT && command == MODULE_ACCOUNT.CMD_PING))
                {
                    ProtocolCoder.instance.Log.AppendFormat("\n耗时:{0}", check.delayMS);
                    Debuger.Log(ProtocolCoder.instance.Log.ToString());
                }
#endif
                return msgObj;
            }
        }

        public Message MessageSendJson(byte module, int command, string jsonStr)
        {
#if UNITY_EDITOR
            if (!(module == MODULE.MODULE_ACCOUNT && command == MODULE_ACCOUNT.CMD_PING))
            {
                Debuger.Log("sendmsg {0} {1} {2}", module, command, jsonStr);
            }
#endif

            TimeCheck check = new TimeCheck();
            Message msgObj = Message.NewRequestWithJson(module, command, jsonStr);
            if (msgObj == null)
            {
                return null;
            }
            else
            {
                return msgObj;
            }
        }
        #endregion

        #region Static Methods
        #endregion

        #region Private Methods
        #endregion

        public void SetCommandRegister(System.Func<int, int, CommandDefinition> action)
        {
            m_commandRegister = action;
        }

        public void SetOnConnectOK(System.Action callback)
        {
            m_onConnectOK = callback;
        }

        public void SetOnConnectFail(System.Action callback)
        {
            m_onConnectFail = callback;
        }

        public void SetOnConnError(System.Action<SocketError, string> callback)
        {
            m_onConnError = callback;
        }

        public void SetFilter(System.Func<int, int, bool> callback)
        {
            m_msgFilter = callback;
        }
    }
}
