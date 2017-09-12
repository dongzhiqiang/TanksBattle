using System;
using System.Collections.Generic;

namespace NetCore
{
    // 消息流
    public partial class IoBuffer
    {
        public int WriteRemain { get { return mSize - mWritePos; } }
        public int ReadPos { get { return mReadPos; } set { mReadPos = value; } }
        public int WritePos { get { return mWritePos; } set { mWritePos = value; } }
        public int ReadSize { get { return WritePos - ReadPos; } }

        protected byte[] mBuffer; // 缓存
        protected int mSize = 0;
        protected int mReadPos = 0; // 当前读取的位置
        protected int mWritePos = 0; // 当前写入的位置

        // 缓存
        public byte[] Buffer { get { return mBuffer; } }

        public IoBuffer(byte[] data)
        {
            mBuffer = data;
            if (mBuffer != null)
            {
                mSize = mBuffer.Length;
                mWritePos = mBuffer.Length;
            }
                
        }

        public IoBuffer(int capacity)
        {
            mSize = capacity;
            mBuffer = new byte[capacity];
        }

        public bool isCanRead(int size)
        {
            if (mReadPos + size > mWritePos)
                return false;

            return true;
        }

        public void EnsureCapacity(int length)
        {
            if (length > WriteRemain)
            {
                int newsize = length > mSize ? (length + mSize) : (mSize * 2);

                Array.Resize<byte>(ref mBuffer, newsize);
                mSize = newsize;
            }
        }

        public void Reset()
        {
            mReadPos = 0; // 当前读取的位置
            mWritePos = 0; // 当前写入的位置
        }

        //移动到0
        public void MoveToFirst()
        {
            if (mReadPos == mWritePos)
            {
                Reset();
                return;
            }
            Array.Copy(mBuffer, ReadPos, mBuffer, 0, ReadSize);
            mWritePos = ReadSize;
            mReadPos = 0;
        }
    }
}