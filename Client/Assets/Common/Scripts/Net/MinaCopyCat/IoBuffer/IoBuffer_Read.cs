using System;
using System.Collections.Generic;

namespace NetCore
{
    // 消息流
    public partial class IoBuffer
    {
        int mReadMark =-1;
        public bool ReadBool()
        {
            return mBuffer[mReadPos++] == Byte_True; 
        }
        public byte ReadByte()
        {
            return mBuffer[mReadPos++];
        }
        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            Array.Copy(mBuffer, mReadPos, data, 0, count);
            mReadPos += count;
            return data;
        }
        public void ReadBytes(byte[] data, int offset,int count)
        {
            Array.Copy(mBuffer, mReadPos, data, offset, count);
            mReadPos += count;
        }
        public void Read(IoBuffer buffer,int len)
        {
            ReadBytes(buffer.Buffer, buffer.WritePos, len);
            buffer.WritePos += len;
        }
        public static short ReadInt16(byte[] data, int offset)
        {
            short i = (short)(((data[offset + 1] & 0xFF) << 0) + ((data[offset + 0] & 0xFF) << 8));
            return i;
        }
        public short ReadInt16()
        {
            short i = ReadInt16(mBuffer, mReadPos);
            mReadPos += 2;
            return i;
        }
        public static int ReadInt32(byte[] data, int offset)
        {
            int i = ((data[offset + 3] & 0xFF) << 0)
                + ((data[offset + 2] & 0xFF) << 8)
                + ((data[offset + 1] & 0xFF) << 16)
                + ((data[offset + 0] & 0xFF) << 24);
            return i;
        }
        public int ReadInt32()
        {
            int i = ReadInt32(mBuffer, mReadPos);
            mReadPos += 4;
            return i;
        }
        /// <summary>
        /// 读取整数，但不移动读取位置
        /// </summary>
        /// <returns></returns>
        public int PeekInt32()
        {
            return ReadInt32(mBuffer, mReadPos);
        }
        public long ReadInt64()
        {
            long l = ((mBuffer[mReadPos + 7] & (long)0xFF) << 0)
                    + ((mBuffer[mReadPos + 6] & (long)0xFF) << 8)
                    + ((mBuffer[mReadPos + 5] & (long)0xFF) << 16)
                    + ((mBuffer[mReadPos + 4] & (long)0xFF) << 24)
                    + ((mBuffer[mReadPos + 3] & (long)0xFF) << 32)
                    + ((mBuffer[mReadPos + 2] & (long)0xFF) << 40)
                    + ((mBuffer[mReadPos + 1] & (long)0xFF) << 48)
                    + ((mBuffer[mReadPos + 0] & (long)0xFF) << 56);
            //BitConverter.ToInt64(mBuffer, mReadPos)
            //Debuger.LogError("long reader：{0} {1} ", StringUtil.ToHexString(mBuffer, mReadPos, 8), l);
            mReadPos += 8;
            if (l < 0)
                l++;
            return l;
        }
        //public unsafe float ReadFloat()
        //{
        //    float f = BitConverter.ToSingle(mBuffer, mReadPos);
        //    mReadPos += 4;
        //    return f;
        //}
        public unsafe float ReadFloat()
        {
            int num =ReadInt32();
            return *(float*)(&num);
        }
        //public double ReadDouble()
        //{
        //    double d = BitConverter.ToDouble(mBuffer, mReadPos);
        //    mReadPos += 8;
        //    return d;
        //}

        public unsafe double ReadDouble()
        {
            long num = ReadInt64();
            return *(double*)(&num);
        }

        public string ReadStr()
        {
            int length = ReadInt32();
            mReadPos += length;
            return System.Text.Encoding.UTF8.GetString(mBuffer, mReadPos - length, length);
        }
        /// <summary>
        /// 可能先读取了长度，判断好buffer够不够后，继续读取字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ReadOnlyStr(int length)
        {
            mReadPos += length;
            return System.Text.Encoding.UTF8.GetString(mBuffer, mReadPos - length, length);
        }

        public void Mark()
        {
            mReadMark = mReadPos;
        }

        public void ResetMark()
        {
            mReadPos = mReadMark;
        }

        public void ResetRead()
        {
            mReadPos = 0;
        }

        public void Skip(int length)
        {
            mReadPos += length;
        }

        public void SkipAll()
        {
            mReadPos = mWritePos;
        }
    }
}