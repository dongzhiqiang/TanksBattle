#region Header
/**
 * 名称：Command
 
 * 日期：2015.11.3
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NetCore
{
    /**
     * 信息头<br/>
     * 格式:[长度][格式][状态][序号][会话][指令][模块号]
     * 
     */
    public class Command 
    {
        #region Fields
        /** 指令所属模块 */
        private byte module;
        /** 指令标识 */
        private int command;
        #endregion


        #region Properties
        public byte Module { get{return module;}}
        public int CMD { get { return command; } }
        #endregion


        #region Constructors
        public Command()
        {
    
        }
        #endregion

        

        #region Static Methods
        /**
	     * 创建通信指令对象实例
	     * @param command 指令标识
	     * @param modules 所属模块标识
	     * @return
	     */
	    public static Command valueOf(byte module,int command) {
		    Command result = new Command();
            result.module = module;
		    result.command = command;
		    return result;
	    }

        public static Command valueOf(IoBuffer stream)
        {
            Command result = new Command();
            result.command = stream.ReadInt32();
            result.module = stream.ReadByte();
            
            
            
            return result;
        }
        #endregion



        #region Private Methods

        #endregion

        public int getModule()
        {
            return module;
        }

        public int getCommand()
        {
            return command;
        }

        public override string ToString()
        {
            return string.Format("M:{0} C:{1}",module,command);
        }

        /**
         * 将当前对象转换为通信表示格式
         * @return
         */
        public void  write(IoBuffer stream)
        {
            stream.Write(command);
            stream.Write(module);
            
        }

    }
}