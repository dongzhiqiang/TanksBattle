using System;
using System.Collections.Generic;

namespace NetCore
{
    // 消息流
    public partial class IoBuffer
    {
        public const byte Byte_True = 1;
        public const byte Byte_False= 0;
        public void Write(bool value)
        {
            EnsureCapacity(1);
            mBuffer[mWritePos++] = value ? (byte)Byte_True : (byte)Byte_False;
        }
        public void Write(byte value)
        {
            EnsureCapacity(1);
            mBuffer[mWritePos++] = value;
        }
        public void Write(byte[] buffer)
        {
            EnsureCapacity(buffer.Length);
            Array.Copy(buffer, 0, mBuffer, mWritePos, buffer.Length);
            mWritePos += buffer.Length;
        }
        public void Write(byte[] data, int offset, int count)
        {
            EnsureCapacity(count);
            Array.Copy(data, offset, mBuffer, mWritePos, count);
            mWritePos += count;
        }
        public void Write(IoBuffer buffer, int len)
        {
            Write(buffer.Buffer, buffer.ReadPos, len);
            //多数情况下，没必要修改源Buffer的ReadPos
            //buffer.ReadPos += len;
        }
        public void Write(short value)
        {
            EnsureCapacity(2);
            mBuffer[mWritePos + 1] = (byte)(value >> 0);
            mBuffer[mWritePos + 0] = (byte)(value >> 8);
            mWritePos += 2;
        }
        public void Write(int value)
        {
            EnsureCapacity(4);
            mBuffer[mWritePos + 3] = (byte)(value >> 0);
            mBuffer[mWritePos + 2] = (byte)(value >> 8);
            mBuffer[mWritePos + 1] = (byte)(value >> 16);
            mBuffer[mWritePos + 0] = (byte)(value >> 24);
            mWritePos += 4;
        }

        public void WriteBack(int value,int writePos)
        {
            mBuffer[writePos + 3] = (byte)(value >> 0);
            mBuffer[writePos + 2] = (byte)(value >> 8);
            mBuffer[writePos + 1] = (byte)(value >> 16);
            mBuffer[writePos + 0] = (byte)(value >> 24);
        }
        public void Write(long value)
        {
            EnsureCapacity(8);

            mBuffer[mWritePos + 7] = (byte)(int)(value >> 0);
            mBuffer[mWritePos + 6] = (byte)(int)(value >> 8);
            mBuffer[mWritePos + 5] = (byte)(int)(value >> 16);
            mBuffer[mWritePos + 4] = (byte)(int)(value >> 24);
            mBuffer[mWritePos + 3] = (byte)(int)(value >> 32);
            mBuffer[mWritePos + 2] = (byte)(int)(value >> 40);
            mBuffer[mWritePos + 1] = (byte)(int)(value >> 48);
            mBuffer[mWritePos + 0] = (byte)(int)(value >> 56);//08D2D49B4F89B189
            //byte[] bb = BitConverter.GetBytes(value);//和上面算出来刚好相反 89B1894F9BD4D208
            //mBuffer[mWritePos + 7] = bb[0];
            //mBuffer[mWritePos + 6] = bb[1];
            //mBuffer[mWritePos + 5] = bb[2];
            //mBuffer[mWritePos + 4] = bb[3];
            //mBuffer[mWritePos + 3] = bb[4];
            //mBuffer[mWritePos + 2] = bb[5];
            //mBuffer[mWritePos + 1] = bb[6];
            //mBuffer[mWritePos + 0] = bb[7];
            //Debuger.LogError("long write：{0}", StringUtil.ToHexString(mBuffer, mWritePos, 8));
            //Debuger.LogError("long write2：{0}", StringUtil.ToHexString(bb));
            mWritePos += 8;
        }
        //public  void Write(float value)
        //{
        //    ensureCapacity(4);
        //    byte[] bb = BitConverter.GetBytes(value);
        //    mBuffer[mWritePos + 3] = bb[0];
        //    mBuffer[mWritePos + 2] = bb[1];
        //    mBuffer[mWritePos + 1] = bb[2];
        //    mBuffer[mWritePos + 0] = bb[3];
        //    mWritePos += 4;
        //}
        public unsafe void Write(float value)
        {
            Write(*(int*)(&value));
        }
        //public void Write(double value)
        //{
        //    ensureCapacity(8);
        //    byte[] bb = BitConverter.GetBytes(value);
        //    mBuffer[mWritePos + 7] = bb[0];
        //    mBuffer[mWritePos + 6] = bb[1];
        //    mBuffer[mWritePos + 5] = bb[2];
        //    mBuffer[mWritePos + 4] = bb[3];
        //    mBuffer[mWritePos + 3] = bb[4];
        //    mBuffer[mWritePos + 2] = bb[5];
        //    mBuffer[mWritePos + 1] = bb[6];
        //    mBuffer[mWritePos + 0] = bb[7];
        //    mWritePos += 8;
        //}
        public unsafe void Write(double value)
        {
            Write(*(long*)(&value));
        }

        public int Write(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                Debuger.LogError("不能write空的字符串");
                return 0;
            }

            int length = System.Text.Encoding.UTF8.GetByteCount(value);
            Write(length);
            EnsureCapacity(length);
            System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, mBuffer, mWritePos);
            mWritePos += length;
            return length;
        }
        /// <summary>
        /// 写入字符串，字符串长度值另外写入
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int WriteOnlyStr(string value, int length = -1)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debuger.LogError("不能write空的字符串");
                return 0;
            }

            length = length >= 0 ? length : System.Text.Encoding.UTF8.GetByteCount(value);
            EnsureCapacity(length);
            System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, mBuffer, mWritePos);
            mWritePos += length;
            return length;
        }
        //// 移动数据块,从offset位置移到初始位置，offset后面的数据保留，其他的全部丢弃
        //public void Move(int offset)
        //{
        //    Array.Copy(mBuffer, offset, mBuffer, 0, WritePos - offset);
        //    WritePos -= offset; // 写入位置转换下
        //    ReadPos = 0; // 读取位置为0
        //}        
    }
}