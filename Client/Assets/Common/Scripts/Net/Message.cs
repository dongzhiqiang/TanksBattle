#region Header
/**
 * 名称：Message
 * 作者：XiaoLizhi
 * 日期：2016.01.26
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;

namespace NetCore
{
    /**
     * 包：包头+包体
     * 包头：包标识（4字节）+包长度（4字节）+检验码（4字节）
     * 包体：
     *       请求：模块号（4字节）+命令号（4字节）+包选项（4字节）+业务数据（N字节）
     *       带错误消息回应：模块号（4字节）+命令号（4字节）+包选项（4字节）+错误码（4字节）+错误消息长度（4字节）+错误消息（M字节）+业务数据（N字节）
     *       无错误消息回应：模块号（4字节）+命令号（4字节）+包选项（4字节）+错误码（4字节）+业务数据（N字节）
     *       注：回应类消息，只限服务器下发用，客户端不要用
     */
    public class Message
    {
        #region Fields
        //对象池
        private static LinkedList<Message> s_pool = new LinkedList<Message>();
        private static int s_poolCounter = 0;

        /** 包标识 */
        public const int PACKAGE_INDETIFIER = -1;//0xFFFFFFFF
        /** 包头部分字段长度（包标识+包长度字段） */
        public const int PACKAGE_PRE_LENGTH = 8;

        /** 选项常量 */
        public const bool USE_JSON_FORMAT = true;
        public const bool ENABLE_COMPRESS = false;  //TODO 正式发布开启压缩
        public const int JSON_COMPRESS_LEN = 1024;
        public const int MSG_FLAG_JSON = 0x00000001;   //是否JSON，否则是自定义的二进制
        public const int MSG_FLAG_COMPRESS = 0x00000002;   //是否压缩过（只对JSON有效）
        public const int MSG_FLAG_RESPONSE = 0x00000004;   //是否为回应类消息对象，如果是，则消息体前面有错误码，还可能有错误消息
        public const int MSG_FLAG_ERR_MSG = 0x00000008;   //是否带有错误消息字符串，一般只用于回应类消息

        private int m_module = 0;
        private int m_command = 0;
        private int m_flag = 0;
        private int m_code = 0;
        private string m_msg = null;
        private IoBuffer m_body = null;
        #endregion

        #region Properties
        public int Module
        {
            get { return m_module; }
            set { m_module = value; }
        }
        public int Command
        {
            get { return m_command; }
            set { m_command = value; }
        }
        public int Flag
        {
            get { return m_flag; }
            //flag不让直接修改
        }
        public int ErrorCode
        {
            get { return m_code; }
            //code不让直接修改
        }
        public string ErrorMsg
        {
            get { return m_msg; }
            //msg不让直接修改
        }
        public IoBuffer Body
        {
            get { return m_body; }
            //body不让直接修改
        }
        #endregion


        #region Constructors
        public Message()
        {
        }
        #endregion

        #region Static Methods
        public static Message NewRequest(int module, int command, object body = null)
        {
            var msg = get();
            msg.m_module = module;
            msg.m_command = command;
            if (msg.SetRequestBodyObject(body))
            {
                return msg;
            }
            else
            { 
                put(msg);
                return null;
            }
        }
        public static Message NewRequestWithJson(int module, int command, string jsonStr = null)
        {
            var msg = get();
            msg.m_module = module;
            msg.m_command = command;
            if (msg.SetRequestJsonString(jsonStr))
            {
                return msg;
            }
            else
            {
                put(msg);
                return null;
            }
        }
        public static Message NewResponse(int module, int command, int code, string errMsg, object body = null)
        {
            var msg = get();
            msg.m_module = module;
            msg.m_command = command;
            if (msg.SetResponseWithMsg(code, errMsg, body))
            {
                return msg;
            }
            else
            {
                put(msg);
                return null;
            }
        }
        public static Message NewResponseWithRequest(Message reqMsg, int code, string errMsg, object body = null)
        {
            var msg = get();
            msg.m_module = reqMsg.Module;
            msg.m_command = reqMsg.Command;
            if (msg.SetResponseWithMsg(code, errMsg, body))
            {
                return msg;
            }
            else
            {
                put(msg);
                return null;
            }
        }
        public static Message FromIOBuffer(IoBuffer inBuf, int dataLen)
        {
            var msg = get();
            do
            {
                if (dataLen < 12)
                    break;
                else
                    dataLen -= 12;
                msg.m_module = inBuf.ReadInt32();
                msg.m_command = inBuf.ReadInt32();
                msg.m_flag = inBuf.ReadInt32();
                //如果是回应消息，就要提取错误码
                if ((msg.m_flag & MSG_FLAG_RESPONSE) != 0)
                {
                    if (dataLen < 4)
                        break;
                    else
                        dataLen -= 4;
                    msg.m_code = inBuf.ReadInt32();
                }
                else
                {
                    msg.m_code = 0;
                }
                //如果有错误消息，就要提取错误消息
                if ((msg.m_flag & MSG_FLAG_ERR_MSG) != 0)
                {
                    if (dataLen < 4)
                        break;
                    else
                        dataLen -= 4;
                    var len = inBuf.ReadInt32();
                    if (len < 0 || dataLen < len)
                        break;
                    else
                        dataLen -= len;
                    msg.m_msg = inBuf.ReadOnlyStr(len);
                }
                else
                {
                    msg.m_msg = null;
                }
                //有消息体？
                if (dataLen > 0)
                {
                    var bodyBuf = new IoBuffer(dataLen);
                    inBuf.Read(bodyBuf, dataLen);
                    msg.m_body = bodyBuf;
                }
                else
                {
                    msg.m_body = null;
                }
                return msg;
            }
            while (false);

            //对象放回池里
            put(msg);
            //能到这里来？那说明前面遇到break，说明数据不够长
            Util.SafeLogError("Message~fromIOBuffer数据不够长");
            //跳过还未读的
            inBuf.Skip(dataLen);
            //返回null
            return null;
        }
        public static int BPHash(byte[] bytes, int offset, int len)
        {
            long hash = 0;
            for (int i = offset; i < (offset + len); ++i)
            {
                //注意这里要转成有符号的计算，不然计算结果和服务端不同   
                //当为byte的时候 hash=0000185000000411 bytes[i]=82 =>hash<<7^bytes[i]=000c280000020802
                //当为sbyte的时候 hash=0000185000000411 bytes[i]=82 =>hash<<7^(sbyte)bytes[i]=fff3d7fffffdf702
                hash = hash << 7 ^ bytes[i];
            }
            return (int)hash;
        }
        public static void put(Message m)
        {
            lock (s_pool)
            {
                //比较大的放后面，优先拿出来
                if (m.Body != null && m.Body.Buffer.Length > 1024)
                    s_pool.AddLast(m);
                else
                    s_pool.AddFirst(m);
            }
        }
        public static void put(List<Message> l)
        {
            if (l == null || l.Count <= 0)
                return;
            for (var i = 0; i < l.Count; ++i)
            {
                if (l[i] != null)
                    put(l[i]);
            }
            l.Clear();
        }

        private static Message get()
        {
            lock (s_pool)
            {
                if (s_pool.Count > 0)
                {
                    Message m = s_pool.Last.Value;
                    s_pool.RemoveLast();
                    return m;
                }
                else
                {
                    ++s_poolCounter;
                    if (s_poolCounter > 20)
                    {
                        Util.SafeLogError("消息对象池的消息过多，可能有泄露");
                    }
                    return new Message();
                }
            }
        }
        #endregion

        public bool IsJsonBody()
        {
            return (m_flag & MSG_FLAG_JSON) != 0;
        }

        public bool IsResponse()
        {
            return (m_flag & MSG_FLAG_RESPONSE) != 0;
        }

        public void WriteBytes(IoBuffer outBuf)
        {
            outBuf.Write(m_module);
            outBuf.Write(m_command);
            outBuf.Write(m_flag);
            //如果是回应消息，就要写入错误码
            if ((m_flag & MSG_FLAG_RESPONSE) != 0)
            {
                outBuf.Write(m_code);
            }
            //如果有错误消息，就要写入错误消息
            if ((m_flag & MSG_FLAG_ERR_MSG) != 0)
            {
                var msg = m_msg;
                if (msg == null)
                    m_msg = msg = "";
                outBuf.Write(msg);
            }
            //如果有消息体就写入消息体
            if (m_body != null)
            {
                outBuf.Write(m_body, m_body.ReadSize);
            }
        }

        public object GetBodyObject(Type type)
        {
            try
            {
                var body = m_body;
                if (body == null)
                    return null;
                else
                {
                    object bodyObj;
                    //不是JSON？反串行化吧
                    if ((m_flag & MSG_FLAG_JSON) == 0)
                    {
                        body.ResetRead();
                        ProtocolCoder.instance.Decode(body, type, out bodyObj);
                        body.ResetRead();
                    }
                    else
                    {
                        string str;
                        //没压缩过，就直接把数据转成字符串，并解析成对象
                        if ((m_flag & MSG_FLAG_COMPRESS) == 0)
                            str = System.Text.Encoding.UTF8.GetString(body.Buffer);
                        //压缩过？解压吧，默认直接解压成字符串，并解析成对象
                        else
                            str = System.Text.Encoding.UTF8.GetString(Snappy.Sharp.Snappy.Uncompress(body.Buffer));
                        bodyObj = LitJson.JsonMapper.ToObject(type, str);
                    }
                    return bodyObj;
                }
            }
            catch (Exception err)
            {
                //Util.SafeLogError("Message~getBodyObject发生错误：\r\n{0}", err.StackTrace);
                //return null;
                //直接抛出吧
                throw err;
            }
        }

        public bool SetRequestBodyObject(object obj)
        {
            try
            {
                //如果是null，那就直接body为null
                if (obj == null)
                {
                    m_body = null;
                    m_msg = null;
                    m_code = 0;
                    m_flag = 0;
                    return true;
                }

                if (USE_JSON_FORMAT)
                {
                    var jsonStr = LitJson.JsonMapper.ToJson(obj);
                    var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonStr);
                    //如果要压缩，且达到要压缩的字符数，那就压缩，compress可以直接得到Buffer，再直接转为IOBuffer
                    if (jsonStr.Length > JSON_COMPRESS_LEN && ENABLE_COMPRESS)
                    {
                        //压缩并直接转为IOBuffer
                        m_body = new IoBuffer(Snappy.Sharp.Snappy.Compress(jsonBytes));
                        //错误消息为空
                        m_msg = null;
                        //错误码为空
                        m_code = 0;
                        //加上标记
                        m_flag = MSG_FLAG_JSON | MSG_FLAG_COMPRESS;
                    }
                    //否则那就把字符串转为IOBuffer吧
                    else
                    {
                        //把字符串转成IOBuffer
                        m_body = new IoBuffer(jsonBytes);
                        //错误消息为空
                        m_msg = null;
                        //错误码为空
                        m_code = 0;
                        //加上标记
                        m_flag = MSG_FLAG_JSON;
                    }
                }
                else
                {
                    if (m_body == null)
                        m_body = new IoBuffer(256);
                    else
                        m_body.Reset();

                    //错误消息为空
                    m_msg = null;
                    //错误码为空
                    m_code = 0;
                    //设置标记
                    m_flag = 0;
                    ProtocolCoder.instance.Encode(m_body, obj);
                }

                return true;
            }
            catch (Exception err)
            {
                Util.SafeLogError("Message~setRequestBodyObject发生错误{0}", err.StackTrace);
                m_body = null;
                m_msg = null;
                m_code = 0;
                m_flag = 0;
                return false;
            }
        }

        public bool SetRequestJsonString(string jsonStr)
        {
            try
            {
                //如果是null，那就直接body为null
                if (jsonStr == null)
                {
                    m_body = null;
                    m_msg = null;
                    m_code = 0;
                    m_flag = 0;
                    return true;
                }

                var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonStr);
                //如果要压缩，且达到要压缩的字符数，那就压缩，compress可以直接得到Buffer，再直接转为IOBuffer
                if (jsonStr.Length > JSON_COMPRESS_LEN && ENABLE_COMPRESS)
                {
                    //压缩并直接转为IOBuffer
                    m_body = new IoBuffer(Snappy.Sharp.Snappy.Compress(jsonBytes));
                    //错误消息为空
                    m_msg = null;
                    //错误码为空
                    m_code = 0;
                    //加上标记
                    m_flag = MSG_FLAG_JSON | MSG_FLAG_COMPRESS;
                }
                //否则那就把字符串转为IOBuffer吧
                else
                {
                    //把字符串转成IOBuffer
                    m_body = new IoBuffer(jsonBytes);
                    //错误消息为空
                    m_msg = null;
                    //错误码为空
                    m_code = 0;
                    //加上标记
                    m_flag = MSG_FLAG_JSON;
                }

                return true;
            }
            catch (Exception err)
            {
                Util.SafeLogError("Message~SetRequestJsonString发生错误{0}", err.StackTrace);
                m_body = null;
                m_msg = null;
                m_code = 0;
                m_flag = 0;
                return false;
            }
        }

        public bool SetResponseData(int code, object obj)
        {
            return SetResponseWithMsg(code, null, obj);
        }

        public bool SetResponseWithMsg(int code, string errMsg, object obj)
        {
            try
            {
                var errMsgFlag = errMsg == null ? 0 : MSG_FLAG_ERR_MSG;
                //如果是null，那就直接body为null
                if (obj == null)
                {
                    m_body = null;
                    m_msg = errMsg;
                    m_code = code;
                    m_flag = MSG_FLAG_RESPONSE | errMsgFlag;
                    return true;
                }

                if (USE_JSON_FORMAT)
                {
                    var jsonStr = LitJson.JsonMapper.ToJson(obj);
                    var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonStr);
                    //如果要压缩，且达到要压缩的字符数，那就压缩，compress可以直接得到Buffer，再直接转为IOBuffer
                    if (jsonStr.Length > JSON_COMPRESS_LEN && ENABLE_COMPRESS)
                    {
                        //压缩并直接转为IOBuffer
                        m_body = new IoBuffer(Snappy.Sharp.Snappy.Compress(jsonBytes));
                        //添加错误消息
                        m_msg = errMsg;
                        //添加错误码
                        m_code = code;
                        //加上标记
                        m_flag = MSG_FLAG_RESPONSE | MSG_FLAG_JSON | MSG_FLAG_COMPRESS | errMsgFlag;
                    }
                    //否则那就把字符串转为IOBuffer吧
                    else
                    {
                        //把字符串转成IOBuffer
                        m_body = new IoBuffer(jsonBytes);
                        //添加错误消息
                        m_msg = errMsg;
                        //添加错误码
                        m_code = code;
                        //加上标记
                        m_flag = MSG_FLAG_RESPONSE | MSG_FLAG_JSON | errMsgFlag;
                    }
                }
                else
                {
                    if (m_body == null)
                        m_body = new IoBuffer(256);
                    else
                        m_body.Reset();

                    //添加错误消息
                    m_msg = errMsg;
                    //添加错误码
                    m_code = code;
                    //设置标记
                    m_flag = MSG_FLAG_RESPONSE | errMsgFlag;
                    ProtocolCoder.instance.Encode(m_body, obj);
                }

                return true;
            }
            catch (Exception err)
            {
                Util.SafeLogError("Message~setResponseWithMsg发生错误：{0}", err.StackTrace);
                m_body = null;
                m_msg = null;
                m_code = 0;
                m_flag = 0;
                return false;
            }
        }
    }
}