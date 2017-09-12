using System;
//using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using System.IO;

namespace Encrypt
{
    public class Tea16
    {
        public const bool ENABLE_ENCRYPT = false;   //TODO 正式发布开启加密

        // 默认的密码
        private static uint[] default_key = new uint[]
        {
            0x3687C5E3,
            0xB7EF3327,
            0xE3791011, 
            0x84E2D3BC
        };

        private static int _readInt32(byte[] data, int offset)
        {
            int i = ((data[offset + 3] & 0xFF) << 0)
                + ((data[offset + 2] & 0xFF) << 8)
                + ((data[offset + 1] & 0xFF) << 16)
                + ((data[offset + 0] & 0xFF) << 24);

            return i;
        }

        private static void _writeInt32(int value, byte[] data, int offset)
        {
            data[offset + 3] = (byte)(value >> 0);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 0] = (byte)(value >> 24);
        }

        private static void _encrypt(uint[] v, uint[] k)
        {
            uint v0 = v[0], v1 = v[1], sum = 0, i;           /* set up */
            uint delta = 0x9e3779b9;                     /* a key schedule constant */
            uint k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];   /* cache key */
            for (i = 0; i < 16; i++)
            {
                /* basic cycle start */
                sum += delta;
                v0 += ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >> 5) + k1);
                v1 += ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >> 5) + k3);
            } /* end cycle */

            v[0] = v0;
            v[1] = v1;
        }

        private static void _decrypt(uint[] v, uint[] k)
        {
            uint v0 = v[0], v1 = v[1], sum = 0xE3779B90, i;  /* set up */
            uint delta = 0x9e3779b9;                     /* a key schedule constant */
            uint k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];   /* cache key */
            for (i = 0; i < 16; i++)
            {
                /* basic cycle start */
                v1 -= ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >> 5) + k3);
                v0 -= ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >> 5) + k1);
                sum -= delta;
            } /* end cycle */

            v[0] = v0;
            v[1] = v1;
        }

        private static void _processLeftBytes(byte[] bytes, int start, int len, uint[] keys)
	    {
		    for (int i = 0; i < len; ++i)
		    {
			    int index = start + i;
			    byte b = bytes[index];
                uint k = (byte)keys[i % 4];
			    b = (byte)(b ^ k);
			    bytes[index] = b;
		    }
	    }

        public static void EncryptInplace(byte[] src, int start = 0, int length = -1, uint[] k = null)
        {
            if (!ENABLE_ENCRYPT)
                return;

            if (k == null)
                k = default_key;
            if (length < 0)
                length = src.Length;

            int offset = start;
            int count = length / 8;
            uint[] v = new uint[2] { 0, 0 };
            for (int i = 0; i < count; ++i)
            {
                v[0] = (uint)_readInt32(src, offset);
                v[1] = (uint)_readInt32(src, offset + 4);

                _encrypt(v, k);

                _writeInt32((int)v[0], src, offset);
                _writeInt32((int)v[1], src, offset + 4);

                offset += 8;
            }
            _processLeftBytes(src, start + count * 8, length % 8, k);
        }

        public static byte[] Encrypt(byte[] src, int start = 0, int length = -1, uint[] k = null)
        {
            src = src.Clone() as byte[];
            EncryptInplace(src, start, length, k);
            return src;
        }

        public static byte[] EncryptString(string src, uint[] k = null)
        {
            byte[] result = Util.StringToBytes(src);
            EncryptInplace(result, 0, result.Length, k);
            return result;
        }

        public static void DecryptInplace(byte[] src, int start = 0, int length = -1, uint[] k = null)
        {
            if (!ENABLE_ENCRYPT)
                return;

            if (k == null)
                k = default_key;
            if (length < 0)
                length = src.Length;

            int offset = start;
            int count = length / 8;
            uint[] v = new uint[2] { 0, 0 };
            for (int i = 0; i < count; ++i)
            {
                v[0] = (uint)_readInt32(src, offset);
                v[1] = (uint)_readInt32(src, offset + 4);

                _decrypt(v, k);

                _writeInt32((int)v[0], src, offset);
                _writeInt32((int)v[1], src, offset + 4);

                offset += 8;
            }
            _processLeftBytes(src, start + count * 8, length % 8, k);
        }

        public static byte[] Decrypt(byte[] src, int start = 0, int length = -1, uint[] k = null)
        {
            src = src.Clone() as byte[];
            DecryptInplace(src, start, length, k);
            return src;
        }

        public static string DecryptString(byte[] src, uint[] k = null)
        {
            //克隆一下，不要破坏参数
            src = src.Clone() as byte[];
            DecryptInplace(src, 0, src.Length, k);
            return Util.BytesToString(src);
        }
    }
}