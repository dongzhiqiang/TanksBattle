#region Header
/**
 * 名称：发送数据线程
 
 * 日期：2015.11.3
 * 描述：
 *      
 * **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
namespace NetCore
{
    public partial class Connector : IoConnector
    {        
        void SendThread()
        {
            var tempBuffer = new IoBuffer(ONCE_SIZE);
            
            while (m_isIOThreadActive)
            {
                if (m_socket == null || !m_socket.Connected)
                {
                    Thread.Sleep(20);
                    continue;
                }

                if (m_sendMessageList.Count == 0)
                {
                    Thread.Sleep(20);
                    continue;
                }

                lock (m_sendMessageList)
                {
                    m_sendTempMsgList.AddRange(m_sendMessageList);
                    m_sendMessageList.Clear();
                }
                
                while (m_sendTempMsgList.Count > 0)
                {
                    try
                    {
                        tempBuffer.Reset();
                        encode(m_sendTempMsgList[0], tempBuffer);
                    }
                    catch (System.Exception e)
                    {
                        Util.SafeLogError(e.Message + "\n" + e.StackTrace);
                        continue;
                    }

                    try
                    {
                        while (tempBuffer.ReadSize > 0)
                        {
                            int sendSize = Math.Min(tempBuffer.ReadSize, ONCE_SIZE);

                            SocketError error = SocketError.Success;
                            sendSize = m_socket.Send(tempBuffer.Buffer, tempBuffer.ReadPos, sendSize, SocketFlags.None, out error);
                            if (error != SocketError.Success)
                            {
                                AddError(error, "m_socket.Send error");
                                return;
                            }

                            tempBuffer.ReadPos += sendSize;
                        }
                        Message.put(m_sendTempMsgList[0]);
                        m_sendTempMsgList.RemoveAt(0);
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        AddError(e.SocketErrorCode, e.Message + "\n" + e.StackTrace);
                        return;
                    }
                    catch (Exception e)
                    {
                        AddError(SocketError.SocketError, e.Message + "\n" + e.StackTrace);
                        return;
                    }
                }

                //发送速度不要太快
                Thread.Sleep(5);
            }
        }

        void encode(Message inObj, IoBuffer outBuffer)
        {
            //包头
            outBuffer.Write(Message.PACKAGE_INDETIFIER);                //包标识
            int writePosOfLen = outBuffer.WritePos;                     //保存包长度写入位置
            outBuffer.Write((int)0);                                    //包长度，先点位
            int writePosOfChkSum = outBuffer.WritePos;                  //保存包检验码写入位置
            outBuffer.Write((int)0);                                    //包检验码，先占位

            //包体
            int writePosOfBody = outBuffer.WritePos;                    //保存消息体写入位置
            inObj.WriteBytes(outBuffer);                                //写消息体
            int writePosNow = outBuffer.WritePos;                       //获取现在的写入位置

            //回写长度
            int msgBodyLen = writePosNow - writePosOfBody;              //消息体写入的长度
            outBuffer.WriteBack(msgBodyLen + 8, writePosOfLen);         //重新写包长度，消息体N字节 + 长度字段4字节 + 检验码字段4字节

            //加密
            Encrypt.Tea16.EncryptInplace(outBuffer.Buffer, writePosOfBody, msgBodyLen);

            //检验和
            int hashCode = (int)Message.BPHash(outBuffer.Buffer, writePosOfBody, msgBodyLen);
            outBuffer.WriteBack(hashCode, writePosOfChkSum);            //写包检验和
        }

        void ResetEncode()
        {
        }
    }
}
